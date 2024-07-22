using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace Player
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        // Debug Settings
        public bool showDebugMessage = false;

        public Collider2D playerCollider;

        // Player's main rigid body
        private Rigidbody2D _rigid;

        // Environment Awareness
        private EnvAware _envAware;
        private Movements _movements;

        // Player Movement
        [Header("Basic Player Movement")] public float moveSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_MOVE_SPEED;
        public float jumpSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_JUMP_SPEED;
        public float rushSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_RUSH_SPEED;

        [Header("Detailed Player Movement")]
        //public float rushCD = PlayerProperties.Instance.PLAYER_DEFAULT_RUSH_CD;
        public bool haveRushCD = false;

        public float rushCD = 2f;
        public float rushDur = 2f;

        public float maxAllowCoyoteTime = PlayerProperties.Instance.PLAYER_MAX_ALLOW_COYOTE_TIME; //离开地面后的最大土狼时间
        public float maxGravityScale = PlayerProperties.Instance.PLAYER_MAX_GRAVITY_SCALE;

        // Static Player Properties
        [Header("Ground Detection")]
        public string groundLayerMask = PlayerProperties.Instance.PLAYER_GROUND_LAYER_MASK; // Ground Detection

        public float groundDetectionRaycastDistance =
            PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RAYCAST_MAX_DISTANCE;

        public float raycastUpPosition = PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RAYCAST_UP_POSITION;
        public float raycastMaxDistance = PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RAYCAST_MAX_DISTANCE;

        [Header("Player Life")] public int maxLife = PlayerProperties.Instance.PLAYER_DEFAULT_MAX_LIFE;
        public float fallDamageSpeedThreshold = PlayerProperties.Instance.PLAYER_FALL_DAMAGE_THRESHOLD;

        // Dynamic Player variables.
        private bool _isFaceTowardsRight = true;
        private bool _isRushing = false;
        private bool _isSpaceGravity = false;
        private float _timeSinceLastRushed = 0;
        private float _remainingAllowCoyoteTime; //土狼
        private int _currentLife;

        // Delegates

        // Initialize parameters.
        void InitParams()
        {
            _envAware = new EnvAware();
            _movements = new Movements(_envAware, playerCollider, transform, moveSpeed);
            
            _rigid = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            //_timeAfterLastRushed = rushCD;
            _remainingAllowCoyoteTime = 0;
            _isFaceTowardsRight = true; // Initialize player face to right.
            _isSpaceGravity = false; // Initialize player gravity.
            _currentLife = maxLife; // Maximizes player life.
        }

        // Initialize event listeners.
        private void InitDelegates()
        {
            EventCenterManager.Instance.AddEventListener<bool>(GameEvent.PlayerEnterSpace, TriggerSpaceGravityRegion);
            EventCenterManager.Instance.AddEventListener<int>(GameEvent.PlayerGetHurt, GetHurt);
            EventCenterManager.Instance.AddEventListener(GameEvent.PlayerDie, Die);
        }

        private void Start()
        {
            InitParams(); // Initialize player properties.
            InitDelegates(); // Initialize event listeners.
        }

        private void Update()
        {
            Move();
            AdjustPropertyConstantly();
            //PassiveSenseEnvironment();
        }


        #region Movement

        /* Player movements.*/
        private void Move()
        {
            /* Get WASD inputs */
            // TODO: Get these inputs into configuration file.
            var xInput = Input.GetAxis("Horizontal");
            var jumpInput = Input.GetKey(KeyCode.Space);
            var flyInput = Input.GetKeyDown(KeyCode.Space);
            var rushInput = Input.GetKey(KeyCode.R);
            var interactInput = Input.GetKey(KeyCode.F);

            // Control Facing Direction
            _isFaceTowardsRight = xInput != 0 ? xInput >= 0 : _isFaceTowardsRight;

            /* Calculate player movements based on inputs.*/
            // Horizontal Movements
            var xMove = xInput * moveSpeed;

            if (rushInput && _movements.CheckAllow("rush")(_isRushing))
            {
                _isRushing = true;
                xMove = xMove == 0
                    ? Convert.ToInt32(_isFaceTowardsRight) * 25f
                    : xMove + (xMove / math.abs(xMove)) * 25f;
                _timeSinceLastRushed = Time.time;
                _isRushing = false;
            }

            // Vertical Movements
            float jumpMove;
            if (_movements.CheckAllow("fly")(_isSpaceGravity))
            {
                jumpMove = flyInput ? moveSpeed : _rigid.velocity.y - 10f * Time.deltaTime;
            }
            else
            {
                TrySetAllowCoyote();
                jumpMove = _movements.CheckAllow("jump")(_remainingAllowCoyoteTime) && jumpInput ? jumpSpeed : _rigid.velocity.y;
            }

            /* Perform moving. */
            _rigid.velocity = new Vector2(xMove, jumpMove);

            /* Check player interaction. */
            Interact(interactInput);
        }
        
        private void TrySetAllowCoyote()
        {
            if (_envAware.groundCheck.ThreeHitGroundCheck(playerCollider, transform) == 2)
            {
                // Player Jumps from the edge
                _remainingAllowCoyoteTime = maxAllowCoyoteTime;
            }
        }

        private bool CheckAllow(string item)
        {
            // Func<bool> checkFunc;
            Dictionary<string, Func<bool>> checkFunc = new Dictionary<string, Func<bool>>
            {
                {
                    "jump",
                    () => _envAware
                              .groundCheck
                              .ThreeHitGroundCheck(playerCollider, transform) >= 1 || // Player on ground
                          _remainingAllowCoyoteTime > 0 // Allow coyote'
                },
                {
                    "rush",
                    () => !_isRushing
                },
                {
                    "fly",
                    () => _isSpaceGravity
                }
            };

            return checkFunc[item]();
        }
        

        /// <summary>
        /// Player interact with environment.
        /// </summary>
        /// <param name="isIntendInteract"></param>
        private void Interact(bool isIntendInteract)
        {
            if (!isIntendInteract)
                return;
            EventCenterManager.Instance.EventTrigger(GameEvent.PlayerTryInteract);
        }

        #endregion

        #region Environment Awareness

        private static Vector3 ColliderSizeJudge(Collider2D collider2D)
        {
            var c2DBounds = collider2D.bounds;

            return new Vector3(c2DBounds.size.x, c2DBounds.size.y, c2DBounds.size.z);
        }

        #endregion

        #region Constant Property Adjustments

        void AdjustPropertyConstantly()
        {
            AdjustFaceDir();
            AdjustGravityScale();
        }

        /// <summary>
        /// Adjust player face direction according to last speed dir.
        /// </summary>
        void AdjustFaceDir()
        {
            float scale = _isFaceTowardsRight ? 1 : -1;
            transform.localScale = new Vector3(scale, 1, 1);
        }

        /// <summary>
        /// When player is not in space and vertical velocity is big enough, adjust gravity scale.
        /// </summary>
        void AdjustGravityScale()
        {
            // When in space gravity, this method don't work.
            // When vertical velocity is small, restore gravity scale.
            if (_isSpaceGravity || Math.Abs(_rigid.velocity.y) <= 0.1f)
            {
                // _rigid.gravityScale = _isSpaceGravity? 0 : 1;
                _rigid.gravityScale = Convert.ToInt32(!_isSpaceGravity);
                return;
            }

            // When player is moving horizontally and didn't reach max grav scale
            // gradually increase gravity scale for player.
            if (Math.Abs(_rigid.velocity.y) >= 0.1f && _rigid.gravityScale <= maxGravityScale)
            {
                _rigid.gravityScale += Time.deltaTime * 10f;
            }
        }

        #endregion

        #region Ocassional Property Adjustments

        /// <summary>
        /// Adjust player running speed.
        /// </summary>
        /// <param name="speed"> The speed which you want the player to be at. Positive/Negative values
        /// will result in opposite moving directions.</param>
        void AdjustMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        void AdjustJumpSpeed(float speed)
        {
            jumpSpeed = speed;
        }

        // Trigger space gravity
        void TriggerSpaceGravityRegion(bool enter)
        {
            _isSpaceGravity = enter;
            _rigid.gravityScale = enter ? 0 : 1;
        }

        /// <summary>
        /// Player get hurt.
        /// </summary>
        /// <param name="hurtVal">Number of life to deduct.</param>
        void GetHurt(int hurtVal)
        {
            if (_currentLife <= 0 || _currentLife - hurtVal <= 0)
            {
                _currentLife = 0;
                Die();
                return;
            }

            _currentLife -= hurtVal;
        }

        /// <summary>
        /// Player dies.
        /// </summary>
        void Die()
        {
            //Debug.Log("Player dies.");
            //EventCenterManager.Instance.EventTrigger(GameEvent.PlayerDie);
        }

        #endregion

        public void OnDrawGizmos()
        {
            _envAware.groundCheck.OnDrawGizmos(playerCollider, transform, raycastUpPosition, raycastMaxDistance);
        }
    }
}
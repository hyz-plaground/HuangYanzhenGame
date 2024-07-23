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
    public class Movements
    {
        // Player Properties
        private static readonly PlayerProperties Prop = PlayerProperties.Instance;

        // Unity Basic Properties
        private Collider2D playerCollider;
        private Transform transform;
        private Rigidbody2D _rigid;

        // Player Movements
        public float moveSpeed = Prop.PLAYER_DEFAULT_MOVE_SPEED;
        public float jumpSpeed = Prop.PLAYER_DEFAULT_JUMP_SPEED;
        public float rushSpeed = Prop.PLAYER_DEFAULT_RUSH_SPEED;

        public bool haveRushCD = false;
        public float rushCD = 2f;
        public float rushDur = 2f;

        public float maxAllowCoyoteTime = Prop.PLAYER_MAX_ALLOW_COYOTE_TIME; //离开地面后的最大土狼时间
        public float maxGravityScale = Prop.PLAYER_MAX_GRAVITY_SCALE;

        private Dictionary<string, Func<object, bool>> _checkFunc;

        // Dynamic Player variables.
        private bool _isFaceTowardsRight = true;
        private bool _isRushing = false;
        private bool _isSpaceGravity = false;
        private float _timeSinceLastRushed = 0;
        private float _remainingAllowCoyoteTime = 0; //土狼

        // Environment Awareness
        private EnvAware _envAware;

        // Input Config
        private readonly Dictionary<string, Func<object>> _inputConfig = new Dictionary<string, Func<object>>
        {
            { "x", () => Input.GetAxis("Horizontal") },
            { "jump", () => Input.GetKey(KeyCode.Space) },
            { "fly", () => Input.GetKey(KeyCode.Space) },
            { "rush", () => Input.GetKey(KeyCode.R) },
            { "interact", () => Input.GetKey(KeyCode.F) },
        };

        // Constructor
        public Movements(
            EnvAware envAware,
            Collider2D playerCollider,
            Transform transform,
            Rigidbody2D rigid)
        {
            _envAware = envAware;
            this.playerCollider = playerCollider;
            this.transform = transform;
            _rigid = rigid;
            _checkFunc = new Dictionary<string, Func<object, bool>>
            {
                {
                    "jump",
                    (remainCoyoteTime) => _envAware
                                              .groundCheck
                                              // Player on ground
                                              .ThreeHitGroundCheck(playerCollider, transform) >= 1 ||
                                          // Allow coyote'
                                          (float)remainCoyoteTime > 0
                },
                {
                    "rush",
                    (isRushing) => !(bool)isRushing
                },
                {
                    "fly",
                    (isSpaceGravity) => (bool)isSpaceGravity
                }
            };
        }

        /// <summary>
        /// Determine whether a given move could be performed.
        /// </summary>
        /// <param name="item"> The movement you need to enter.</param>
        /// <returns>A function that returns a boolean value on your given input.</returns>
        public Func<object, bool> PlayerCan(string item)
        {
            return _checkFunc[item];
        }

        /// <summary>
        /// Get the corresponding input values for a desired player movement.
        /// </summary>
        /// <param name="item">Name of the movement.</param>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <returns>The input value of the desired player movement.</returns>
        public T GetInput<T>(string item)
        {
            return (T)_inputConfig[item]();
        }

        /// <summary>
        /// Get the horizontal component (x) of the desired player velocity.
        /// </summary>
        /// <returns>The horizontal component of the desired player velocity.</returns>
        private float GetXMove()
        {
            var xMove = GetInput<float>("x") * moveSpeed;

            if (!GetInput<bool>("rush") || !PlayerCan("rush")(_isRushing)) return xMove;

            _isRushing = true;
            xMove = xMove == 0
                ? Convert.ToInt32(_isFaceTowardsRight) * 25f
                : xMove + (xMove / math.abs(xMove)) * 25f;
            _timeSinceLastRushed = Time.time;
            _isRushing = false;

            return xMove;
        }

        /// <summary>
        /// Get the vertical component (y) of the desired player velocity.
        /// </summary>
        /// <returns>The vertical component of the desired player velocity.</returns>
        private float GetYMove()
        {
            float yMove;
            if (PlayerCan("fly")(_isSpaceGravity))
            {
                yMove = GetInput<bool>("fly") ? moveSpeed : _rigid.velocity.y - 10f * Time.deltaTime;
            }
            else
            {
                // Set coyote time.
                if (_envAware.groundCheck.ThreeHitGroundCheck(playerCollider, transform) == 2)
                {
                    // Player Jumps from the edge
                    _remainingAllowCoyoteTime = maxAllowCoyoteTime;
                }

                yMove = PlayerCan("jump")(_remainingAllowCoyoteTime) && GetInput<bool>("jump")
                    ? jumpSpeed
                    : _rigid.velocity.y;
            }

            return yMove;
        }

        /// <summary>
        /// Get the desired face direction of the player based on the current horizontal movements.
        /// True - player faces right.
        /// False - player faces left.
        /// </summary>
        /// <param name="isFaceTowardsRight"> Whether the player is facing right. </param>
        /// <returns>The new facing direction.</returns>
        private bool GetFaceDir(bool isFaceTowardsRight)
        {
            var xInput = GetInput<float>("x");
            return xInput != 0 ? xInput >= 0 : isFaceTowardsRight;
        }

        private void ChangeFaceDirFrom(bool isFaceTowardsRight)
        {
            float scale = isFaceTowardsRight ? 1 : -1;
            transform.localScale = new Vector3(scale, 1, 1);
        }

        private void ChangeGravityScale(bool isSpaceGravity)
        {
            if (isSpaceGravity || Math.Abs(_rigid.velocity.y) <= 0.1f)
            {
                _rigid.gravityScale = Convert.ToInt32(!isSpaceGravity);
                return;
            }

            // Player Falling: Gradually add gravityScale until max.
            if (Math.Abs(_rigid.velocity.y) >= 0.1f && _rigid.gravityScale <= maxGravityScale)
            {
                _rigid.gravityScale = math.min(_rigid.gravityScale + Time.deltaTime * 10f, maxGravityScale);
            }
        }

        private void Interact()
        {
            if (!GetInput<bool>("interact"))
                return;
            EventCenterManager.Instance.EventTrigger(GameEvent.PlayerTryInteract);
        }

        public void OnPlayerEnterSpace(bool isEnter)
        {
            _isSpaceGravity = isEnter;
            _rigid.gravityScale = isEnter ? 0 : 1;
        }

        public void Move()
        {
            // Horizontal and Vertical movements, along with the Face Direction
            var xMove = GetXMove();
            var yMove = GetYMove();
            _isFaceTowardsRight = GetFaceDir(_isFaceTowardsRight);

            // Change face direction base on x input.
            ChangeFaceDirFrom(_isFaceTowardsRight);
            ChangeGravityScale(_isSpaceGravity);

            // Apply the velocity to the rigid body.
            _rigid.velocity = new Vector2(xMove, yMove);

            // Player interact with the environment.
            Interact();
        }
    }
}
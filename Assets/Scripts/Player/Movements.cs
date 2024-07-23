using System;
using System.Collections.Generic;
using Player.IO;
using Unity.Mathematics;
using UnityEngine;

namespace Player
{
    public class Movements
    {
        // Player Properties
        private static readonly PlayerProperties Prop = PlayerProperties.Instance;

        // Unity Basic Properties
        private readonly Collider2D _playerCollider;
        private readonly Transform _transform;
        private readonly Rigidbody2D _rigid;

        // Environment Awareness
        private readonly EnvAware _envAware;

        // Player Movements
        private readonly float _moveSpeed = Prop.PLAYER_DEFAULT_MOVE_SPEED;
        private readonly float _jumpSpeed = Prop.PLAYER_DEFAULT_JUMP_SPEED;
        private readonly float _maxAllowCoyoteTime = Prop.PLAYER_MAX_ALLOW_COYOTE_TIME; //离开地面后的最大土狼时间
        private readonly float _maxGravityScale = Prop.PLAYER_MAX_GRAVITY_SCALE;
        private readonly Dictionary<Intention, Func<object, bool>> _checkFunc;

        // Dynamic Player variables.
        private bool _isFaceTowardsRight = true;
        private bool _isRushing;
        private bool _isSpaceGravity;
        private float _timeSinceLastRushed;
        private float _remainingAllowCoyoteTime; //土狼

        // Input Config
        private readonly InputConfig _in = new();

        // Constructor
        public Movements(
            EnvAware envAware,
            Collider2D playerCollider,
            Transform transform,
            Rigidbody2D rigid)
        {
            _envAware = envAware;
            _playerCollider = playerCollider;
            _transform = transform;
            _rigid = rigid;
            _checkFunc = new Dictionary<Intention, Func<object, bool>>
            {
                {
                    Intention.Jump,
                    (remainCoyoteTime) => _envAware
                                              .GroundCheck
                                              // Player on ground
                                              .ThreeHitGroundCheck(playerCollider, transform) >= 1 ||
                                          // Allow coyote'
                                          (float)remainCoyoteTime > 0
                },
                {
                    Intention.Rush,
                    (isRushing) => !(bool)isRushing
                },
                {
                    Intention.Fly,
                    (isSpaceGravity) => (bool)isSpaceGravity
                }
            };
        }

        /// <summary>
        /// Determine whether a given move could be performed.
        /// </summary>
        /// <param name="item"> The movement you need to enter.</param>
        /// <returns>A function that returns a boolean value on your given input.</returns>
        private Func<object, bool> PlayerCan(Intention item)
        {
            return _checkFunc[item];
        }

        /// <summary>
        /// Get the horizontal component (x) of the desired player velocity.
        /// </summary>
        /// <returns>The horizontal component of the desired player velocity.</returns>
        private float GetXMove()
        {
            var xMove = _in.GetInput<float>(Intention.X) * _moveSpeed;

            if (!_in.GetInput<bool>(Intention.Rush) || !PlayerCan(Intention.Rush)(_isRushing)) return xMove;

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
            if (PlayerCan(Intention.Fly)(_isSpaceGravity))
            {
                yMove = _in.GetInput<bool>(Intention.Fly) ? _moveSpeed : _rigid.velocity.y - 10f * Time.deltaTime;
            }
            else
            {
                // Set coyote time.
                if (_envAware.GroundCheck.ThreeHitGroundCheck(_playerCollider, _transform) == 2)
                {
                    // Player Jumps from the edge
                    _remainingAllowCoyoteTime = _maxAllowCoyoteTime;
                }

                yMove = PlayerCan(Intention.Jump)(_remainingAllowCoyoteTime) && _in.GetInput<bool>(Intention.Jump)
                    ? _jumpSpeed
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
            var xInput = _in.GetInput<float>(Intention.X);
            return xInput != 0 ? xInput >= 0 : isFaceTowardsRight;
        }

        private void ChangeFaceDirFrom(bool isFaceTowardsRight)
        {
            float scale = isFaceTowardsRight ? 1 : -1;
            _transform.localScale = new Vector3(scale, 1, 1);
        }

        private void ChangeGravityScale(bool isSpaceGravity)
        {
            if (isSpaceGravity || Math.Abs(_rigid.velocity.y) <= 0.1f)
            {
                _rigid.gravityScale = Convert.ToInt32(!isSpaceGravity);
                return;
            }

            // Player Falling: Gradually add gravityScale until max.
            if (Math.Abs(_rigid.velocity.y) >= 0.1f && _rigid.gravityScale <= _maxGravityScale)
            {
                _rigid.gravityScale = math.min(_rigid.gravityScale + Time.deltaTime * 10f, _maxGravityScale);
            }
        }

        private void Interact()
        {
            if (!_in.GetInput<bool>(Intention.Interact))
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
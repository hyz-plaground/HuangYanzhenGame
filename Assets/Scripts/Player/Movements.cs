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
    public class InputState
    {
        public float XInput { get; set; }
        public bool JumpInput { get; set; }
        public bool FlyInput { get; set; }
        public bool RushInput { get; set; }
        public bool InteractInput { get; set; }
    }

    public class Movements
    {
        private EnvAware _envAware;
        private Collider2D playerCollider;
        private Transform transform;
        private float moveSpeed;
        private Dictionary<string, Func<object, bool>> checkFunc;
        public Movements(
            EnvAware envAware,
            Collider2D playerCollider,
            Transform transform,
            float moveSpeed)
        {
            _envAware = envAware;
            this.playerCollider = playerCollider;
            this.transform = transform;
            this.moveSpeed = moveSpeed;
            checkFunc = new Dictionary<string, Func<object, bool>>
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
        public Func<object, bool> CheckAllow(string item)
        {
            return checkFunc[item];
        }
    }
}
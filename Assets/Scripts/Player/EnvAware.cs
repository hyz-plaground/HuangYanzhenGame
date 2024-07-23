using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Player
{
    public class EnvAware
    {
        
        public GroundCheck groundCheck;

        public EnvAware()
        {
            groundCheck = new GroundCheck();
        }
    };

    public class GroundCheck
    {
        // Player Properties
        private static readonly PlayerProperties Prop = PlayerProperties.Instance;
        private readonly float _rayCastUpPosition = Prop.PLAYER_GROUND_DETECTION_RAYCAST_UP_POSITION;
        private readonly float _rayCastMaxDistance = Prop.PLAYER_GROUND_DETECTION_RAYCAST_MAX_DISTANCE;

        
        /// <summary>
        /// Get the collider size.
        /// </summary>
        /// <param name="collider2D"> The 2D collider of the player.</param>
        /// <returns></returns>
        private static Vector3 GetColliderSize(Collider2D collider2D)
        {
            var c2DBounds = collider2D.bounds;
            return new Vector3(c2DBounds.size.x, c2DBounds.size.y, c2DBounds.size.z);
        }

        /// <summary>
        /// Get a list of the origins of the ray casts.
        /// </summary>
        /// <param name="playerCollider2D"></param>
        /// <param name="transform"></param>
        /// <param name="rayCastUpPosition"></param>
        /// <returns> A list of Vector2 positions, which are origins of the ray casts.</returns>
        private static IEnumerable<Vector2> GetRayOrigins(
            Collider2D playerCollider2D,
            Transform transform,
            float rayCastUpPosition)
        {
            // Object Length
            var objLength = GetColliderSize(playerCollider2D).x;

            // Basic line
            var basicOrigin = transform.position + rayCastUpPosition * transform.up;

            Vector2[] rayOrigins =
            {
                basicOrigin,
                basicOrigin + 0.8f * objLength / 1.5f * Vector3.right,
                basicOrigin - 0.8f * objLength / 1.5f * Vector3.right
            };

            return rayOrigins;
        }

        /// <summary>
        /// Use three downward rays to detect if the player is on the ground.
        /// </summary>
        /// <param name="playerCollider2D"> The 2D collider of the player. </param>
        /// <param name="transform"> The transform of the player. </param>
        /// <param name="rayCastUpPosition"> The ray cast up position. </param>
        /// <param name="rayCastMaxDistance"> The ray cast down position. </param>
        /// <returns></returns>
        public int ThreeHitGroundCheck(
            Collider2D playerCollider2D,
            Transform transform,
            float rayCastUpPosition = -0.5f,
            float rayCastMaxDistance = 0.8f)
        {
            var hitNum = 0;
            IEnumerable<Vector2> rayOrigins = GetRayOrigins(playerCollider2D, transform, rayCastUpPosition);

            rayOrigins.ToList().ForEach(rayOrigin =>
            {
                var hit = Physics2D.Raycast(
                    rayOrigin,
                    -Vector3.up,
                    rayCastMaxDistance,
                    LayerMask.GetMask("Ground")
                );
                if (hit.transform) hitNum++;
            });

            return hitNum;
        }
        
        /// <summary>
        /// Draw the ray cast in Unity Editor.
        /// </summary>
        public void OnDrawGizmos(
            Collider2D playerCollider2D,
            Transform transform)
        {
            var objLength = GetColliderSize(playerCollider2D).x;
            List<Vector2> rayOrigins = GetRayOrigins(playerCollider2D, transform, _rayCastUpPosition).ToList();
            Color[] rayColors = { Color.green, Color.blue, Color.red };

            for (var i = 0; i < rayOrigins.Count; i++)
            {
                Gizmos.color = rayColors[i];
                Vector2 startPos = rayOrigins[i];
                Vector2 endPos = startPos + -Vector2.up * _rayCastMaxDistance;
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
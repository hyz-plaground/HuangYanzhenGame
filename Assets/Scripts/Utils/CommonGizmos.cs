using UnityEngine;

namespace UnityEngine
{
    public class CommonGizmos : Singleton<CommonGizmos>
    {
        public void DrawCollider(
            Collider2D collider2D,
            Transform transform)
        {
            if (!collider2D || !transform)
                return;
            var c2DBounds = collider2D.bounds;
            var colliderSize = new Vector2(c2DBounds.size.x, c2DBounds.size.y);
            var colliderOffset = collider2D.offset;

            Gizmos.color = Color.green;
            switch (collider2D)
            {
                case BoxCollider2D boxCollider2D:
                    Vector2 bl = transform.TransformPoint(colliderOffset - colliderSize / 2);
                    Vector2 br = transform.TransformPoint(new Vector2(colliderOffset.x + colliderSize.x / 2,
                        colliderOffset.y - colliderSize.y / 2));
                    Vector2 tl = transform.TransformPoint(new Vector2(colliderOffset.x - colliderSize.x / 2,
                        colliderOffset.y + colliderSize.y / 2));
                    Vector2 tr = transform.TransformPoint(colliderOffset + colliderSize / 2);

                    Gizmos.DrawLine(bl, br);
                    Gizmos.DrawLine(bl, tl);
                    Gizmos.DrawLine(tl, tr);
                    Gizmos.DrawLine(tr, br);
                    break;
                case CircleCollider2D circleCollider2D:
                    Vector3 position = transform.position + (Vector3)colliderOffset;
                    Gizmos.DrawWireSphere(position, c2DBounds.size.x/2);
                    break;
            }
        }
    }
}
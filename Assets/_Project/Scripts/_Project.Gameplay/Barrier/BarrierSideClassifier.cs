using Castlebound.Gameplay.AI;
using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    public static class BarrierSideClassifier
    {
        public static bool IsEnemyPastThreshold(Collider2D barrier, Collider2D enemy, float threshold)
        {
            if (barrier == null || enemy == null)
            {
                return false;
            }

            if (!TryGetAnchorAndInward(barrier, out var anchorPos, out var inwardDir))
            {
                return false;
            }

            Vector2 dir = inwardDir.normalized;
            float actorExtent = Mathf.Abs(Vector2.Dot(enemy.bounds.extents, dir));
            float required = Mathf.Max(0f, threshold) + actorExtent;
            float signed = Vector2.Dot((Vector2)enemy.bounds.center - anchorPos, dir);

            return signed >= required;
        }

        static bool TryGetAnchorAndInward(Collider2D barrier, out Vector2 anchorPos, out Vector2 inwardDir)
        {
            anchorPos = Vector2.zero;
            inwardDir = Vector2.zero;

            var hold = barrier.GetComponent<EnemyBarrierHoldBehavior>();
            if (hold == null)
            {
                return false;
            }

            anchorPos = hold.Debug_GetAnchorPosition();
            Vector2 barrierPos = barrier.bounds.center;
            Vector2 outward = (anchorPos - barrierPos).normalized;
            if (outward.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            inwardDir = -outward;
            return true;
        }
    }
}

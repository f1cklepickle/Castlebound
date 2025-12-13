using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyBarrierHoldBehavior : MonoBehaviour
    {
        [SerializeField] private Transform approachAnchor;
        [SerializeField] private float holdRadius = 2.6f;
        [SerializeField] private float releaseMargin = 0.6f;

        public float HoldRadius => holdRadius;
        public float ReleaseMargin => releaseMargin;

        public float DistanceToAnchor(Vector2 enemyPosition)
        {
            Vector2 anchorPos = approachAnchor != null ? (Vector2)approachAnchor.position : (Vector2)transform.position;
            return Vector2.Distance(enemyPosition, anchorPos);
        }

        public bool CanHold(Vector2 enemyPosition, bool barrierBroken, int distTrend, int outrunFrames)
        {
            float dist = DistanceToAnchor(enemyPosition);
            return EnemyController2D.ShouldHoldForBarrierTarget(
                dist,
                barrierBroken,
                holdRadius,
                releaseMargin,
                distTrend,
                outrunFrames);
        }

        #region Debug/Test hooks
        public void Debug_SetAnchor(Transform anchor)
        {
            approachAnchor = anchor;
        }

        public void Debug_SetHoldRadius(float radius)
        {
            holdRadius = radius;
        }

        public bool Debug_CanHold(Vector2 enemyPosition)
        {
            return CanHold(enemyPosition, barrierBroken: false, distTrend: 0, outrunFrames: 0);
        }
        #endregion
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyBarrierHoldBehavior : MonoBehaviour
    {
        [SerializeField] private Transform approachAnchor;
        [SerializeField] private float holdRadius = 2.6f;

        public float HoldRadius => holdRadius;

        public float DistanceToAnchor(Vector2 enemyPosition)
        {
            Vector2 anchorPos = approachAnchor != null ? (Vector2)approachAnchor.position : (Vector2)transform.position;
            return Vector2.Distance(enemyPosition, anchorPos);
        }

        public Vector2 Debug_GetAnchorPosition()
        {
            return approachAnchor != null ? (Vector2)approachAnchor.position : (Vector2)transform.position;
        }

        public bool CanHold(Vector2 enemyPosition, bool barrierBroken)
        {
            float dist = DistanceToAnchor(enemyPosition);
            return EnemyController2D.ShouldHoldForBarrierTarget(
                dist,
                barrierBroken,
                holdRadius);
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
            return CanHold(enemyPosition, barrierBroken: false);
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 pos = approachAnchor != null ? approachAnchor.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pos, 0.08f);
            Gizmos.DrawLine(transform.position, pos);
        }
#endif
    }
}

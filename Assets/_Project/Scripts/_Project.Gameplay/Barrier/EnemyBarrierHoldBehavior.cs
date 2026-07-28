using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Legacy component name retained for prefab compatibility; this now owns only
    // authored barrier approach geometry, never enemy engagement distance.
    public class EnemyBarrierHoldBehavior : MonoBehaviour
    {
        [SerializeField] private Transform approachAnchor;

        public float DistanceToAnchor(Vector2 enemyPosition)
        {
            Vector2 anchorPos = approachAnchor != null ? (Vector2)approachAnchor.position : (Vector2)transform.position;
            return Vector2.Distance(enemyPosition, anchorPos);
        }

        public Vector2 Debug_GetAnchorPosition()
        {
            return approachAnchor != null ? (Vector2)approachAnchor.position : (Vector2)transform.position;
        }

        #region Debug/Test hooks
        public void Debug_SetAnchor(Transform anchor)
        {
            approachAnchor = anchor;
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

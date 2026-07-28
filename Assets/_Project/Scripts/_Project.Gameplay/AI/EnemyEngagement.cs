using System.Collections.Generic;
using UnityEngine;

public class EnemyEngagement : MonoBehaviour
{
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField, Min(0f)] private float engagementDistance = 0.5f;
    [SerializeField, Min(0f)] private float releaseMargin = 0.25f;

    private Transform cachedTarget;
    private Collider2D[] cachedTargetColliders = System.Array.Empty<Collider2D>();

    public float EngagementDistance => engagementDistance;
    public float ReleaseMargin => releaseMargin;

    private void Awake()
    {
        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();
    }

    public float SurfaceDistanceTo(Transform target)
    {
        if (target == null)
            return float.PositiveInfinity;

        if (bodyCollider == null)
            bodyCollider = GetComponent<Collider2D>();

        if (cachedTarget != target)
        {
            cachedTarget = target;
            cachedTargetColliders = target.GetComponentsInChildren<Collider2D>();
        }

        return GetSurfaceDistance(
            bodyCollider,
            cachedTargetColliders,
            transform.position,
            target.position);
    }

    public bool IsWithinEngagementDistance(Transform target)
    {
        return SurfaceDistanceTo(target) <= engagementDistance;
    }

    public bool ShouldHoldTarget(Transform target, bool targetBroken, bool currentlyHolding)
    {
        return ShouldHold(
            currentlyHolding,
            SurfaceDistanceTo(target),
            engagementDistance,
            releaseMargin,
            targetBroken);
    }

    public static float GetSurfaceDistance(
        Collider2D sourceCollider,
        IReadOnlyList<Collider2D> targetColliders,
        Vector2 sourcePosition,
        Vector2 targetPosition)
    {
        float nearestDistance = float.PositiveInfinity;

        if (targetColliders != null)
        {
            for (int i = 0; i < targetColliders.Count; i++)
            {
                Collider2D targetCollider = targetColliders[i];
                if (targetCollider == null || !targetCollider.enabled || targetCollider.isTrigger)
                    continue;

                float distance;
                if (sourceCollider != null && sourceCollider.enabled && !sourceCollider.isTrigger)
                {
                    distance = Mathf.Max(0f, Physics2D.Distance(sourceCollider, targetCollider).distance);
                }
                else
                {
                    distance = Vector2.Distance(sourcePosition, targetCollider.ClosestPoint(sourcePosition));
                }

                nearestDistance = Mathf.Min(nearestDistance, distance);
            }
        }

        return float.IsPositiveInfinity(nearestDistance)
            ? Vector2.Distance(sourcePosition, targetPosition)
            : nearestDistance;
    }

    public static bool ShouldHold(
        bool currentlyHolding,
        float surfaceDistance,
        float engagementDistance,
        float releaseMargin,
        bool targetBroken)
    {
        if (targetBroken)
            return false;

        float entryDistance = Mathf.Max(0f, engagementDistance);
        if (!currentlyHolding)
            return surfaceDistance <= entryDistance;

        float releaseDistance = entryDistance + Mathf.Max(0f, releaseMargin);
        return surfaceDistance < releaseDistance;
    }

#if UNITY_EDITOR
    public void Debug_SetTuning(float distance, float margin)
    {
        engagementDistance = Mathf.Max(0f, distance);
        releaseMargin = Mathf.Max(0f, margin);
    }
#endif
}

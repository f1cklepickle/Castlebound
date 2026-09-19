using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Live queries only. Positions describe the body circle's WORLD center, not its transform.
    // Callers synchronize transform edits before sampling and invalidate cached data after map changes.
    public sealed class StaticNavigationWorld2D
    {
        private readonly int obstacleMask = LayerMask.GetMask("Default", "Walls", "Barriers", "Gates", "Environment");
        private readonly Collider2D[] overlaps;
        private readonly RaycastHit2D[] hits;
        private readonly ContactFilter2D filter;
        public float ClearanceRadius { get; }
        public float BodyRadius { get; }
        public float ClearanceMargin { get; }
        internal ContactFilter2D ObstacleFilter => filter;
#if UNITY_EDITOR
        public int DebugPhysicsQueryCount { get; private set; }
#endif

        public StaticNavigationWorld2D(float bodyRadius, float clearanceMargin = 0.02f, int queryCapacity = 64)
        {
            if (!IsFinite(bodyRadius) || bodyRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(bodyRadius));
            if (!IsFinite(clearanceMargin) || clearanceMargin < 0f || !IsFinite(bodyRadius + clearanceMargin))
                throw new ArgumentOutOfRangeException(nameof(clearanceMargin));
            ClearanceRadius = bodyRadius + clearanceMargin;
            BodyRadius = bodyRadius;
            ClearanceMargin = clearanceMargin;
            if (queryCapacity < 1) throw new ArgumentOutOfRangeException(nameof(queryCapacity));
            overlaps = new Collider2D[queryCapacity];
            hits = new RaycastHit2D[queryCapacity];
            filter = new ContactFilter2D { useLayerMask = true, layerMask = obstacleMask, useTriggers = false };
        }

        public bool IsPointClear(Vector2 center) => SamplePoint(center) == StaticNavigationSampleState.Clear;

        public StaticNavigationSampleState SamplePoint(Vector2 center)
        {
            Validate(center);
#if UNITY_EDITOR
            DebugPhysicsQueryCount++;
#endif
            int count = Physics2D.OverlapCircle(center, ClearanceRadius, filter, overlaps);
            if (count == overlaps.Length) return StaticNavigationSampleState.Unknown;
            for (int i = 0; i < count; i++)
                if (IsObstacle(overlaps[i])) return StaticNavigationSampleState.Blocked;
            return StaticNavigationSampleState.Clear;
        }

        public bool IsSegmentClear(Vector2 start, Vector2 end)
        {
            Validate(start);
            Validate(end);
            // Explicit endpoint checks also handle zero-length segments and starting overlaps.
            if (!IsPointClear(start) || !IsPointClear(end)) return false;
            return SampleEdgeWithClearEndpoints(start, end) == StaticNavigationSampleState.Clear;
        }

        // Cache callers must already know BOTH endpoints are Clear for this same unchanged world/profile.
        // Exactly one physics query for a nonzero edge; does not repeat endpoint overlap queries.
        public StaticNavigationSampleState SampleEdgeWithClearEndpoints(Vector2 start, Vector2 end)
        {
            Validate(start);
            Validate(end);
            Vector2 delta = end - start;
            if (!IsFinite(delta.magnitude)) throw new ArgumentOutOfRangeException(nameof(end));
            if (delta.sqrMagnitude == 0f) return StaticNavigationSampleState.Clear;
#if UNITY_EDITOR
            DebugPhysicsQueryCount++;
#endif
            int count = Physics2D.CircleCast(start, ClearanceRadius, delta.normalized, filter, hits, delta.magnitude);
            if (count == hits.Length) return StaticNavigationSampleState.Unknown;
            for (int i = 0; i < count; i++)
                if (IsObstacle(hits[i].collider)) return StaticNavigationSampleState.Blocked;
            return StaticNavigationSampleState.Clear;
        }

        internal bool IsObstacle(Collider2D collider)
        {
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy || collider.isTrigger ||
                (obstacleMask & (1 << collider.gameObject.layer)) == 0) return false;
            var body = collider.attachedRigidbody;
            if (body != null && (!body.simulated || body.bodyType != RigidbodyType2D.Static)) return false;
            // Actor children can be on Default and may have no Rigidbody2D of their own.
            for (Transform node = collider.transform; node != null; node = node.parent)
                if (node.CompareTag("Enemy") || node.CompareTag("Player") ||
                    node.gameObject.layer == LayerMask.NameToLayer("Enemies") ||
                    node.gameObject.layer == LayerMask.NameToLayer("Player")) return false;
            return true;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static void Validate(Vector2 value)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y)) throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}

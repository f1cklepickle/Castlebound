using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Shared synchronous geometry queries only. Never moves an actor or changes cache walkability.
    public sealed class StaticNavigationRecoveryQueries : IDisposable
    {
        public const float ContactTolerance = StaticNavigationStartConnector.ContactTolerance;
        private readonly StaticNavigationWorld2D world;
        private readonly Collider2D[] overlaps;
        private readonly ContactFilter2D filter = new ContactFilter2D { useTriggers = false };
        private readonly GameObject probeObject;
        private readonly CapsuleCollider2D probe;

        public StaticNavigationRecoveryQueries(StaticNavigationWorld2D world, int capacity = 64)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            overlaps = new Collider2D[capacity];
            probeObject = new GameObject("Navigation recovery query") { hideFlags = HideFlags.HideAndDontSave };
            probe = probeObject.AddComponent<CapsuleCollider2D>();
            probe.enabled = false; probe.isTrigger = true; probe.excludeLayers = ~0;
            probe.layerOverridePriority = int.MaxValue; probe.direction = CapsuleDirection2D.Vertical;
        }

        // Blocked means genuine static penetration; Clear includes shallow numerical contact.
        public StaticNavigationSampleState Detect(CircleCollider2D body, out Vector2 outward, Func<bool> spend = null)
        {
            outward = Vector2.zero;
            if (!ValidBody(body) || !Spend(spend)) return StaticNavigationSampleState.Unknown;
            int count = Physics2D.OverlapCircle(body.bounds.center, world.ClearanceRadius, filter, overlaps);
            if (count == overlaps.Length) return StaticNavigationSampleState.Unknown;
            float deepest = -ContactTolerance;
            for (int i = 0; i < count; i++)
            {
                var obstacle = overlaps[i];
                if (Own(body, obstacle) || !world.IsObstacle(obstacle)) continue;
                if (!Spend(spend)) return StaticNavigationSampleState.Unknown;
                var distance = Physics2D.Distance(body, obstacle);
                if (!distance.isValid) return StaticNavigationSampleState.Unknown;
                float gap = Corrected(distance, obstacle);
                Vector2 normal = Outward(distance);
                if (gap < deepest || gap == deepest && Earlier(normal, outward))
                { deepest = gap; outward = normal; }
            }
            return deepest < -ContactTolerance ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear;
        }

        public StaticNavigationSampleState Destination(CircleCollider2D body, Vector2 end, Func<bool> spend = null)
        {
            if (!ValidBody(body) || !Finite(end) || !Spend(spend)) return StaticNavigationSampleState.Unknown;
            int count = Physics2D.OverlapCircle(end, world.ClearanceRadius, filter, overlaps);
            if (count == overlaps.Length) return StaticNavigationSampleState.Unknown;
            for (int i = 0; i < count; i++)
                if (!Own(body, overlaps[i]) && (world.IsObstacle(overlaps[i]) || Actor(overlaps[i])))
                    return StaticNavigationSampleState.Blocked;
            return StaticNavigationSampleState.Clear;
        }

        // A complete swept body check, including the initial overlap. The ONLY overlap exception
        // is non-deepening travel along an outward/tangent separating plane of an initial convex solid.
        public StaticNavigationSampleState Sweep(CircleCollider2D body, Vector2 end, Func<bool> spend = null)
        {
            if (!ValidBody(body) || !Finite(end)) return StaticNavigationSampleState.Unknown;
            Vector2 start = body.bounds.center, delta = end - start;
            float length = delta.magnitude;
            if (length <= 0f || length > EnemyNavigationRecovery.MaxDistance + ContactTolerance)
                return StaticNavigationSampleState.Blocked;
            if (!Spend(spend)) return StaticNavigationSampleState.Unknown;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
            Vector2 middle = start + delta * 0.5f;
            int count = Physics2D.OverlapCapsule(middle,
                new Vector2(2f * world.ClearanceRadius, 2f * world.ClearanceRadius + length),
                CapsuleDirection2D.Vertical, angle, filter, overlaps);
            if (count == overlaps.Length) return StaticNavigationSampleState.Unknown;
            probeObject.transform.SetPositionAndRotation(middle, Quaternion.Euler(0f, 0f, angle));
            probe.size = new Vector2(2f * world.BodyRadius, 2f * world.BodyRadius + length);
            probe.enabled = true;
            try
            {
                Physics2D.SyncTransforms();
                for (int i = 0; i < count; i++)
                {
                    var obstacle = overlaps[i];
                    if (Own(body, obstacle)) continue;
                    // Recovery deliberately includes actors, although ordinary static navigation excludes them.
                    if (Actor(obstacle)) return StaticNavigationSampleState.Blocked;
                    if (!world.IsObstacle(obstacle)) continue;
                    if (!Spend(spend)) return StaticNavigationSampleState.Unknown;
                    var initial = Physics2D.Distance(body, obstacle);
                    if (!initial.isValid) return StaticNavigationSampleState.Unknown;
                    float gap = Corrected(initial, obstacle);
                    // Do not enter a new obstacle's clearance envelope or ignore a compound/concave blocker.
                    if (gap > world.ClearanceMargin + StaticNavigationContactSweep.PolygonContactSkin(obstacle) + ContactTolerance ||
                        !Convex(obstacle)) return StaticNavigationSampleState.Blocked;
                    Vector2 normal = Outward(initial);
                    if (normal.sqrMagnitude < 0.5f || Vector2.Dot(delta, normal) < -0.000001f)
                        return StaticNavigationSampleState.Blocked;
                    if (!Spend(spend)) return StaticNavigationSampleState.Unknown;
                    var swept = Physics2D.Distance(probe, obstacle);
                    if (!swept.isValid) return StaticNavigationSampleState.Unknown;
                    if (Corrected(swept, obstacle) < Mathf.Min(0f, gap) - ContactTolerance * 0.1f)
                        return StaticNavigationSampleState.Blocked;
                }
                return StaticNavigationSampleState.Clear;
            }
            finally { probe.enabled = false; }
        }

        private bool ValidBody(CircleCollider2D body) => body != null && body.enabled && !body.isTrigger &&
            body.gameObject.activeInHierarchy &&
            Mathf.Approximately(body.radius * Mathf.Abs(body.transform.lossyScale.x), world.BodyRadius) &&
            Mathf.Approximately(body.radius * Mathf.Abs(body.transform.lossyScale.y), world.BodyRadius);
        private static bool Own(CircleCollider2D body, Collider2D other) => other == null || other == body ||
            other.transform.IsChildOf(body.transform) ||
            body.attachedRigidbody != null && other.attachedRigidbody == body.attachedRigidbody;
        private static bool Actor(Collider2D collider)
        {
            if (collider == null || !collider.enabled || collider.isTrigger) return false;
            for (var node = collider.transform; node != null; node = node.parent)
                if (node.CompareTag("Player") || node.CompareTag("Enemy") ||
                    node.gameObject.layer == LayerMask.NameToLayer("Player") ||
                    node.gameObject.layer == LayerMask.NameToLayer("Enemies")) return true;
            return false;
        }
        private static bool Convex(Collider2D collider) => collider is BoxCollider2D ||
            collider is CircleCollider2D || collider is CapsuleCollider2D;
        private static Vector2 Outward(ColliderDistance2D distance) =>
            (distance.isOverlapped ? distance.pointB - distance.pointA : distance.pointA - distance.pointB).normalized;
        private static float Corrected(ColliderDistance2D distance, Collider2D obstacle) =>
            distance.distance + StaticNavigationContactSweep.PolygonContactSkin(obstacle);
        private static bool Earlier(Vector2 a, Vector2 b) => a.x < b.x || a.x == b.x && a.y < b.y;
        private static bool Spend(Func<bool> spend) => spend == null || spend();
        private static bool Finite(Vector2 p) => !float.IsNaN(p.x) && !float.IsNaN(p.y) && !float.IsInfinity(p.x) && !float.IsInfinity(p.y);
        public void Dispose()
        {
            if (probeObject == null) return;
            probe.enabled = false;
            if (Application.isPlaying) UnityEngine.Object.Destroy(probeObject);
            else UnityEngine.Object.DestroyImmediate(probeObject);
        }
    }
}

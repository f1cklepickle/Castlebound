using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public static class EnemyNavigationGoals
    {
        public static void Build(EnemyNavigationTarget target, Vector2 start, IEnemyNavigationWorld world, List<Vector2> goals)
        {
            goals.Clear();
            if (!target.IsValid) return;
            Vector2 center = target.Transform.position;
            if (target.Passage) { goals.Add(center); return; }
            // Player center is a preference only; Resolve validates clearance before selecting it.
            if (target.Type == EnemyTargetType.Player) goals.Add(center);
            Bounds bounds = new Bounds(center, Vector3.zero);
            if (target.Colliders != null)
                foreach (var collider in target.Colliders)
                    if (Solid(collider)) bounds.Encapsulate(collider.bounds);
            float extent = world.BodyRadius + target.EngagementDistance;
            bounds.Expand(extent * 2f);
            var cache = world.Cache;
            // Current actors/barriers are small; refuse unbounded geometry enumeration.
            if (bounds.size.x > 16f || bounds.size.y > 16f) return;
            if (!cache.TryWorldToCell(bounds.min, out var min) || !cache.TryWorldToCell(bounds.max, out var max)) return;
            var band = new List<Vector2>();
            for (int y = min.y; y <= max.y; y++)
            for (int x = min.x; x <= max.x; x++)
            {
                Vector2 point = cache.CellToWorld(new Vector2Int(x, y));
                if (InAttackBand(target, point, world.BodyRadius) && ValidSide(target, point)) band.Add(point);
            }
            band.Sort((a, b) =>
            {
                int sideA = PreferredSide(target, a) ? 0 : 1, sideB = PreferredSide(target, b) ? 0 : 1;
                int order = sideA.CompareTo(sideB);
                if (order == 0) order = (a - start).sqrMagnitude.CompareTo((b - start).sqrMagnitude);
                if (order == 0) order = a.y.CompareTo(b.y);
                return order == 0 ? a.x.CompareTo(b.x) : order;
            });
            goals.AddRange(band);
        }

        public static bool InAttackBand(EnemyNavigationTarget target, Vector2 point, float radius)
        {
            float distance = SurfaceGap(target, point, radius);
            return distance >= 0f && distance <= Mathf.Max(0f, target.EngagementDistance - 0.02f);
        }
        public static bool WithinReach(EnemyNavigationTarget target, Vector2 point, float radius)
            => SurfaceGap(target, point, radius) <= target.EngagementDistance;
        private static float SurfaceGap(EnemyNavigationTarget target, Vector2 point, float radius)
        {
            float distance = float.PositiveInfinity;
            if (target.Colliders != null)
                foreach (var collider in target.Colliders)
                    if (Solid(collider)) distance = Mathf.Min(distance, Vector2.Distance(point, collider.ClosestPoint(point)) - radius);
            if (float.IsPositiveInfinity(distance)) distance = Vector2.Distance(point, target.Transform.position);
            return distance;
        }
        public static bool ValidSide(EnemyNavigationTarget target, Vector2 point)
        {
            if (!target.ExteriorOnly) return true;
            if (CastleRegionTracker.Instance != null) return !CastleRegionTracker.Instance.ContainsPosition(point);
            // Without a region, honor the existing approach anchor if one is authored.
            return !target.HasAnchor || PreferredSide(target, point);
        }
        private static bool PreferredSide(EnemyNavigationTarget target, Vector2 point)
            => !target.HasAnchor || Vector2.Dot(point - (Vector2)target.Transform.position,
                target.Anchor - (Vector2)target.Transform.position) >= 0f;
        private static bool Solid(Collider2D collider) => collider != null && collider.enabled &&
            collider.gameObject.activeInHierarchy && !collider.isTrigger;

        public static StaticNavigationSampleState Validate(EnemyNavigationTarget target, Vector2 point, IEnemyNavigationWorld world)
        {
            var state = world.Point(point);
            if (state != StaticNavigationSampleState.Clear || target.Passage || target.Type != EnemyTargetType.Player) return state;
            // Prevent a near-player candidate on the other side of a thin separating wall.
            return world.Sight(point, target.Transform.position);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // One world and one clearance profile per instance. Callers explicitly invalidate changed geometry.
    public sealed class StaticNavigationWorldCache : IStaticNavigationGraph
    {
        private readonly StaticNavigationLayout layout;
        private readonly Func<Vector2, StaticNavigationSampleState> samplePoint;
        private readonly Func<Vector2, Vector2, StaticNavigationSampleState> sampleEdge;
        private readonly Dictionary<Vector2Int, StaticNavigationSampleState> cells = new Dictionary<Vector2Int, StaticNavigationSampleState>();
        private readonly Dictionary<Work, StaticNavigationSampleState> edges = new Dictionary<Work, StaticNavigationSampleState>();
        private readonly Queue<Work> pending = new Queue<Work>();
        private readonly HashSet<Work> requested = new HashSet<Work>();
        public BoundsInt Bounds => layout.Bounds;
        public long Revision { get; private set; }
        public float ClearanceRadius { get; }
        public float CellSize => layout.CellSize;
        internal StaticNavigationWorld2D World { get; }
        public int PendingSamples => pending.Count;
#if UNITY_EDITOR
        public int DebugUniqueCellSamples { get; private set; }
        public int DebugUniqueEdgeSamples { get; private set; }
        public int DebugCacheHits { get; private set; }
        public int DebugSamplingQueries { get; private set; }
#endif

        public StaticNavigationWorldCache(StaticNavigationLayout layout)
            : this(layout, new StaticNavigationWorld2D(0.87684506f, 0.02f)) { }

        public StaticNavigationWorldCache(StaticNavigationLayout layout, StaticNavigationWorld2D world)
            : this(layout, (world ?? throw new ArgumentNullException(nameof(world))).SamplePoint,
                world.SampleEdgeWithClearEndpoints, world.ClearanceRadius) { World = world; }

        // Injected samplers must each perform at most ONE physics query and use the same clearance profile.
        public StaticNavigationWorldCache(StaticNavigationLayout layout, Func<Vector2, StaticNavigationSampleState> pointSampler,
            Func<Vector2, Vector2, StaticNavigationSampleState> edgeSampler, float clearanceRadius = 0.89684506f)
        {
            this.layout = layout ?? throw new ArgumentNullException(nameof(layout));
            if (layout.Subdivisions != 4) throw new ArgumentException("Shared navigation uses four WorldGrid subdivisions.", nameof(layout));
            samplePoint = pointSampler ?? throw new ArgumentNullException(nameof(pointSampler));
            sampleEdge = edgeSampler ?? throw new ArgumentNullException(nameof(edgeSampler));
            if (float.IsNaN(clearanceRadius) || float.IsInfinity(clearanceRadius) || clearanceRadius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(clearanceRadius));
            ClearanceRadius = clearanceRadius;
        }

        public Vector2 CellToWorld(Vector2Int cell) => layout.CellToWorld(cell);
        public bool TryWorldToCell(Vector2 point, out Vector2Int cell) => layout.TryWorldToCell(point, out cell);

        // Supply the collider's world AABBs before/after the edit (including when disabling it).
        // Sparse global keys have no chunk seams. Segment AABBs include both edge endpoints.
        public void InvalidateWorldBounds(Rect oldBounds, Rect newBounds)
        {
            Rect oldDirty = Expand(oldBounds), newDirty = Expand(newBounds);
            bool Affected(Work work)
            {
                Vector2 a = CellToWorld(work.A), b = CellToWorld(work.B);
                return Intersects(oldDirty, a, b) || Intersects(newDirty, a, b);
            }
            var dirtyCells = new List<Vector2Int>();
            foreach (var cell in cells.Keys)
                if (Affected(new Work(cell, cell))) dirtyCells.Add(cell);
            foreach (var cell in dirtyCells) cells.Remove(cell);
            var dirtyEdges = new List<Work>();
            foreach (var edge in edges.Keys)
                if (Affected(edge)) dirtyEdges.Add(edge);
            foreach (var edge in dirtyEdges) edges.Remove(edge);
            // Drop pending edges as well: their previously-clear endpoints may now be Unknown.
            int count = pending.Count;
            while (count-- > 0)
            {
                var work = pending.Dequeue();
                if (Affected(work)) requested.Remove(work);
                else pending.Enqueue(work);
            }
            Revision++;
        }

        private Rect Expand(Rect bounds)
        {
            float padding = ClearanceRadius + CellSize;
            float minX = bounds.xMin - padding, minY = bounds.yMin - padding;
            float maxX = bounds.xMax + padding, maxY = bounds.yMax + padding;
            if (bounds.width < 0f || bounds.height < 0f || !Finite(minX) || !Finite(minY) || !Finite(maxX) || !Finite(maxY))
                throw new ArgumentOutOfRangeException(nameof(bounds));
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static bool Intersects(Rect rect, Vector2 a, Vector2 b)
            => Mathf.Max(a.x, b.x) >= rect.xMin && Mathf.Min(a.x, b.x) <= rect.xMax &&
               Mathf.Max(a.y, b.y) >= rect.yMin && Mathf.Min(a.y, b.y) <= rect.yMax;

        public StaticNavigationSampleState GetCellState(Vector2Int cell)
        {
            if (!layout.Contains(cell)) return StaticNavigationSampleState.Blocked;
            if (cells.TryGetValue(cell, out var state))
            {
#if UNITY_EDITOR
                DebugCacheHits++;
#endif
                return state;
            }
            Request(new Work(cell, cell));
            return StaticNavigationSampleState.Unknown;
        }

        public StaticNavigationSampleState GetTraversalState(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;
            if (!layout.Contains(from) || !layout.Contains(to) || delta == Vector2Int.zero ||
                Mathf.Abs(delta.x) > 1 || Mathf.Abs(delta.y) > 1) return StaticNavigationSampleState.Blocked;
            var result = RawEdge(from, to);
            if (delta.x == 0 || delta.y == 0 || result == StaticNavigationSampleState.Blocked) return result;
            var horizontal = from + new Vector2Int(delta.x, 0);
            var vertical = from + new Vector2Int(0, delta.y);
            result = Combine(result, RawEdge(from, horizontal));
            result = Combine(result, RawEdge(horizontal, to));
            result = Combine(result, RawEdge(from, vertical));
            return Combine(result, RawEdge(vertical, to));
        }

        // Returns query attempts, including saturated queries. A saturated item retries on a later call.
        public int SamplePending(int maxQueries)
        {
            if (maxQueries < 0) throw new ArgumentOutOfRangeException(nameof(maxQueries));
            int attempts = 0, available = pending.Count;
            while (attempts < maxQueries && available-- > 0)
            {
                Work work = pending.Dequeue();
                var state = work.IsCell ? samplePoint(CellToWorld(work.A)) : sampleEdge(CellToWorld(work.A), CellToWorld(work.B));
                attempts++;
#if UNITY_EDITOR
                DebugSamplingQueries++;
#endif
                if (state == StaticNavigationSampleState.Unknown) { pending.Enqueue(work); continue; }
                requested.Remove(work);
                if (work.IsCell)
                {
                    cells.Add(work.A, state);
#if UNITY_EDITOR
                    DebugUniqueCellSamples++;
#endif
                }
                else
                {
                    edges.Add(work, state);
#if UNITY_EDITOR
                    DebugUniqueEdgeSamples++;
#endif
                }
            }
            return attempts;
        }

        private StaticNavigationSampleState RawEdge(Vector2Int from, Vector2Int to)
        {
            var key = new Work(from, to);
            if (edges.TryGetValue(key, out var cached))
            {
#if UNITY_EDITOR
                DebugCacheHits++;
#endif
                return cached;
            }
            var endpoints = Combine(GetCellState(from), GetCellState(to));
            if (endpoints == StaticNavigationSampleState.Blocked)
            {
                edges[key] = endpoints;
                return endpoints;
            }
            if (endpoints == StaticNavigationSampleState.Clear) Request(key);
            return StaticNavigationSampleState.Unknown;
        }

        private void Request(Work work) { if (requested.Add(work)) pending.Enqueue(work); }
        private static StaticNavigationSampleState Combine(StaticNavigationSampleState a, StaticNavigationSampleState b)
            => a == StaticNavigationSampleState.Blocked || b == StaticNavigationSampleState.Blocked
                ? StaticNavigationSampleState.Blocked
                : a == StaticNavigationSampleState.Unknown || b == StaticNavigationSampleState.Unknown
                    ? StaticNavigationSampleState.Unknown : StaticNavigationSampleState.Clear;

        private readonly struct Work : IEquatable<Work>
        {
            public readonly Vector2Int A;
            public readonly Vector2Int B;
            public bool IsCell => A == B;
            public Work(Vector2Int a, Vector2Int b)
            {
                bool ordered = a.y < b.y || a.y == b.y && a.x <= b.x;
                A = ordered ? a : b;
                B = ordered ? b : a;
            }
            public bool Equals(Work other) => A == other.A && B == other.B;
            public override bool Equals(object obj) => obj is Work other && Equals(other);
            public override int GetHashCode() => unchecked(A.GetHashCode() * 397 ^ B.GetHashCode());
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Live/off-lattice safety queries have their own explicit shared frame budget.
    // Lattice sampling remains under the scheduler's separate 128-query cap.
    public sealed class StaticNavigationRuntimeQueries
    {
        public const int QueriesPerFrame = 256;
        private readonly StaticNavigationWorld2D world;
        private readonly StaticNavigationStartConnector connector;
        private readonly Dictionary<Vector2, StaticNavigationSampleState> points = new Dictionary<Vector2, StaticNavigationSampleState>();
        private readonly Dictionary<Vector2, List<Vector2>> clearSegments = new Dictionary<Vector2, List<Vector2>>();
        private int frame = -1;
        public int Used { get; private set; }
        public StaticNavigationRuntimeQueries(StaticNavigationWorld2D world, StaticNavigationStartConnector connector)
        { this.world = world; this.connector = connector; }

        public void BeginFrame(int index)
        {
            if (index == frame) return;
            frame = index; Used = 0; Invalidate();
        }
        public void Invalidate() { points.Clear(); clearSegments.Clear(); }
        internal bool Spend() { if (Used >= QueriesPerFrame) return false; Used++; return true; }
        public StaticNavigationSampleState Point(Vector2 point)
        {
            if (points.TryGetValue(point, out var state)) return state;
            if (!Spend()) return StaticNavigationSampleState.Unknown;
            state = world.SamplePoint(point);
            points[point] = state;
            return state;
        }
        public StaticNavigationSampleState Segment(Vector2 start, Vector2 end)
        {
            if (IsCertified(start, end)) return StaticNavigationSampleState.Clear;
            var a = Point(start); var b = Point(end);
            if (a == StaticNavigationSampleState.Blocked || b == StaticNavigationSampleState.Blocked)
                return StaticNavigationSampleState.Blocked;
            if (a != StaticNavigationSampleState.Clear || b != StaticNavigationSampleState.Clear || !Spend())
                return StaticNavigationSampleState.Unknown;
            var state = world.SampleEdgeWithClearEndpoints(start, end);
            if (state == StaticNavigationSampleState.Clear) Certify(start, end);
            return state;
        }
        public StaticNavigationSampleState Sight(Vector2 start, Vector2 end)
            => Spend() ? world.SampleSight(start, end) : StaticNavigationSampleState.Unknown;
        public StaticNavigationSampleState Connect(Vector2 start, StaticNavigationWorldCache cache, out Vector2Int cell)
        {
            var state = connector.TryConnect(start, out cell, Spend);
            if (state == StaticNavigationSampleState.Clear) Certify(start, cache.CellToWorld(cell));
            return state;
        }
        private void Certify(Vector2 start, Vector2 end)
        {
            if (!clearSegments.TryGetValue(start, out var ends)) clearSegments.Add(start, ends = new List<Vector2>(2));
            ends.Add(end);
        }
        private bool IsCertified(Vector2 start, Vector2 end)
        {
            if (!clearSegments.TryGetValue(start, out var ends)) return false;
            Vector2 delta = end - start;
            foreach (var destination in ends)
            {
                Vector2 safe = destination - start;
                float length = safe.sqrMagnitude;
                if (length <= 0f) continue;
                float t = Vector2.Dot(delta, safe) / length;
                // Only a straight prefix of a proven sweep, within floating-point precision.
                if (t >= 0f && t <= 1f && (delta - safe * t).sqrMagnitude <= 1e-12f) return true;
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // A query helper, not movement or depenetration. One shared, disposable instance is sufficient.
    // Callers synchronize geometry edits and invalidate the cache before querying.
    public sealed class StaticNavigationStartConnector : IDisposable
    {
        public const float MaxDistance = 0.5f;
        public const float ContactTolerance = StaticNavigationContactSweep.ContactTolerance;
        private readonly StaticNavigationWorldCache cache;
        private readonly StaticNavigationContactSweep sweep;
        private readonly List<Vector2Int> candidates = new List<Vector2Int>();
        private bool disposed;
#if UNITY_EDITOR
        public bool DebugCaptureEnabled { get => sweep.DebugCaptureEnabled; set => sweep.DebugCaptureEnabled = value; }
        public string DebugLastDecision { get; private set; }
        public string DebugCandidateTrace { get; private set; }
#endif

        public StaticNavigationStartConnector(StaticNavigationWorldCache cache, StaticNavigationWorld2D world,
            int queryCapacity = 64)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (queryCapacity < 1) throw new ArgumentOutOfRangeException(nameof(queryCapacity));
            if (!ReferenceEquals(cache.World, world))
                throw new ArgumentException("Connector and cache must share the same world sampler and body profile.", nameof(world));
            sweep = new StaticNavigationContactSweep(world, queryCapacity);
        }

        // Clear = connected; Blocked = invalid start/no connector; Unknown = sample/retry first.
        // Only Clear makes destination meaningful. No start cell is ever overwritten as Clear.
        public StaticNavigationSampleState TryConnect(Vector2 actualBodyCenter, out Vector2Int destination)
        {
            Validate(actualBodyCenter);
            destination = default;
#if UNITY_EDITOR
            if (DebugCaptureEnabled) DebugCandidateTrace = string.Empty;
#endif
            if (!cache.TryWorldToCell(actualBodyCenter, out var origin)) return StaticNavigationSampleState.Blocked;
            candidates.Clear();
            int range = Mathf.CeilToInt(MaxDistance / cache.CellSize) + 1;
            for (int y = origin.y - range; y <= origin.y + range; y++)
            for (int x = origin.x - range; x <= origin.x + range; x++)
            {
                var cell = new Vector2Int(x, y);
                if (cache.Bounds.Contains(new Vector3Int(x, y, 0)) &&
                    (cache.CellToWorld(cell) - actualBodyCenter).sqrMagnitude <= MaxDistance * MaxDistance)
                    candidates.Add(cell);
            }
            candidates.Sort((a, b) =>
            {
                int order = (cache.CellToWorld(a) - actualBodyCenter).sqrMagnitude.CompareTo(
                    (cache.CellToWorld(b) - actualBodyCenter).sqrMagnitude);
                if (order == 0) order = a.y.CompareTo(b.y);
                return order == 0 ? a.x.CompareTo(b.x) : order;
            });
            foreach (var cell in candidates)
            {
                var state = CheckConnection(actualBodyCenter, cell);
#if UNITY_EDITOR
                if (DebugCaptureEnabled) DebugCandidateTrace += DebugLastDecision + "\n";
#endif
                // Wait for the first undecided candidate: cache warmth must not change selection.
                if (state == StaticNavigationSampleState.Unknown) return state;
                if (state != StaticNavigationSampleState.Clear) continue;
                destination = cell;
                return state;
            }
            return StaticNavigationSampleState.Blocked;
        }

        public StaticNavigationSampleState CheckConnection(Vector2 actualBodyCenter, Vector2Int destination)
        {
            Validate(actualBodyCenter);
#if UNITY_EDITOR
            if (DebugCaptureEnabled) DebugLastDecision = $"destination={destination}; outside bounds or distance limit";
#endif
            if (!cache.Bounds.Contains(new Vector3Int(destination.x, destination.y, 0)))
                return StaticNavigationSampleState.Blocked;
            Vector2 end = cache.CellToWorld(destination);
            if ((end - actualBodyCenter).sqrMagnitude > MaxDistance * MaxDistance)
                return StaticNavigationSampleState.Blocked;
            var state = cache.GetCellState(destination);
            var result = state == StaticNavigationSampleState.Clear ? sweep.Sample(actualBodyCenter, end) : state;
#if UNITY_EDITOR
            if (DebugCaptureEnabled)
                DebugLastDecision = $"destination={destination}; start={actualBodyCenter.ToString("F6")}; " +
                    $"end={end.ToString("F6")}; cached={state}; result={result}; " +
                    (state == StaticNavigationSampleState.Clear ? sweep.DebugLastDecision : "destination clearance");
#endif
            return result;
        }

        private void Validate(Vector2 point)
        {
            if (disposed) throw new ObjectDisposedException(nameof(StaticNavigationStartConnector));
            if (float.IsNaN(point.x) || float.IsInfinity(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.y))
                throw new ArgumentOutOfRangeException(nameof(point));
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            sweep.Dispose();
        }
    }
}

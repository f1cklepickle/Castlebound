using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Call Tick once per rendered frame with that frame's monotonically increasing index.
    // No gameplay lifecycle, priority policy, background execution, or implicit Unity callbacks.
    public sealed class StaticNavigationScheduler
    {
        public const int ExpansionsPerTick = 256;
        public const int QueriesPerTick = 128;
        public const int RequestExpansionBudget = 4096;
        public const int MaxRequestCells = 4096;
        public const int MaxAxisCells = 96;
        public const int MaxOutstandingRequests = 64;
        private readonly StaticNavigationWorldCache cache;
        private readonly Queue<Request> queue = new Queue<Request>();
        private readonly Queue<KeyValuePair<int, StaticNavigationSearch>> completed = new Queue<KeyValuePair<int, StaticNavigationSearch>>();
        private StaticNavigationSearch active;
        private bool repairingCompleted;
        private int activeId, nextId = 1, lastFrame = -1;
#if UNITY_EDITOR
        public int DebugTicks { get; private set; }
        public int DebugLastExpansions { get; private set; }
        public int DebugLastSamplingQueries { get; private set; }
        public int DebugTotalExpansions { get; private set; }
#endif

        public StaticNavigationScheduler(StaticNavigationWorldCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // Rejection performs no sampling/search allocation. IDs are assigned only to accepted requests.
        public bool TrySubmit(BoundsInt bounds, Vector2Int start, Vector2Int goal, out int requestId)
        {
            requestId = 0;
            if (queue.Count + completed.Count + (active == null || repairingCompleted ? 0 : 1) >= MaxOutstandingRequests ||
                bounds.size.x <= 0 || bounds.size.y <= 0 || bounds.size.x > MaxAxisCells || bounds.size.y > MaxAxisCells ||
                (long)bounds.size.x * bounds.size.y > MaxRequestCells || bounds.zMin != 0 || bounds.size.z != 1 ||
                bounds.xMin < cache.Bounds.xMin || bounds.yMin < cache.Bounds.yMin ||
                (long)bounds.xMin + bounds.size.x > cache.Bounds.xMax ||
                (long)bounds.yMin + bounds.size.y > cache.Bounds.yMax) return false;
            requestId = nextId++;
            queue.Enqueue(new Request(requestId, bounds, start, goal));
            return true;
        }

        public bool Tick(int frameIndex, int samplingBudget = QueriesPerTick)
        {
            if (frameIndex < 0) throw new ArgumentOutOfRangeException(nameof(frameIndex));
            if (samplingBudget < 0 || samplingBudget > QueriesPerTick) throw new ArgumentOutOfRangeException(nameof(samplingBudget));
            if (frameIndex <= lastFrame) return false;
            lastFrame = frameIndex;
#if UNITY_EDITOR
            DebugTicks++;
            DebugLastExpansions = 0;
            DebugLastSamplingQueries = 0;
#endif
            if (active == null && completed.Count > 0 && completed.Peek().Value.Result == null)
            {
                var stale = completed.Peek();
                activeId = stale.Key;
                active = stale.Value;
                repairingCompleted = true;
            }
            if (active == null && queue.Count > 0)
            {
                var request = queue.Dequeue();
                activeId = request.Id;
                active = new StaticNavigationSearch(cache, request.Bounds, request.Start, request.Goal, RequestExpansionBudget);
            }
            int expansions = 0, queries = 0;
            while (active != null)
            {
                expansions += active.Step(ExpansionsPerTick - expansions);
                if (active.Result != null)
                {
                    if (!repairingCompleted)
                        completed.Enqueue(new KeyValuePair<int, StaticNavigationSearch>(activeId, active));
                    repairingCompleted = false;
                    active = null;
                    break; // Never start/publish a second request on this tick.
                }
                if (expansions >= ExpansionsPerTick || !active.WaitingForSampling || queries >= samplingBudget) break;
                int sampled = cache.SamplePending(samplingBudget - queries);
                queries += sampled;
                if (sampled == 0) break;
            }
#if UNITY_EDITOR
            DebugLastExpansions = expansions;
            DebugLastSamplingQueries = queries;
            DebugTotalExpansions += expansions;
#endif
            return true;
        }

        public bool TryGetCompleted(out int requestId, out StaticNavigationPathResult result)
        {
            requestId = 0; result = null;
            if (completed.Count == 0 || completed.Peek().Value.Result == null) return false;
            var item = completed.Dequeue();
            requestId = item.Key; result = item.Value.Result;
            return true;
        }

        private readonly struct Request
        {
            public readonly int Id;
            public readonly BoundsInt Bounds;
            public readonly Vector2Int Start, Goal;
            public Request(int id, BoundsInt bounds, Vector2Int start, Vector2Int goal)
            { Id = id; Bounds = bounds; Start = start; Goal = goal; }
        }
    }
}

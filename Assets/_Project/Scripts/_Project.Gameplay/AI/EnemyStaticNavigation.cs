using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Route/request state only. No Unity callbacks, combat decisions, or movement execution.
    public sealed class EnemyStaticNavigation
    {
        public const float ReplanInterval = 0.75f, RetryInterval = 2f, DisplacementThreshold = 1f;
        public const float ProgressDistance = 0.25f, StallGrace = 1.5f;
        private readonly IEnemyNavigationWorld world;
        private readonly List<Vector2> candidates = new List<Vector2>();
        private readonly List<Vector2> route = new List<Vector2>();
        private Transform identity;
        private EnemyTargetType type;
        private bool passage, exterior, invalidRequest;
        private Vector2 targetPosition, anchor, plannedGoal, segmentStart;
        private Quaternion targetRotation;
        private Vector3 targetScale;
        private int candidateIndex, attempt, visibilityCursor, requestId;
        private long routeRevision, requestRevision;
        private float nextPlan, stallTime, bestRemaining = float.PositiveInfinity;
        private bool goalReady;
        private bool segmentBlocked;
        private bool pendingReplan;
        public EnemyNavigationState State { get; private set; } = EnemyNavigationState.Suspended;
        public int WaypointIndex { get; private set; }
        public int RequestId => requestId;
        public int PlanningAttempt => attempt;
        public Vector2 SteeringPoint { get; private set; }
        public Vector2 PlannedGoal => plannedGoal;
        public int RouteCount => route.Count;
        public EnemyStaticNavigation(IEnemyNavigationWorld world) { this.world = world; }

        public void Suspend()
        {
            Reset(); State = EnemyNavigationState.Suspended;
        }
        public void Reset()
        {
            identity = null; route.Clear(); candidates.Clear(); goalReady = false;
            invalidRequest = requestId != 0; candidateIndex = attempt = WaypointIndex = 0;
            nextPlan = stallTime = 0f; bestRemaining = float.PositiveInfinity;
            pendingReplan = false;
            State = EnemyNavigationState.Suspended;
        }
        public void ResetAfterRecovery() { if (requestId != 0) world.Abandon(requestId); requestId = 0; Reset(); }
        public void Dispose() => ResetAfterRecovery();

        public Vector2 Compute(EnemyNavigationTarget target, Vector2 position, float speed, float dt, float now,
            bool suspended = false, bool knockback = false)
        {
            if (suspended || !target.IsValid) { Suspend(); Drain(now); return Vector2.zero; }
            bool changed = identity != target.Transform || type != target.Type || passage != target.Passage || exterior != target.ExteriorOnly;
            bool moved = identity != null && ((Vector2)target.Transform.position - targetPosition).sqrMagnitude >= 1f;
            bool barrierChanged = identity != null && target.Type == EnemyTargetType.Barrier &&
                ((Vector2)target.Transform.position != targetPosition || target.Transform.rotation != targetRotation ||
                 target.Transform.lossyScale != targetScale || target.Anchor != anchor);
            bool goalObsolete = goalReady && target.Type == EnemyTargetType.Player &&
                !EnemyNavigationGoals.WithinReach(target, plannedGoal, world.BodyRadius);
            if (changed || barrierChanged || (moved || goalObsolete) && now >= nextPlan)
            {
                Reset(); identity = target.Transform; type = target.Type; passage = target.Passage; exterior = target.ExteriorOnly;
                targetPosition = target.Transform.position; anchor = target.Anchor;
                targetRotation = target.Transform.rotation; targetScale = target.Transform.lossyScale;
            }
            Drain(now);
            if (knockback) { stallTime = 0f; bestRemaining = float.PositiveInfinity; }

            // Always test direct Player pursuit first, including when a route or request exists.
            if (target.Type == EnemyTargetType.Player || target.Passage)
            {
                var direct = world.Segment(position, target.Transform.position);
                if (direct == StaticNavigationSampleState.Clear) return Direct(position, target.Transform.position, speed, dt);
            }
            if (!goalReady)
            {
                if (candidates.Count == 0) EnemyNavigationGoals.Build(target, position, world, candidates);
                int work = 0;
                while (candidateIndex < candidates.Count && work++ < 8)
                {
                    var state = EnemyNavigationGoals.Validate(target, candidates[candidateIndex], world);
                    if (state == StaticNavigationSampleState.Unknown) return Wait();
                    if (state == StaticNavigationSampleState.Clear) { plannedGoal = candidates[candidateIndex]; goalReady = true; break; }
                    candidateIndex++;
                }
                if (!goalReady)
                {
                    if (candidateIndex >= candidates.Count && now >= nextPlan)
                    { candidateIndex = 0; candidates.Clear(); nextPlan = now + RetryInterval; }
                    return Wait();
                }
            }
            var clear = world.Segment(position, plannedGoal);
            if (clear == StaticNavigationSampleState.Clear) return Direct(position, plannedGoal, speed, dt);

            if (route.Count > 0)
            {
                if (routeRevision != world.Cache.Revision)
                {
                    // Keep the ordered route, but every selected segment is checked against current geometry.
                    routeRevision = world.Cache.Revision; visibilityCursor = route.Count - 1;
                }
                if (!knockback && (Deviation(position) > DisplacementThreshold || stallTime >= StallGrace)) pendingReplan = true;
                if (now >= nextPlan && pendingReplan && !knockback)
                    Replan(now);
                else
                {
                    Vector2 follow = Follow(position, speed, dt);
                    if (follow.sqrMagnitude > 0f && !knockback)
                    {
                        float remaining = Remaining(position);
                        if (bestRemaining - remaining >= ProgressDistance) { bestRemaining = remaining; stallTime = 0f; }
                        else stallTime += dt;
                    }
                    if (follow.sqrMagnitude > 0f || !segmentBlocked || clear == StaticNavigationSampleState.Unknown) return follow;
                    // A blocked route must not force direct wall pursuit while cooldown runs.
                    if (now >= nextPlan) Replan(now);
                    else return Wait();
                }
            }
            if (requestId != 0 || now < nextPlan || clear == StaticNavigationSampleState.Unknown) return Wait();
            var connection = world.Connect(position, out var start);
            if (connection != StaticNavigationSampleState.Clear)
            { if (connection == StaticNavigationSampleState.Blocked) nextPlan = now + RetryInterval; return Wait(); }
            if (!world.Cache.TryWorldToCell(plannedGoal, out var goal)) { Failed(now); return Wait(); }
            // The exact goal and its snapped lattice endpoint both need a safe connection.
            var goalCellState = world.Cache.GetCellState(goal);
            if (goalCellState == StaticNavigationSampleState.Unknown) return Wait();
            var goalConnection = goalCellState == StaticNavigationSampleState.Clear
                ? world.Segment(world.Cache.CellToWorld(goal), plannedGoal) : StaticNavigationSampleState.Blocked;
            if (goalConnection == StaticNavigationSampleState.Unknown) return Wait();
            if (goalConnection == StaticNavigationSampleState.Blocked)
            { candidateIndex++; goalReady = false; return Wait(); }
            if (world.TrySubmit(start, goal, attempt, out requestId))
            { requestRevision = world.Cache.Revision; invalidRequest = false; nextPlan = now + ReplanInterval; State = EnemyNavigationState.Waiting; }
            else Failed(now);
            return Wait();
        }
        private void Drain(float now)
        {
            if (requestId == 0 || !world.TryResult(requestId, out var result)) return;
            requestId = 0;
            if (invalidRequest || result.WorldRevision != world.Cache.Revision || requestRevision != result.WorldRevision)
            { invalidRequest = false; return; }
            if (result.Status != StaticNavigationPathStatus.PathFound) { Failed(now); return; }
            route.Clear(); route.AddRange(result.WorldPoints);
            if (route.Count == 0 || route[route.Count - 1] != plannedGoal) route.Add(plannedGoal);
            WaypointIndex = 0; visibilityCursor = route.Count - 1; routeRevision = result.WorldRevision;
            segmentStart = route[0];
            bestRemaining = float.PositiveInfinity; stallTime = 0f; State = EnemyNavigationState.Following;
            pendingReplan = false;
        }
        private void Failed(float now)
        {
            route.Clear(); attempt++;
            if (attempt > 2) { attempt = 0; candidateIndex++; goalReady = false; }
            nextPlan = now + RetryInterval; State = EnemyNavigationState.RegionFailed;
        }
        private void Replan(float now)
        {
            route.Clear(); invalidRequest = requestId != 0; nextPlan = Mathf.Max(nextPlan, now);
            bestRemaining = float.PositiveInfinity; stallTime = 0f;
            pendingReplan = false;
        }
        private Vector2 Direct(Vector2 position, Vector2 goal, float speed, float dt)
        {
            route.Clear(); invalidRequest = requestId != 0; attempt = 0; stallTime = 0f;
            State = EnemyNavigationState.Direct; SteeringPoint = goal;
            return Velocity(position, goal, speed, dt);
        }
        private Vector2 Wait()
        {
            if (State != EnemyNavigationState.RegionFailed) State = EnemyNavigationState.Waiting;
            return Vector2.zero;
        }
        private Vector2 Follow(Vector2 position, float speed, float dt)
        {
            segmentBlocked = false;
            int checks = 0;
            for (; visibilityCursor >= WaypointIndex && checks++ < 4; visibilityCursor--)
            {
                var state = world.Segment(position, route[visibilityCursor]);
                if (state == StaticNavigationSampleState.Unknown) break;
                if (state != StaticNavigationSampleState.Clear) continue;
                if (WaypointIndex != visibilityCursor) segmentStart = position;
                WaypointIndex = visibilityCursor; break;
            }
            var segmentState = world.Segment(position, route[WaypointIndex]);
            if (segmentState != StaticNavigationSampleState.Clear)
            {
                // Contact starts use ONLY the short, validated connector before ordinary following.
                if (WaypointIndex != 0) { segmentBlocked = segmentState == StaticNavigationSampleState.Blocked; return Vector2.zero; }
                var connection = world.Connect(position, out var cell);
                if (connection != StaticNavigationSampleState.Clear || world.Cache.CellToWorld(cell) != route[0])
                { segmentBlocked = connection == StaticNavigationSampleState.Blocked; return Vector2.zero; }
            }
            SteeringPoint = route[WaypointIndex]; State = EnemyNavigationState.Following;
            Vector2 velocity = Velocity(position, SteeringPoint, speed, dt);
            if ((SteeringPoint - position).sqrMagnitude <= 0.0025f && WaypointIndex < route.Count - 1)
            { WaypointIndex++; if (visibilityCursor < WaypointIndex) visibilityCursor = route.Count - 1; }
            else if (visibilityCursor <= WaypointIndex) visibilityCursor = route.Count - 1;
            return velocity;
        }
        private float Remaining(Vector2 position)
        {
            float value = Vector2.Distance(position, route[WaypointIndex]);
            for (int i = WaypointIndex + 1; i < route.Count; i++) value += Vector2.Distance(route[i - 1], route[i]);
            return value;
        }
        private float Deviation(Vector2 position)
        {
            float result = DistanceToSegment(position, segmentStart, route[WaypointIndex]);
            for (int i = WaypointIndex + 1; i < route.Count; i++)
                result = Mathf.Min(result, DistanceToSegment(position, route[i - 1], route[i]));
            return result;
        }
        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 delta = b - a;
            float t = delta.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, delta) / delta.sqrMagnitude) : 0f;
            return Vector2.Distance(p, a + delta * t);
        }
        private static Vector2 Velocity(Vector2 position, Vector2 goal, float speed, float dt)
            => dt > 0f ? (goal - position).normalized * Mathf.Min(Mathf.Max(0f, speed), Vector2.Distance(position, goal) / dt) : Vector2.zero;
    }
}

using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Per-enemy bounded search/state. Movement still belongs to EnemyLocomotion.
    public sealed class EnemyNavigationRecovery
    {
        public const float MaxDistance = 1f, RingSpacing = 0.0625f, RetryDelay = 0.5f;
        public const int DirectionCount = 16, RingCount = 16, CandidatesPerTick = 4;
        private Vector2 origin, outward, destination, progressPosition;
        private int candidateIndex;
        private float retryAt, progressAt, stepLimit;
        private bool hasDestination;
        public bool IsActive { get; private set; }
        public bool HasDestination => hasDestination;
        public Vector2 Destination => destination;
        public int CandidateIndex => candidateIndex;
        public float RetryAt => retryAt;

        public void Reset()
        { IsActive = hasDestination = false; candidateIndex = 0; retryAt = stepLimit = 0f; }

        // True owns this CHASE step (including safe waiting); false resumes ordinary navigation.
        public bool Compute(StaticNavigationRecoveryQueries queries, CircleCollider2D body,
            float speed, float dt, float now, out Vector2 velocity, Func<bool> spend = null, bool paused = false)
        {
            velocity = Vector2.zero; stepLimit = 0f;
            Vector2 center = body.bounds.center;
            if (!IsActive)
            {
                var state = queries.Detect(body, out var normal, spend);
                if (state == StaticNavigationSampleState.Unknown) return true;
                if (state == StaticNavigationSampleState.Clear) return false;
                IsActive = true; origin = progressPosition = center; outward = normal;
                candidateIndex = 0; retryAt = 0f; progressAt = now;
            }
            // Finish only at normal clearance, not merely after crossing the penetration tolerance.
            var current = queries.Destination(body, center, spend);
            if (current == StaticNavigationSampleState.Clear) { Reset(); return false; }
            if (current == StaticNavigationSampleState.Unknown || paused || now < retryAt) return true;
            if ((center - origin).sqrMagnitude > MaxDistance * MaxDistance)
            { Wait(now); return true; }
            if (!hasDestination)
            {
                for (int work = 0; work < CandidatesPerTick && candidateIndex < RingCount * DirectionCount; work++)
                {
                    Vector2 candidate = Candidate(origin, outward, candidateIndex);
                    var result = queries.Destination(body, candidate, spend);
                    if (result == StaticNavigationSampleState.Clear) result = queries.Sweep(body, candidate, spend);
                    if (result == StaticNavigationSampleState.Unknown) return true; // Do not skip an undecided nearer candidate.
                    candidateIndex++;
                    if (result != StaticNavigationSampleState.Clear) continue;
                    destination = candidate; hasDestination = true; progressPosition = center; progressAt = now;
                    break;
                }
                if (!hasDestination)
                { if (candidateIndex >= RingCount * DirectionCount) Wait(now); return true; }
            }
            var safe = queries.Destination(body, destination, spend);
            if (safe == StaticNavigationSampleState.Clear) safe = queries.Sweep(body, destination, spend);
            if (safe != StaticNavigationSampleState.Clear)
            { if (safe == StaticNavigationSampleState.Blocked) Wait(now); return true; }
            if ((center - progressPosition).sqrMagnitude >= 0.0001f)
            { progressPosition = center; progressAt = now; }
            else if (now - progressAt >= 1f) { Wait(now); return true; }
            stepLimit = Mathf.Min(Mathf.Max(0f, speed) * Mathf.Max(0f, dt), Vector2.Distance(center, destination));
            if (dt > 0f) velocity = (destination - center).normalized * (stepLimit / dt);
            return true;
        }

        // Revalidate AFTER #277 changes the displacement; no cached exception to the normal guard.
        public Vector2 Constrain(StaticNavigationRecoveryQueries queries, CircleCollider2D body,
            Vector2 displacement, Func<bool> spend = null)
        {
            if (!IsActive || !hasDestination || displacement.sqrMagnitude == 0f) return Vector2.zero;
            Vector2 center = body.bounds.center, end = center + displacement;
            if (displacement.magnitude > stepLimit + 0.000001f ||
                (end - origin).sqrMagnitude > MaxDistance * MaxDistance ||
                Vector2.Dot(displacement, destination - center) <= 0f) return Vector2.zero;
            return queries.Sweep(body, end, spend) == StaticNavigationSampleState.Clear ? displacement : Vector2.zero;
        }

        public static Vector2 Candidate(Vector2 center, Vector2 normal, int index)
        {
            if (index < 0 || index >= RingCount * DirectionCount) throw new ArgumentOutOfRangeException(nameof(index));
            // Ascending distance; ties use outward, then successive counterclockwise 22.5-degree directions.
            float angle = Mathf.Atan2(normal.y, normal.x) + (index % DirectionCount) * 2f * Mathf.PI / DirectionCount;
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ((index / DirectionCount + 1) * RingSpacing);
        }
        private void Wait(float now)
        { hasDestination = false; candidateIndex = 0; stepLimit = 0f; retryAt = now + RetryDelay; }
    }
}

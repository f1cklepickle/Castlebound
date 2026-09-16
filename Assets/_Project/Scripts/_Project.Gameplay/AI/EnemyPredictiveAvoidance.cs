using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Experimental velocity selection. Never changes a collider or applies movement.
    public sealed class EnemyPredictiveAvoidance
    {
        public const float PredictionHorizon = 0.6f;
        public const float MaximumTurnDegrees = 75f;
        private const float AngleStepDegrees = 15f;
        private const float PreferredRadiusMultiplier = 2.5f;
        private const float NearTieSpeedSquaredRatio = 0.02f;
        private int preferredSide = 1;

        public void Reset()
        {
            preferredSide = 1;
        }

        public static float GetPreferredRadius(float hardRadius, int crowdCount)
        {
            return hardRadius * (crowdCount < 8 ? PreferredRadiusMultiplier : 1f);
        }

        public Vector2 SelectVelocity(Vector2 position, Vector2 preferredVelocity,
            float hardRadius, IReadOnlyList<EnemyAvoidanceNeighbor> neighbors)
        {
            float speed = preferredVelocity.magnitude;
            if (speed <= 0.000001f)
                return Vector2.zero;
            float preferredScale = GetPreferredRadius(1f, neighbors.Count + 1);

            // The preference may yield even below eight: it is not a hard movement boundary.
            for (int relaxation = 0; relaxation < 3; relaxation++)
            {
                float scale = Mathf.Lerp(preferredScale, 1f, relaxation * 0.5f);
                if (TrySample(position, preferredVelocity, hardRadius, neighbors, scale, out Vector2 result))
                    return result;
                if (preferredScale == 1f) break;
            }
            // Completely enclosed: no safe forward sample exists. #277 still owns recovery.
            return Vector2.zero;
        }

        private bool TrySample(Vector2 position, Vector2 preferredVelocity, float hardRadius,
            IReadOnlyList<EnemyAvoidanceNeighbor> neighbors, float radiusScale, out Vector2 result)
        {
            result = Vector2.zero;
            float speed = preferredVelocity.magnitude;
            float bestCost = float.PositiveInfinity;
            int bestSide = 0;
            bool found = false;
            float tieTolerance = speed * speed * NearTieSpeedSquaredRatio;
            // Curve at authored speed whenever possible; braking is a fallback.
            for (int speedStep = 0; speedStep < 3; speedStep++)
            {
                float speedScale = 1f - speedStep * 0.25f;
                for (int step = -(int)(MaximumTurnDegrees / AngleStepDegrees);
                    step <= (int)(MaximumTurnDegrees / AngleStepDegrees); step++)
                {
                    float angle = step * AngleStepDegrees * Mathf.Deg2Rad;
                    Vector2 sample = new Vector2(
                        preferredVelocity.x * Mathf.Cos(angle) - preferredVelocity.y * Mathf.Sin(angle),
                        preferredVelocity.x * Mathf.Sin(angle) + preferredVelocity.y * Mathf.Cos(angle)) * speedScale;
                    if (!IsSafe(position, sample, hardRadius, neighbors, radiusScale))
                        continue;
                    float cost = (sample - preferredVelocity).sqrMagnitude;
                    int sampleSide = step == 0 ? 0 : step > 0 ? 1 : -1;
                    bool winsTie = Mathf.Abs(cost - bestCost) <= tieTolerance &&
                        sampleSide == preferredSide && bestSide != preferredSide;
                    if (!found || cost < bestCost - tieTolerance || winsTie)
                    {
                        found = true;
                        bestCost = cost;
                        result = Vector2.ClampMagnitude(sample, speed);
                        bestSide = sampleSide;
                    }
                }

                if (found)
                {
                    if (bestSide != 0) preferredSide = bestSide;
                    return true;
                }
            }
            return false;
        }

        public static bool IsSafe(Vector2 position, Vector2 velocity, float hardRadius,
            IReadOnlyList<EnemyAvoidanceNeighbor> neighbors, float radiusScale)
        {
            foreach (var neighbor in neighbors)
            {
                Vector2 delta = neighbor.Position - position;
                Vector2 relativeVelocity = velocity - neighbor.Velocity;
                float radius = (hardRadius + neighbor.HardRadius) * Mathf.Max(1f, radiusScale);
                float speedSquared = relativeVelocity.sqrMagnitude;
                float closestTime = speedSquared > 0.000001f
                    ? Mathf.Clamp(Vector2.Dot(delta, relativeVelocity) / speedSquared, 0f, PredictionHorizon)
                    : 0f;
                float closestSquared = (delta - relativeVelocity * closestTime).sqrMagnitude;
                // Inside a preference already: allow non-closing motion, never demand expansion.
                float requiredSquared = Mathf.Min(radius * radius, delta.sqrMagnitude);
                if (closestSquared < requiredSquared - 0.000001f)
                    return false;
            }
            return true;
        }
    }
}

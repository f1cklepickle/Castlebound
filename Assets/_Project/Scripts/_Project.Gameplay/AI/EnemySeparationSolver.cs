using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Owns displacement only, never Unity transforms/rigidbodies. Guard must return a safe
    // prefix (or zero) of its input. Components share one final time fraction so reducing
    // a neighbour cannot invalidate a previously checked relative trajectory.
    public sealed class EnemySeparationSolver
    {
        private const float Skin = 0.0002f;
        private readonly EnemyOverlapRecovery recovery = new EnemyOverlapRecovery();
        private readonly int[] parents = new int[EnemySeparationDiscovery.BodyCapacity];
        private readonly float[] fractions = new float[EnemySeparationDiscovery.BodyCapacity];
        public int RecoveryPairs { get; private set; }

        public void Solve(EnemySeparationBody[] bodies, int count, EnemySeparationDiscovery discovery,
            Func<int, Vector2, Vector2> guard)
        {
            RecoveryPairs = 0;
            for (int i = 0; i < count; i++) bodies[i].Displacement = Bounded(bodies[i], bodies[i].Desired);
            if (!discovery.Build(bodies, count))
            { for (int i = 0; i < count; i++) bodies[i].Displacement = Vector2.zero; return; }
            for (int i = 0; i < count; i++) { parents[i] = i; fractions[i] = 1f; }
            for (int p = 0; p < discovery.PairCount; p++)
            {
                var pair = discovery.Pairs[p]; Union(pair.First, pair.Second);
                if (Overlapped(bodies[pair.First], bodies[pair.Second])) RecoveryPairs++;
            }
            PreventContacts(bodies, discovery);
            for (int i = 0; i < count; i++) bodies[i].Displacement = Guard(i, bodies[i].Displacement, bodies, guard);
            if (RecoveryPairs > 0) recovery.Resolve(bodies, count, discovery.Pairs, discovery.PairCount, guard);
            // This is also the post-world-guard revalidation: use only the displacements
            // that actually survived the guard, never another controller's speculative endpoint.
            ConstrainComponents(bodies, count, discovery);
        }

        private void ConstrainComponents(EnemySeparationBody[] bodies, int count, EnemySeparationDiscovery discovery)
        {
            for (int i = 0; i < count; i++) fractions[i] = 1f;
            for (int p = 0; p < discovery.PairCount; p++)
            {
                var pair = discovery.Pairs[p]; int a = pair.First, b = pair.Second;
                float fraction = SafeFraction(bodies[b].Position - bodies[a].Position,
                    bodies[b].Displacement - bodies[a].Displacement, bodies[a].Radius + bodies[b].Radius);
                int root = Find(a); fractions[root] = Mathf.Min(fractions[root], fraction);
            }
            for (int i = 0; i < count; i++) bodies[i].Displacement *= fractions[Find(i)];
        }

        private static void PreventContacts(EnemySeparationBody[] bodies, EnemySeparationDiscovery discovery)
        {
            // Remove only closing motion, preserving common translation and tangent.
            // Every edit reduces a positive inward component, so it cannot increase speed.
            // Other pair constraints may be affected; bounded passes end in swept revalidation.
            for (int pass = 0; pass < 4; pass++)
            {
                bool changed = false;
                for (int p = 0; p < discovery.PairCount; p++)
                {
                    var pair = discovery.Pairs[p]; int a = pair.First, b = pair.Second;
                    Vector2 delta = bodies[b].Position - bodies[a].Position;
                    float distance = delta.magnitude, radius = bodies[a].Radius + bodies[b].Radius;
                    if (distance < radius - Skin || distance <= 0.00001f) continue;
                    Vector2 axis = delta / distance;
                    Vector2 da = bodies[a].Displacement, db = bodies[b].Displacement;
                    if (SafeFraction(delta, db - da, radius) >= 1f) continue;
                    float inwardA = Vector2.Dot(da, axis), inwardB = Vector2.Dot(db, -axis);
                    float excess = inwardA + inwardB - Mathf.Max(0f, distance - radius - Skin);
                    if (excess <= 0f) continue;
                    EnemySeparationMath.AllocateCorrections(excess, Mathf.Max(0f, inwardA), Mathf.Max(0f, inwardB),
                        out float removeA, out float removeB);
                    bodies[a].Displacement = da - axis * removeA;
                    bodies[b].Displacement = db + axis * removeB;
                    changed |= removeA + removeB > 0f;
                }
                if (!changed) break;
            }
        }

        public static float SafeFraction(Vector2 delta, Vector2 relativeStep, float radius)
        {
            double a = Vector2.Dot(relativeStep, relativeStep), b = Vector2.Dot(delta, relativeStep);
            double distanceSquared = Vector2.Dot(delta, delta);
            if (a < 1e-16) return 1f;
            if (distanceSquared < radius * radius)
                return b < -1e-10 ? 0f : 1f; // Existing overlap may separate, never initially deepen.
            if (b >= 0.0) return 1f;
            double safeRadius = radius + Skin;
            double c = distanceSquared - safeRadius * safeRadius;
            if (c <= 0.0) return 0f;
            double discriminant = b * b - a * c;
            if (discriminant <= 0.0) return 1f;
            return Mathf.Clamp01((float)((-b - Math.Sqrt(discriminant)) / a));
        }

        private static bool Overlapped(EnemySeparationBody a, EnemySeparationBody b)
            => (b.Position - a.Position).sqrMagnitude < (a.Radius + b.Radius - Skin) * (a.Radius + b.Radius - Skin);
        private static Vector2 Bounded(EnemySeparationBody body, Vector2 desired)
            => body.Locked ? Vector2.zero : Vector2.ClampMagnitude(desired, Mathf.Max(0f, body.Budget));
        private static Vector2 Guard(int i, Vector2 desired, EnemySeparationBody[] bodies, Func<int, Vector2, Vector2> guard)
        {
            Vector2 bounded = Bounded(bodies[i], desired);
            return guard != null && bounded.sqrMagnitude > 0f ? Bounded(bodies[i], guard(i, bounded)) : bounded;
        }
        private int Find(int i) { while (parents[i] != i) { parents[i] = parents[parents[i]]; i = parents[i]; } return i; }
        private void Union(int a, int b) { a = Find(a); b = Find(b); if (a != b) parents[Mathf.Max(a, b)] = Mathf.Min(a, b); }
    }
}

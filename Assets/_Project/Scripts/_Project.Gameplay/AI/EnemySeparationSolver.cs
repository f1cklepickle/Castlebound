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
        private const int RecoveryPasses = 4;
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
            ConstrainComponents(bodies, count, discovery);
            // Exceptional recovery replaces locomotion for the affected pair. Multiple
            // constraints are rechecked below; an infeasible pocket waits rather than launches.
            for (int pass = 0; pass < RecoveryPasses; pass++)
            {
                bool changed = false;
                for (int p = 0; p < discovery.PairCount; p++)
                {
                var pair = discovery.Pairs[p]; int a = pair.First, b = pair.Second;
                if (!Overlapped(bodies[a], bodies[b]) || bodies[a].External.sqrMagnitude > 0f || bodies[b].External.sqrMagnitude > 0f) continue;
                Vector2 delta = bodies[b].Position - bodies[a].Position;
                Vector2 axis = EnemySeparationMath.ResolveAxis(delta, 0.00001f);
                float needed = bodies[a].Radius + bodies[b].Radius + Skin - delta.magnitude;
                float capacityA = bodies[a].Locked ? 0f : bodies[a].Budget;
                float capacityB = bodies[b].Locked ? 0f : bodies[b].Budget;
                EnemySeparationMath.AllocateCorrections(needed, capacityA, capacityB, out float moveA, out float moveB);
                Vector2 allowedA = Guard(a, -axis * moveA, bodies, guard);
                Vector2 allowedB = Guard(b, axis * moveB, bodies, guard);
                float remaining = Mathf.Max(0f, needed - Vector2.Dot(allowedB - allowedA, axis));
                if (remaining > Skin)
                {
                    allowedA = Guard(a, -axis * Mathf.Min(capacityA, moveA + remaining), bodies, guard);
                    remaining = Mathf.Max(0f, needed - Vector2.Dot(allowedB - allowedA, axis));
                    allowedB = Guard(b, axis * Mathf.Min(capacityB, moveB + remaining), bodies, guard);
                }
                changed |= (bodies[a].Displacement - allowedA).sqrMagnitude > 1e-10f ||
                    (bodies[b].Displacement - allowedB).sqrMagnitude > 1e-10f;
                bodies[a].Displacement = allowedA; bodies[b].Displacement = allowedB;
                }
                if (!changed) break;
            }
            for (int i = 0; i < count; i++) bodies[i].Displacement = Guard(i, bodies[i].Displacement, bodies, guard);
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

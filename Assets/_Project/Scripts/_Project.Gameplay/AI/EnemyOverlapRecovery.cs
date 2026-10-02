using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Exceptional recovery only. Coordinate improvement over discovered local components:
    // accept a guarded body step only if ALL adjacent constraints remain satisfied and no
    // previously achieved overlap clearance is lost. No persistent correction/debt state.
    public sealed class EnemyOverlapRecovery
    {
        private const float Skin = 0.0002f;
        private const int Passes = 4;
        private readonly int[] parents = new int[EnemySeparationDiscovery.BodyCapacity];
        private readonly int[] order = new int[EnemySeparationDiscovery.BodyCapacity];
        private readonly int[] ids = new int[EnemySeparationDiscovery.BodyCapacity];
        private readonly bool[] active = new bool[EnemySeparationDiscovery.BodyCapacity];
        private readonly int[] heads = new int[EnemySeparationDiscovery.BodyCapacity];
        private readonly int[] edges = new int[16384], next = new int[16384];
        private static readonly Vector2[] directions = { Vector2.left, Vector2.right, Vector2.up, Vector2.down,
            new Vector2(-1f, 1f).normalized, new Vector2(1f, 1f).normalized,
            new Vector2(-1f, -1f).normalized, new Vector2(1f, -1f).normalized };

        public void Resolve(EnemySeparationBody[] bodies, int count, EnemySeparationPair[] pairs, int pairCount,
            Func<int, Vector2, Vector2> guard)
        {
            // Canonicalize explicitly: callers and tests need not supply pair/body order.
            Array.Sort(pairs, 0, pairCount);
            for (int i = 0; i < count; i++) { parents[i] = order[i] = i; ids[i] = bodies[i].Id; active[i] = false; heads[i] = -1; }
            Array.Sort(ids, order, 0, count);
            if (pairCount > edges.Length / 2)
            { for (int i = 0; i < count; i++) bodies[i].Displacement = Vector2.zero; return; }
            for (int p = 0; p < pairCount; p++)
            {
                var pair = pairs[p]; int a = Find(pair.First), b = Find(pair.Second);
                if (a != b) parents[Mathf.Max(a, b)] = Mathf.Min(a, b);
                edges[2 * p] = edges[2 * p + 1] = p;
                next[2 * p] = heads[pair.First]; heads[pair.First] = 2 * p;
                next[2 * p + 1] = heads[pair.Second]; heads[pair.Second] = 2 * p + 1;
            }
            for (int p = 0; p < pairCount; p++)
                if (Overlapped(bodies[pairs[p].First], bodies[pairs[p].Second])) active[Find(pairs[p].First)] = true;
            // Begin with a jointly feasible zero recovery plan, not competing pair proposals.
            for (int i = 0; i < count; i++) if (active[Find(i)]) bodies[i].Displacement = Vector2.zero;
            for (int pass = 0; pass < Passes; pass++)
            {
                bool changed = false;
                for (int ordinal = 0; ordinal < count; ordinal++)
                {
                    int i = order[ordinal];
                    if (!active[Find(i)] || bodies[i].Locked || bodies[i].Budget <= 0f || bodies[i].External.sqrMagnitude > 0f) continue;
                    Vector2 outward = Vector2.zero;
                    bool hasRecoveryRequirement = false;
                    for (int edge = heads[i]; edge >= 0; edge = next[edge])
                    {
                        var pair = pairs[edges[edge]]; int j = pair.First == i ? pair.Second : pair.First;
                        if (!Overlapped(bodies[i], bodies[j]) || bodies[j].External.sqrMagnitude > 0f) continue;
                        hasRecoveryRequirement = true;
                        Vector2 delta = bodies[i].Position - bodies[j].Position;
                        Vector2 axis = delta.sqrMagnitude > 1e-10f ? delta.normalized : (bodies[i].Id < bodies[j].Id ? Vector2.left : Vector2.right);
                        outward += axis * (bodies[i].Radius + bodies[j].Radius + Skin - delta.magnitude);
                    }
                    if (!hasRecoveryRequirement) continue;
                    Vector2 best = bodies[i].Displacement;
                    float bestGain = 0f;
                    // Sum of overlap requirements first, then bounded escape alternatives
                    // for wall-adjacent or symmetric chains. No speed is added to the budget.
                    for (int candidate = -1; candidate < directions.Length; candidate++)
                    {
                        Vector2 direction = candidate < 0 ? outward.normalized : directions[candidate];
                        if (direction.sqrMagnitude == 0f) continue;
                        Vector2 proposal = direction * bodies[i].Budget;
                        if (guard != null) proposal = guard(i, proposal);
                        proposal = Vector2.ClampMagnitude(proposal, bodies[i].Budget);
                        float gain = Gain(i, proposal, bodies, pairs);
                        if (gain <= bestGain + 1e-7f) continue;
                        bestGain = gain; best = proposal;
                    }
                    if (bestGain <= 0f) continue;
                    // Keep this already-guarded proposal even if a later candidate exhausts
                    // the query budget. Re-guarding it could invalidate the joint plan.
                    bodies[i].Displacement = best; changed = true;
                }
                if (!changed) break;
            }
        }

        private float Gain(int i, Vector2 proposed, EnemySeparationBody[] bodies, EnemySeparationPair[] pairs)
        {
            float gain = 0f;
            for (int edge = heads[i]; edge >= 0; edge = next[edge])
            {
                var pair = pairs[edges[edge]]; int j = pair.First == i ? pair.Second : pair.First;
                Vector2 delta = bodies[j].Position - bodies[i].Position;
                Vector2 relative = bodies[j].Displacement - proposed;
                float radius = bodies[i].Radius + bodies[j].Radius;
                if (EnemySeparationSolver.SafeFraction(delta, relative, radius) < 1f) return float.NegativeInfinity;
                if (!Overlapped(bodies[i], bodies[j])) continue;
                float before = (delta + bodies[j].Displacement - bodies[i].Displacement).magnitude;
                float after = (delta + relative).magnitude;
                if (after < before - 1e-7f) return float.NegativeInfinity;
                if (bodies[j].External.sqrMagnitude == 0f)
                    gain += Mathf.Min(radius + Skin, after) - Mathf.Min(radius + Skin, before);
            }
            return gain;
        }
        private static bool Overlapped(EnemySeparationBody a, EnemySeparationBody b)
            => Vector2.Distance(a.Position, b.Position) < a.Radius + b.Radius - Skin;
        private int Find(int i) { while (parents[i] != i) { parents[i] = parents[parents[i]]; i = parents[i]; } return i; }
    }
}

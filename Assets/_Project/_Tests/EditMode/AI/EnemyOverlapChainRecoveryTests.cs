using System;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyOverlapChainRecoveryTests
    {
        [Test]
        public void ThreeBodyExactStack_RecoversWithoutDebtOrSpeedExcess()
            => Recover(new[] { Body(1, 0f), Body(2, 0f), Body(3, 0f) }, null);

        [Test]
        public void ThreeBodyChain_DoesNotRebreakEarlierPairs()
            => Recover(new[] { Body(1, 0f), Body(2, 0.25f), Body(3, 0.5f) }, null);

        [Test]
        public void LockedMiddle_RemainsFixedWhileNeighborsRecover()
        {
            var bodies = new[] { Body(1, 0f), Body(2, 0.15f), Body(3, 0.3f) };
            bodies[1].Locked = true;
            Recover(bodies, null);
            Assert.That(bodies[1].Position, Is.EqualTo(Vector2.right * 0.15f));
        }

        [Test]
        public void WallAdjacentStack_RecoversOnLegalSides()
        {
            var bodies = new[] { Body(1, 0f), Body(2, 0f), Body(3, 0f) };
            Recover(bodies, (i, d) => d.x < 0f && bodies[i].Position.x + d.x < 0f ? Vector2.zero : d);
            foreach (var body in bodies) Assert.That(body.Position.x, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void FullyGuardedChain_WaitsWithoutMovingOrAccumulatingDebt()
        {
            var bodies = new[] { Body(1, 0f), Body(2, 0.15f), Body(3, 0.3f) };
            var solver = new EnemySeparationSolver(); var discovery = new EnemySeparationDiscovery();
            for (int tick = 0; tick < 8; tick++)
            {
                solver.Solve(bodies, 3, discovery, (i, d) => Vector2.zero);
                foreach (var body in bodies) Assert.That(body.Displacement, Is.EqualTo(Vector2.zero));
            }
            Recover(bodies, null);
        }

        [TestCase(0f)]
        [TestCase(0.15f)]
        public void ReversedPairAndBodyOrder_ProducesSameRecoveryById(float spacing)
        {
            var forward = new[] { Body(1, 0f), Body(2, spacing), Body(3, 2f * spacing) };
            var reverse = (EnemySeparationBody[])forward.Clone(); Array.Reverse(reverse);
            var a = new EnemySeparationDiscovery(); var b = new EnemySeparationDiscovery();
            var first = new EnemyOverlapRecovery(); var second = new EnemyOverlapRecovery();
            for (int tick = 0; tick < 24; tick++)
            {
                for (int i = 0; i < 3; i++) forward[i].Displacement = reverse[i].Displacement = Vector2.zero;
                Assert.IsTrue(a.Build(forward, 3)); Assert.IsTrue(b.Build(reverse, 3));
                Array.Reverse(b.Pairs, 0, b.PairCount);
                first.Resolve(forward, 3, a.Pairs, a.PairCount, null);
                second.Resolve(reverse, 3, b.Pairs, b.PairCount, null);
                for (int i = 0; i < 3; i++)
                {
                    Assert.That(Vector2.Distance(forward[i].Displacement, reverse[2 - i].Displacement), Is.LessThan(0.000001f));
                    forward[i].Position += forward[i].Displacement;
                    reverse[2 - i].Position += reverse[2 - i].Displacement;
                }
            }
        }

        private static void Recover(EnemySeparationBody[] bodies, Func<int, Vector2, Vector2> guard)
        {
            var solver = new EnemySeparationSolver(); var discovery = new EnemySeparationDiscovery();
            for (int tick = 0; tick < 80; tick++)
            {
                solver.Solve(bodies, bodies.Length, discovery, guard);
                for (int a = 0; a < bodies.Length; a++)
                {
                    Assert.That(bodies[a].Displacement.magnitude, Is.LessThanOrEqualTo(bodies[a].Budget + 0.00001f));
                    if (bodies[a].Locked) Assert.That(bodies[a].Displacement, Is.EqualTo(Vector2.zero));
                    for (int b = a + 1; b < bodies.Length; b++)
                    {
                        float before = Vector2.Distance(bodies[a].Position, bodies[b].Position);
                        for (int sample = 1; sample <= 8; sample++)
                            Assert.That(Vector2.Distance(bodies[a].Position + bodies[a].Displacement * (sample / 8f),
                                bodies[b].Position + bodies[b].Displacement * (sample / 8f)),
                                Is.GreaterThanOrEqualTo(Mathf.Min(before, 0.4f) - 0.00001f));
                    }
                }
                for (int i = 0; i < bodies.Length; i++) bodies[i].Position += bodies[i].Displacement;
            }
            for (int a = 0; a < bodies.Length; a++)
            for (int b = a + 1; b < bodies.Length; b++)
                Assert.That(Vector2.Distance(bodies[a].Position, bodies[b].Position), Is.GreaterThanOrEqualTo(0.3999f));
        }
        private static EnemySeparationBody Body(int id, float x)
            => new EnemySeparationBody { Id = id, Position = Vector2.right * x, Radius = 0.2f, Budget = 0.02f };
    }
}

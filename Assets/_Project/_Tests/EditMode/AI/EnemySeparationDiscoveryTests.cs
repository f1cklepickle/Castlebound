using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySeparationDiscoveryTests
    {
        [Test]
        public void UntouchedAsymmetricPair_IsFoundBeforeContact_WithoutGroupFiltering()
        {
            var bodies = new[] { Body(20, new Vector2(-0.3f, 0f), 0.16f), Body(10, new Vector2(0.3f, 0f), 0.06f) };
            var discovery = new EnemySeparationDiscovery();
            Assert.IsTrue(discovery.Build(bodies, 2));
            Assert.That(discovery.PairCount, Is.EqualTo(1));
            Assert.That(discovery.Pairs[0].FirstId, Is.EqualTo(10));
            Assert.That(discovery.Pairs[0].SecondId, Is.EqualTo(20));
            // Discovery deliberately has no target/type/avoidance-group input.
        }

        [Test]
        public void CrossingEnvelopes_AndReversedSubmission_ProduceSameOrderedPairs()
        {
            var bodies = new[] { Body(3, Vector2.left, 1f), Body(1, Vector2.down, 1f), Body(2, Vector2.right * 20f, 0.1f) };
            var discovery = new EnemySeparationDiscovery();
            Assert.IsTrue(discovery.Build(bodies, bodies.Length));
            Assert.That(discovery.PairCount, Is.EqualTo(1));
            Assert.That(discovery.Pairs[0].FirstId, Is.EqualTo(1));
            Assert.That(discovery.Pairs[0].SecondId, Is.EqualTo(3));
            System.Array.Reverse(bodies);
            Assert.IsTrue(discovery.Build(bodies, bodies.Length));
            Assert.That(discovery.PairCount, Is.EqualTo(1));
            Assert.That(discovery.Pairs[0].FirstId, Is.EqualTo(1));
            Assert.That(discovery.Pairs[0].SecondId, Is.EqualTo(3));
        }

        [Test]
        public void Saturation_IsExplicit_AndNextBuildCanRecover()
        {
            var discovery = new EnemySeparationDiscovery(pairCapacity: 1);
            var bodies = new[] { Body(1, Vector2.zero, 0.1f), Body(2, Vector2.zero, 0.1f), Body(3, Vector2.zero, 0.1f) };
            Assert.IsFalse(discovery.Build(bodies, 3));
            Assert.IsTrue(discovery.Saturated);
            Assert.IsTrue(discovery.Build(bodies, 2));
            Assert.IsFalse(discovery.Saturated);
            Assert.That(discovery.PairCount, Is.EqualTo(1));
        }

        [Test]
        public void ExternalTrajectory_IsIncluded_AndDistantBodiesDoNotBecomePairs()
        {
            var bodies = new[] { Body(1, Vector2.zero, 0f), Body(2, Vector2.right * 2f, 0f), Body(3, Vector2.right * 30f, 0f) };
            bodies[0].External = Vector2.right * 2f;
            var discovery = new EnemySeparationDiscovery();
            Assert.IsTrue(discovery.Build(bodies, 3));
            Assert.That(discovery.PairCount, Is.EqualTo(1));
        }

        private static EnemySeparationBody Body(int id, Vector2 position, float budget)
            => new EnemySeparationBody { Id = id, Position = position, Radius = 0.2f, Budget = budget };
    }
}

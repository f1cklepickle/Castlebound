using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySeparationSolverTests
    {
        [TestCase(false)] [TestCase(true)]
        public void HeadOnAsymmetricFirstContact_IsSweptAndSpeedBounded(bool reverse)
        {
            var bodies = new[] { Body(1, Vector2.zero, Vector2.right * 0.16f), Body(2, Vector2.right * 0.45f, Vector2.left * 0.06f) };
            if (reverse) System.Array.Reverse(bodies);
            Solve(bodies);
            AssertSafe(bodies);
            Assert.That(bodies[0].Displacement.magnitude + bodies[1].Displacement.magnitude, Is.GreaterThan(0f));
        }

        [Test]
        public void CrossingWithClearEndpoints_StopsBeforeInteriorCollision()
        {
            var bodies = new[] { Body(1, Vector2.left, Vector2.right * 2f), Body(2, Vector2.down, Vector2.up * 2f) };
            Solve(bodies); AssertSafe(bodies);
            Assert.That(bodies[0].Displacement.magnitude, Is.LessThan(2f));
        }

        [Test]
        public void SimultaneousNeighbors_ReverseOrderGivesSameMovementById()
        {
            var bodies = new[] { Body(10, Vector2.left * 0.45f, Vector2.right * 0.16f),
                Body(20, Vector2.zero, Vector2.right * 0.06f), Body(30, Vector2.right * 0.45f, Vector2.left * 0.16f) };
            Solve(bodies); AssertSafe(bodies);
            var expected = (EnemySeparationBody[])bodies.Clone();
            System.Array.Reverse(bodies); Solve(bodies); AssertSafe(bodies);
            for (int i = 0; i < bodies.Length; i++) Assert.That(bodies[i].Displacement, Is.EqualTo(expected[bodies.Length - 1 - i].Displacement));
        }

        [Test]
        public void GuardCancelsLeader_FollowerCannotRelyOnItsMovement()
        {
            var bodies = new[] { Body(1, Vector2.zero, Vector2.right * 0.1f), Body(2, Vector2.right * 0.45f, Vector2.right * 0.1f) };
            Solve(bodies, (i, d) => i == 1 ? Vector2.zero : d);
            AssertSafe(bodies);
            Assert.That(bodies[1].Displacement, Is.EqualTo(Vector2.zero));
            Assert.That(bodies[0].Displacement.x, Is.LessThanOrEqualTo(0.05f));
        }

        [Test]
        public void SharedTranslationAtContact_IsPreserved()
        {
            var bodies = new[] { Body(1, Vector2.zero, Vector2.right * 0.06f), Body(2, Vector2.right * 0.4f, Vector2.right * 0.06f) };
            Solve(bodies);
            Assert.That(bodies[0].Displacement, Is.EqualTo(bodies[0].Desired));
            Assert.That(bodies[1].Displacement, Is.EqualTo(bodies[1].Desired));
        }

        [Test]
        public void ConvergingPairAtContact_PreservesSharedTangentialMovement()
        {
            var bodies = new[] { Body(1, Vector2.zero, new Vector2(0.04f, 0.01f)),
                Body(2, Vector2.right * 0.4f, new Vector2(-0.04f, 0.01f)) };
            Solve(bodies); AssertSafe(bodies);
            Assert.That(bodies[0].Displacement.y, Is.EqualTo(0.01f).Within(0.00001f));
            Assert.That(bodies[1].Displacement.y, Is.EqualTo(0.01f).Within(0.00001f));
        }

        [TestCase(false)] [TestCase(true)]
        public void PreExistingOverlap_RecoversAcrossBoundedTicks(bool coincident)
        {
            var bodies = new[] { Body(1, Vector2.zero, Vector2.zero, 0.02f), Body(2, Vector2.right * (coincident ? 0f : 0.1f), Vector2.zero, 0.02f) };
            float previous = Vector2.Distance(bodies[0].Position, bodies[1].Position);
            for (int tick = 0; tick < 16; tick++)
            {
                Solve(bodies);
                foreach (var body in bodies) Assert.That(body.Displacement.magnitude, Is.LessThanOrEqualTo(body.Budget + 0.00001f));
                for (int i = 0; i < bodies.Length; i++) bodies[i].Position += bodies[i].Displacement;
                float distance = Vector2.Distance(bodies[0].Position, bodies[1].Position);
                Assert.That(distance, Is.GreaterThanOrEqualTo(previous - 0.00001f)); previous = distance;
            }
            Assert.That(previous, Is.GreaterThanOrEqualTo(0.3999f));
        }

        [Test]
        public void Recovery_WallOrLockWins_UsesAvailableSide()
        {
            var bodies = new[] { Body(1, Vector2.zero, Vector2.zero, 0.06f), Body(2, Vector2.right * 0.35f, Vector2.zero, 0.06f) };
            Solve(bodies, (i, d) => i == 0 ? Vector2.zero : d);
            Assert.That(bodies[0].Displacement, Is.EqualTo(Vector2.zero));
            Assert.That(bodies[1].Displacement.x, Is.GreaterThan(0.049f));
            bodies[0].Locked = bodies[1].Locked = true;
            Solve(bodies);
            Assert.That(bodies[0].Displacement, Is.EqualTo(Vector2.zero));
            Assert.That(bodies[1].Displacement, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void SaturationStopsLocomotion_WithoutClampingExternalMovement()
        {
            var bodies = new[] { Body(1, Vector2.zero, Vector2.right), Body(2, Vector2.zero, Vector2.left), Body(3, Vector2.zero, Vector2.up) };
            bodies[0].External = Vector2.right * 4f;
            var discovery = new EnemySeparationDiscovery(1);
            new EnemySeparationSolver().Solve(bodies, 3, discovery, null);
            Assert.IsTrue(discovery.Saturated);
            foreach (var body in bodies) Assert.That(body.Displacement, Is.EqualTo(Vector2.zero));
            Assert.That(bodies[0].External.x, Is.EqualTo(4f));
        }

        private static EnemySeparationBody Body(int id, Vector2 position, Vector2 desired, float budget = -1f)
            => new EnemySeparationBody { Id = id, Position = position, Desired = desired, Radius = 0.2f, Budget = budget < 0f ? desired.magnitude : budget };
        private static void Solve(EnemySeparationBody[] bodies, System.Func<int, Vector2, Vector2> guard = null)
            => new EnemySeparationSolver().Solve(bodies, bodies.Length, new EnemySeparationDiscovery(), guard);
        private static void AssertSafe(EnemySeparationBody[] bodies)
        {
            foreach (var body in bodies) Assert.That(body.Displacement.magnitude, Is.LessThanOrEqualTo(body.Budget + 0.00001f));
            for (int a = 0; a < bodies.Length; a++)
            for (int b = a + 1; b < bodies.Length; b++)
            for (int step = 0; step <= 100; step++)
            {
                float t = step / 100f;
                Assert.That(Vector2.Distance(bodies[a].Position + bodies[a].Displacement * t,
                    bodies[b].Position + bodies[b].Displacement * t), Is.GreaterThanOrEqualTo(0.3999f));
            }
        }
    }
}

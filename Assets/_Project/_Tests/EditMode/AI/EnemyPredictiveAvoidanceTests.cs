using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyPredictiveAvoidanceTests
    {
        [Test]
        public void HeadOnPrediction_ChangesVelocityBeforePhysicalContact()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.right * 1.5f, Vector2.left * 3f, 0.2f) };
            Vector2 result = new EnemyPredictiveAvoidance().SelectVelocity(
                Vector2.zero, Vector2.right * 3f, 0.2f, neighbors);
            Assert.That(result, Is.Not.EqualTo(Vector2.right * 3f));
            Assert.That(result.magnitude, Is.GreaterThan(0f));
            Assert.IsTrue(EnemyPredictiveAvoidance.IsSafe(
                Vector2.zero, result, 0.2f, neighbors, 1f));
        }

        [Test]
        public void OffPathNeighbor_PreservesPreferredVelocity()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.up * 3f, Vector2.zero, 0.2f) };
            Assert.That(new EnemyPredictiveAvoidance().SelectVelocity(
                Vector2.zero, Vector2.right * 3f, 0.2f, neighbors), Is.EqualTo(Vector2.right * 3f));
        }

        [Test]
        public void SymmetricOptions_KeepDeterministicSideAcrossRepeatedRequests()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.right * 1.5f, Vector2.zero, 0.2f) };
            var solver = new EnemyPredictiveAvoidance();
            Vector2 first = solver.SelectVelocity(Vector2.zero, Vector2.right * 3f, 0.2f, neighbors);
            Assert.That(first.y, Is.GreaterThan(0f));
            for (int i = 0; i < 20; i++)
                Assert.That(solver.SelectVelocity(Vector2.zero, Vector2.right * 3f, 0.2f, neighbors),
                    Is.EqualTo(first));
        }

        [Test]
        public void EqualVelocityNeighbors_DoNotRequireArtificialLaneFormation()
        {
            var neighbors = new[]
            {
                new EnemyAvoidanceNeighbor(Vector2.left * 0.6f, Vector2.left * 3f, 0.2f),
                new EnemyAvoidanceNeighbor(Vector2.left * 1.5f, Vector2.left * 3f, 0.2f)
            };
            Assert.That(new EnemyPredictiveAvoidance().SelectVelocity(
                Vector2.zero, Vector2.left * 3f, 0.2f, neighbors), Is.EqualTo(Vector2.left * 3f));
        }

        [TestCase(1, 0.5f)]
        [TestCase(5, 0.5f)]
        [TestCase(7, 0.5f)]
        [TestCase(8, 0.2f)]
        [TestCase(12, 0.2f)]
        public void PreferredRadius_CompressesAtEight(int count, float expected)
        {
            Assert.That(EnemyPredictiveAvoidance.GetPreferredRadius(0.2f, count),
                Is.EqualTo(expected).Within(0.000001f));
        }

        [Test]
        public void InsidePreferredBubble_RelaxesRadiusAndStillApproaches()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.right * 0.7f, Vector2.zero, 0.2f) };
            Vector2 result = new EnemyPredictiveAvoidance().SelectVelocity(
                Vector2.zero, Vector2.right * 3f, 0.2f, neighbors);
            Assert.That(result.x, Is.GreaterThan(0f));
            Assert.IsTrue(EnemyPredictiveAvoidance.IsSafe(Vector2.zero, result, 0.2f, neighbors, 1f));
        }

        [Test]
        public void EightNearbyEnemies_RetainSafeForwardProgress()
        {
            var neighbors = new List<EnemyAvoidanceNeighbor>
            {
                new EnemyAvoidanceNeighbor(Vector2.right * 0.8f, Vector2.zero, 0.2f)
            };
            for (int i = 0; i < 6; i++)
                neighbors.Add(new EnemyAvoidanceNeighbor(new Vector2(-2f, -2f + i * 0.8f), Vector2.zero, 0.2f));
            Vector2 result = new EnemyPredictiveAvoidance().SelectVelocity(
                Vector2.zero, Vector2.right * 3f, 0.2f, neighbors);
            Assert.That(result.x, Is.GreaterThan(0f));
            Assert.IsTrue(EnemyPredictiveAvoidance.IsSafe(Vector2.zero, result, 0.2f, neighbors, 1f));
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(3.5f)]
        [TestCase(8f)]
        public void Sampling_NeverExceedsRequestedSpeedOrTurnsOutward(float speed)
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.right, Vector2.zero, 0.2f) };
            Vector2 result = new EnemyPredictiveAvoidance().SelectVelocity(
                Vector2.zero, Vector2.right * speed, 0.2f, neighbors);
            Assert.That(result.magnitude, Is.LessThanOrEqualTo(speed + 0.000001f));
            Assert.That(result.x, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void PredictedCrossing_IsDetectedBetweenEndpoints()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(new Vector2(1f, 1f), Vector2.down * 4f, 0.2f) };
            Assert.IsFalse(EnemyPredictiveAvoidance.IsSafe(
                Vector2.zero, Vector2.right * 4f, 0.2f, neighbors, 1f));
        }

        [Test]
        public void OverlappedPreference_AllowsSeparatingMotionWithoutGeneratingMotion()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.right * 0.7f, Vector2.zero, 0.2f) };
            var solver = new EnemyPredictiveAvoidance();
            Assert.That(solver.SelectVelocity(Vector2.zero, Vector2.left * 3f, 0.2f, neighbors),
                Is.EqualTo(Vector2.left * 3f));
            Assert.That(solver.SelectVelocity(Vector2.zero, Vector2.zero, 0.2f, neighbors), Is.EqualTo(Vector2.zero));
        }
    }
}

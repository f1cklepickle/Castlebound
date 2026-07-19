using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyApproachSpreadTests
    {
        [Test]
        public void CrowdedApproach_BlendsLocalSeparationAndPreservesForwardFloor()
        {
            EnemyApproachSpread.ComputeApproach(
                pursuit: Vector2.right * 8f,
                directionToTarget: Vector2.right,
                localSeparation: Vector2.up,
                hasNeighbors: true,
                stableBias: Vector2.zero,
                speed: 8f,
                separationStrength: 0.8f,
                maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f,
                out Vector2 radial,
                out Vector2 tangent);

            Assert.That(tangent.y, Is.GreaterThan(0f));
            Assert.That(radial.x, Is.GreaterThanOrEqualTo(6.4f));
            Assert.That((radial + tangent).magnitude, Is.LessThanOrEqualTo(8.001f));
        }

        [Test]
        public void CoincidentNeighbors_UseStableBiasToBreakSymmetry()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f, Vector2.right, Vector2.zero, true,
                stableBias: new Vector2(0.2f, -0.8f), speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f,
                out _, out Vector2 tangent);

            Assert.That(tangent.y, Is.LessThan(0f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UncrowdedApproach_RemainsDirect(bool hasNeighbors)
        {
            var pursuit = Vector2.right * 8f;

            EnemyApproachSpread.ComputeApproach(
                pursuit, Vector2.right, Vector2.zero, hasNeighbors,
                stableBias: Vector2.zero, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f,
                out Vector2 radial, out Vector2 tangent);

            Assert.That(radial, Is.EqualTo(pursuit));
            Assert.That(tangent, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void StrongSeparation_NeverReversesPursuit()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f, Vector2.right, new Vector2(-10f, 10f), true,
                stableBias: Vector2.zero, speed: 8f,
                separationStrength: 1f, maxLateralRatio: 0.5f,
                minimumForwardRatio: 0.75f,
                out Vector2 radial, out _);

            Assert.That(radial.x, Is.GreaterThanOrEqualTo(6f));
        }

        [Test]
        public void GroupInsideArrivalDistance_SteersTowardLargerAngularGap()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f, Vector2.right, Vector2.zero, false, Vector2.zero,
                distance: 8f, holdRadius: 2.6f,
                gapCW: 0.1f, gapCCW: 0.5f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out Vector2 radial, out Vector2 tangent);

            Assert.That(tangent.y, Is.GreaterThan(0f));
            Assert.That(radial.x, Is.GreaterThanOrEqualTo(6.4f));
        }

        [Test]
        public void GroupOutsideArrivalDistance_DoesNotShapeDistantRing()
        {
            var pursuit = Vector2.right * 8f;
            EnemyApproachSpread.ComputeApproach(
                pursuit, Vector2.right, Vector2.zero, false, Vector2.up,
                distance: 13f, holdRadius: 2.6f,
                gapCW: 0f, gapCCW: 0.5f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out Vector2 radial, out Vector2 tangent);

            Assert.That(radial, Is.EqualTo(pursuit));
            Assert.That(tangent, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void SeparatedGroupWithClearedGaps_DoesNotUseFallbackBias()
        {
            var pursuit = Vector2.right * 8f;
            EnemyApproachSpread.ComputeApproach(
                pursuit, Vector2.right, Vector2.zero, hasNeighbors: false, stableBias: Vector2.up,
                distance: 8f, holdRadius: 2.6f,
                gapCW: 0f, gapCCW: 0f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out Vector2 radial, out Vector2 tangent);

            Assert.That(radial, Is.EqualTo(pursuit));
            Assert.That(tangent, Is.EqualTo(Vector2.zero));
        }
    }
}

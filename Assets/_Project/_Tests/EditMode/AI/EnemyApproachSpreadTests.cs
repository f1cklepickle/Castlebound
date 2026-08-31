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
        public void NearCancelledNeighborSums_DoNotAmplifyDirectionChanges()
        {
            EnemyApproachSpread.ComputeApproach(
                pursuit: Vector2.right * 8f,
                directionToTarget: Vector2.right,
                localSeparation: new Vector2(0.03f, 0.04f),
                hasNeighbors: true,
                stableBias: Vector2.down,
                speed: 8f,
                separationStrength: 0.8f,
                maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f,
                out Vector2 firstRadial,
                out Vector2 firstTangent);

            EnemyApproachSpread.ComputeApproach(
                pursuit: Vector2.right * 8f,
                directionToTarget: Vector2.right,
                localSeparation: new Vector2(0.03f, -0.04f),
                hasNeighbors: true,
                stableBias: Vector2.down,
                speed: 8f,
                separationStrength: 0.8f,
                maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f,
                out Vector2 secondRadial,
                out Vector2 secondTangent);

            Vector2 firstMovement = firstRadial + firstTangent;
            Vector2 secondMovement = secondRadial + secondTangent;
            Assert.That(Vector2.Angle(firstMovement, secondMovement), Is.LessThan(2f),
                "Near-cancelled neighbor sums must not become full-strength direction changes.");
            Assert.That(firstTangent.y, Is.GreaterThan(0f));
            Assert.That(secondTangent.y, Is.LessThan(0f));
            Assert.That(firstMovement.magnitude, Is.LessThanOrEqualTo(8.001f));
            Assert.That(secondMovement.magnitude, Is.LessThanOrEqualTo(8.001f));
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

        [Test]
        public void LongitudinalCompression_UsesStableBiasToCreateApproachLane()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f,
                Vector2.right,
                Vector2.left * 0.75f,
                hasNeighbors: true,
                stableBias: Vector2.up,
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
        public void LongitudinalLaneAssist_FadesAsSurroundArrivalTakesOver()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f, Vector2.right, Vector2.left * 0.75f,
                hasNeighbors: true, stableBias: Vector2.up,
                distance: 13f, engagementDistance: 2.6f,
                gapCW: 0.2f, gapCCW: 0.2f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out _, out Vector2 distantTangent);

            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f, Vector2.right, Vector2.left * 0.75f,
                hasNeighbors: true, stableBias: Vector2.up,
                distance: 3.6f, engagementDistance: 2.6f,
                gapCW: 0.2f, gapCCW: 0.2f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out _, out Vector2 arrivingTangent);

            Assert.That(distantTangent.y, Is.GreaterThan(0f));
            Assert.That(arrivingTangent.y, Is.GreaterThan(0f));
            Assert.That(arrivingTangent.magnitude,
                Is.LessThan(distantTangent.magnitude * 0.2f));
        }

        [Test]
        public void LateralSeparation_RemainsAuthoritativeOverStableLaneBias()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.right * 8f,
                Vector2.right,
                Vector2.up,
                hasNeighbors: true,
                stableBias: Vector2.down,
                speed: 8f,
                separationStrength: 0.8f,
                maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f,
                out _,
                out Vector2 tangent);

            Assert.That(tangent.y, Is.GreaterThan(0f));
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
        public void EnemyEastOfPlayer_SteersNorthTowardLargerCounterClockwiseGap()
        {
            EnemyApproachSpread.ComputeApproach(
                Vector2.left * 8f, Vector2.left, Vector2.zero, false, Vector2.zero,
                distance: 8f, engagementDistance: 2.6f,
                gapCW: 0.1f, gapCCW: 0.5f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out Vector2 radial, out Vector2 tangent);

            Assert.That(tangent.y, Is.GreaterThan(0f));
            Assert.That(radial.x, Is.LessThanOrEqualTo(-6.4f));
        }

        [Test]
        public void GroupOutsideArrivalDistance_DoesNotShapeDistantRing()
        {
            var pursuit = Vector2.right * 8f;
            EnemyApproachSpread.ComputeApproach(
                pursuit, Vector2.right, Vector2.zero, false, Vector2.up,
                distance: 13f, engagementDistance: 2.6f,
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
                distance: 8f, engagementDistance: 2.6f,
                gapCW: 0f, gapCCW: 0f, hasGroup: true, speed: 8f,
                separationStrength: 0.8f, maxLateralRatio: 0.35f,
                minimumForwardRatio: 0.8f, surroundArrivalDistance: 10f,
                maxAngularArrivalRatio: 0.18f, gapDeadbandRadians: 8f * Mathf.Deg2Rad,
                out Vector2 radial, out Vector2 tangent);

            Assert.That(radial, Is.EqualTo(pursuit));
            Assert.That(tangent, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void CrowdedHold_UsesOnlyTangentialSeparation()
        {
            Vector2 tangent = EnemyApproachSpread.ComputeHoldSeparation(
                directionToTarget: Vector2.right,
                localSeparation: new Vector2(-1f, 1f),
                hasNeighbors: true,
                stableBias: Vector2.zero,
                speed: 8f,
                separationStrength: 0.8f,
                maxLateralRatio: 0.35f);

            Assert.That(tangent.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(tangent.y, Is.GreaterThan(0f));
            Assert.That(tangent.magnitude, Is.LessThanOrEqualTo(2.801f));
        }

        [Test]
        public void UncrowdedHold_RemainsStationary()
        {
            Vector2 tangent = EnemyApproachSpread.ComputeHoldSeparation(
                Vector2.right, Vector2.up, hasNeighbors: false, stableBias: Vector2.down,
                speed: 8f, separationStrength: 0.8f, maxLateralRatio: 0.35f);

            Assert.That(tangent, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void CoincidentHoldNeighbors_UseStableTangentialBias()
        {
            Vector2 tangent = EnemyApproachSpread.ComputeHoldSeparation(
                Vector2.right, Vector2.zero, hasNeighbors: true, stableBias: Vector2.down,
                speed: 8f, separationStrength: 0.8f, maxLateralRatio: 0.35f);

            Assert.That(tangent.y, Is.LessThan(0f));
        }
    }
}

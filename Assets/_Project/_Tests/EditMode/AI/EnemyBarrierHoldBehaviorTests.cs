using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class EnemyBarrierHoldBehaviorTests
    {
        [Test]
        public void DoesNotHold_WhenBarrierIsBroken()
        {
            // Arrange
            float holdRadius = 2.0f;
            float releaseMargin = 0.5f;
            int outrunFrames = 8;
            int distTrend = 0;

            float distanceToBarrier = 1.0f; // inside hold radius
            bool barrierBroken = true;

            // Act
            bool shouldHold = EnemyController2D.ShouldHoldForBarrierTarget(
                distanceToBarrier,
                barrierBroken,
                holdRadius,
                releaseMargin,
                distTrend,
                outrunFrames);

            // Assert
            Assert.IsFalse(shouldHold,
                "Enemy should not enter or remain in HOLD state when the barrier is broken.");
        }

        [Test]
        public void CanHold_WhenBarrierIntact_AndWithinHoldRadius()
        {
            // Arrange
            float holdRadius = 2.0f;
            float releaseMargin = 0.5f;
            int outrunFrames = 8;
            int distTrend = 0;

            float distanceToBarrier = 1.0f; // inside hold radius
            bool barrierBroken = false;

            // Act
            bool shouldHold = EnemyController2D.ShouldHoldForBarrierTarget(
                distanceToBarrier,
                barrierBroken,
                holdRadius,
                releaseMargin,
                distTrend,
                outrunFrames);

            // Assert
            Assert.IsTrue(shouldHold,
                "Enemy may enter HOLD at an intact barrier when within hold radius.");
        }
    }
}

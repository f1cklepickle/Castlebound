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
            float distanceToBarrier = 1.0f; // inside hold radius
            bool barrierBroken = true;

            // Act
            bool shouldHold = EnemyController2D.ShouldHoldForBarrierTarget(
                distanceToBarrier,
                barrierBroken,
                holdRadius);

            // Assert
            Assert.IsFalse(shouldHold,
                "Enemy should not enter or remain in HOLD state when the barrier is broken.");
        }

        [Test]
        public void CanHold_WhenBarrierIntact_AndWithinHoldRadius()
        {
            // Arrange
            float holdRadius = 2.0f;
            float distanceToBarrier = 1.0f; // inside hold radius
            bool barrierBroken = false;

            // Act
            bool shouldHold = EnemyController2D.ShouldHoldForBarrierTarget(
                distanceToBarrier,
                barrierBroken,
                holdRadius);

            // Assert
            Assert.IsTrue(shouldHold,
                "Enemy may enter HOLD at an intact barrier when within hold radius.");
        }

        [Test]
        public void DoesNotHold_WhenBarrierIntact_AndOutsideHoldRadius()
        {
            // Arrange
            float holdRadius = 2.0f;
            float distanceToBarrier = 2.1f;
            bool barrierBroken = false;

            // Act
            bool shouldHold = EnemyController2D.ShouldHoldForBarrierTarget(
                distanceToBarrier,
                barrierBroken,
                holdRadius);

            // Assert
            Assert.IsFalse(shouldHold,
                "Enemy should not hold at an intact barrier when outside the hold radius.");
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    /// <summary>
    /// Enemy chooses barrier if it blocks the direct path, otherwise chooses player.
    /// </summary>
    public class EnemyBarrierTargetingTests
    {
        [Test]
        public void Enemy_targets_barrier_when_barrier_blocks_direct_path_to_player()
        {
            // Arrange
            var enemyPos = Vector2.zero;
            var playerPos = new Vector2(10f, 0f);
            var barrierPos = new Vector2(5f, 0f);

            // Act
            bool shouldTargetBarrier = EnemyBarrierTargeting.ShouldTargetBarrier(
                enemyPos, playerPos, barrierPos);

            // Assert
            Assert.IsTrue(shouldTargetBarrier, "Barrier should block path but was not detected.");
        }

        [Test]
        public void Enemy_targets_player_when_no_barrier_blocks_direct_path()
        {
            // Arrange
            var enemyPos = Vector2.zero;
            var playerPos = new Vector2(10f, 0f);
            var barrierPos = new Vector2(0f, 5f);

            // Act
            bool shouldTargetBarrier = EnemyBarrierTargeting.ShouldTargetBarrier(
                enemyPos, playerPos, barrierPos);

            // Assert
            Assert.IsFalse(shouldTargetBarrier, "Barrier should not block path but was detected as blocking.");
        }
    }
}

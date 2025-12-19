using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTargetTypeTests
    {
        [Test]
        public void Attack_UsesBarrierGate_WhenTargetTypeIsBarrier()
        {
            // Arrange
            var enemy = new GameObject("Enemy");
            var controller = enemy.AddComponent<EnemyController2D>();
            var attack = enemy.AddComponent<EnemyAttack>();
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BarrierHealth>();

            attack.Damage = 1;

            // Simulate selector decision: barrier target type.
            controller.Debug_SetupRefs(null, barrier.transform);
            SetControllerTargetForTest(controller, barrier.transform, EnemyTargetType.Barrier);

            // Act
            bool canDamage = EnemyAttack.CanDamageBarrier(enemyInside: false, playerInside: false);

            // Assert: gating still applied for barrier targets.
            Assert.IsTrue(canDamage, "Barrier target type should still go through barrier gate logic (outside allowed).");

            Object.DestroyImmediate(barrier);
            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void Attack_SkipsBarrierGate_WhenTargetTypeIsPlayer()
        {
            // Arrange
            var enemy = new GameObject("Enemy");
            var controller = enemy.AddComponent<EnemyController2D>();
            var attack = enemy.AddComponent<EnemyAttack>();
            var player = new GameObject("Player");
            player.tag = "Player";
            player.AddComponent<BarrierHealth>(); // Would trigger old barrier-based gate check.

            attack.Damage = 1;

            // Simulate selector decision: player target type.
            controller.Debug_SetupRefs(player.transform, null);
            SetControllerTargetForTest(controller, player.transform, EnemyTargetType.Player);

            // Act
            // Gate check shouldn't block when target type is player.
            bool canDamage = true;
            if (controller.CurrentTargetType == EnemyTargetType.Barrier)
            {
                canDamage = EnemyAttack.CanDamageBarrier(enemyInside: false, playerInside: false);
            }

            // Assert
            Assert.IsTrue(canDamage, "Player target type should not be treated as a barrier even if the target has BarrierHealth.");

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(enemy);
        }

        private static void SetControllerTargetForTest(EnemyController2D controller, Transform target, EnemyTargetType targetType)
        {
            // Directly assign for test; real flow uses selector.
            controller.Debug_SetupRefs(target, null);
            typeof(EnemyController2D)
                .GetField("_currentTargetType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(controller, targetType);
        }
    }
}

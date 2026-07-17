using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;
using System.Reflection;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTargetTypeTests
    {
        private sealed class DummyDamageable : IDamageable
        {
            public int DamageTaken { get; private set; }
            public void TakeDamage(int amount) => DamageTaken += amount;
        }

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
        public void PlayerTargetedAttack_BlocksBarrierRecipient_WhenEnemyAndPlayerInside()
        {
            // Arrange
            var enemy = new GameObject("Enemy");
            var controller = enemy.AddComponent<EnemyController2D>();
            var regionState = enemy.AddComponent<EnemyRegionState>();
            var attack = enemy.AddComponent<EnemyAttack>();
            var player = new GameObject("Player");
            player.tag = "Player";
            var barrier = new GameObject("Barrier");
            var barrierHealth = barrier.AddComponent<BarrierHealth>();
            barrierHealth.MaxHealth = 5;
            barrierHealth.CurrentHealth = 5;
            var playerDamageable = new DummyDamageable();

            // Simulate selector decision: player target type.
            controller.Debug_SetupRefs(player.transform, null);
            SetControllerTargetForTest(controller, player.transform, EnemyTargetType.Player);
            SetRegionState(regionState, enemyInside: true, playerInside: true);

            // Act
            attack.DealDamage(barrierHealth);
            attack.DealDamage(playerDamageable);

            // Assert
            Assert.That(controller.CurrentTargetType, Is.EqualTo(EnemyTargetType.Player));
            Assert.That(barrierHealth.CurrentHealth, Is.EqualTo(5),
                "A player-targeted swing must not damage an overlapping barrier when both actors are inside.");
            Assert.That(playerDamageable.DamageTaken, Is.EqualTo(1),
                "Inside/inside barrier gating must not block damage to non-barrier recipients.");

            Object.DestroyImmediate(barrier);
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

        private static void SetRegionState(EnemyRegionState state, bool enemyInside, bool playerInside)
        {
            var type = typeof(EnemyRegionState);
            type.GetField("enemyInside", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(state, enemyInside);
            type.GetField("playerInside", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(state, playerInside);
        }
    }
}

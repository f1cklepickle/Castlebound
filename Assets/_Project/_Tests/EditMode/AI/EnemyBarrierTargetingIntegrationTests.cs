// NOTE: Multi-barrier targeting is not part of the barrier destroy/repair feature.
// This test is intentionally ignored until multi-barrier targeting is implemented
// in a dedicated feature branch.

using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class EnemyBarrierTargetingIntegrationTests
    {
        [Test, Ignore("Deferred until multi-barrier targeting feature is implemented")]
        public void Enemy_UsesNearestBarrier_FromRegisteredBarriers()
        {
            // Arrange: player and enemy setup.
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;

            var enemy = new GameObject("Enemy");
            enemy.transform.position = new Vector3(2f, 0f, 0f);
            var enemyController = enemy.AddComponent<EnemyController2D>();

            // Provide a player transform so selector can operate.
            var playerField = typeof(EnemyController2D).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(playerField, "Expected EnemyController2D to have a private 'player' field.");
            playerField.SetValue(enemyController, player.transform);

            // Create barriers; they auto-register via OnEnable.
            var nearBarrierGO = new GameObject("NearBarrier");
            nearBarrierGO.transform.position = new Vector3(0f, 0f, 0f);
            var nearBarrier = nearBarrierGO.AddComponent<BarrierHealth>();

            var farBarrierGO = new GameObject("FarBarrier");
            farBarrierGO.transform.position = new Vector3(10f, 0f, 0f);
            var farBarrier = farBarrierGO.AddComponent<BarrierHealth>();

            // Act: both player and enemy considered outside castle; gating should pick nearest intact gate.
            var target = enemyController.Debug_SelectTarget(playerInside: true, enemyInside: false);

            // Assert: enemy should choose the nearer barrier.
            Assert.AreSame(nearBarrier.transform, target, "Enemy should target the nearest active barrier from the registry.");
            Assert.AreNotSame(farBarrier.transform, target, "Enemy should not pick the farther barrier when a nearer one exists.");

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(nearBarrierGO);
            Object.DestroyImmediate(farBarrierGO);
        }
    }
}

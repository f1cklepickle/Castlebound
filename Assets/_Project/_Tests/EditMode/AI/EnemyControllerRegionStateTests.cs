using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class EnemyControllerRegionStateTests
    {
        [Test]
        public void UsesRegionStateCache_ForTargetSelection()
        {
            var player = new GameObject("Player");
            player.tag = "Player";

            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BarrierHealth>();

            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            var controller = enemy.AddComponent<EnemyController2D>();
            var state = enemy.AddComponent<EnemyRegionState>();

            controller.Debug_SetupRefs(player.transform, barrier.transform);

            SetState(state, enemyInside: false, playerInside: true);

            var fixedUpdate = typeof(EnemyController2D).GetMethod(
                "FixedUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            fixedUpdate.Invoke(controller, null);

            Assert.That(controller.CurrentTargetType, Is.EqualTo(EnemyTargetType.Barrier),
                "Enemy outside with player inside should target barrier when using cached state.");

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(barrier);
        }

        private static void SetState(EnemyRegionState state, bool enemyInside, bool playerInside)
        {
            var type = typeof(EnemyRegionState);
            type.GetField("enemyInside", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(state, enemyInside);
            type.GetField("playerInside", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(state, playerInside);
        }
    }
}

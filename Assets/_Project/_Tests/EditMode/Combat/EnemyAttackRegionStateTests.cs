using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;
using System.Reflection;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackRegionStateTests
    {
        [Test]
        public void ReadsCachedState_FromEnemyRegionState()
        {
            var enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            enemy.AddComponent<EnemyController2D>();
            var state = enemy.AddComponent<EnemyRegionState>();
            var attack = enemy.AddComponent<EnemyAttack>();

            SetState(state, enemyInside: true, playerInside: true);

            attack.Debug_GetRegionState(out bool enemyInside, out bool playerInside);

            Assert.IsTrue(enemyInside, "Expected EnemyAttack to read cached enemyInside.");
            Assert.IsTrue(playerInside, "Expected EnemyAttack to read cached playerInside.");

            Object.DestroyImmediate(enemy);
        }

        private static void SetState(EnemyRegionState state, bool enemyInside, bool playerInside)
        {
            var type = typeof(EnemyRegionState);
            type.GetField("enemyInside", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(state, enemyInside);
            type.GetField("playerInside", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(state, playerInside);
        }
    }
}

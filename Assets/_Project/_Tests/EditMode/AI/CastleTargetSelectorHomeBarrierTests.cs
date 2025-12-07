using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class CastleTargetSelectorHomeBarrierTests
    {
        [Test]
        public void AssignsHomeBarrier_BasedOnSpawnPosition()
        {
            var barrierNear = new GameObject("BarrierNear").transform;
            barrierNear.position = new Vector2(-2f, 0f);

            var barrierFar = new GameObject("BarrierFar").transform;
            barrierFar.position = new Vector2(10f, 0f);

            var spawnPosition = new Vector2(-3f, 0f);
            var barriers = new List<Transform> { barrierFar, barrierNear };

            var homeBarrier = CastleTargetSelector.AssignHomeBarrier(spawnPosition, barriers);

            Assert.AreSame(barrierNear, homeBarrier, "Enemy should assign the nearest barrier to its spawn as the home barrier.");

            Object.DestroyImmediate(barrierNear.gameObject);
            Object.DestroyImmediate(barrierFar.gameObject);
        }

        [Test]
        public void KeepsHomeBarrier_EvenIfAnotherBecomesCloser()
        {
            var barrierHome = new GameObject("BarrierHome").transform;
            barrierHome.position = new Vector2(-2f, 0f);

            var barrierOther = new GameObject("BarrierOther").transform;
            barrierOther.position = new Vector2(8f, 0f);

            var spawnPosition = new Vector2(-3f, 0f);
            var barriers = new List<Transform> { barrierOther, barrierHome };

            var homeBarrier = CastleTargetSelector.AssignHomeBarrier(spawnPosition, barriers);

            var player = new GameObject("Player").transform;
            player.position = new Vector2(0f, 0f);

            // Enemy has moved closer to the other barrier, but target should remain the assigned home barrier.
            var enemyPositionNow = new Vector2(7.5f, 0f);
            bool enemyInside = false;
            bool playerInside = true;

            var result = CastleTargetSelector.ChooseTargetWithHome(
                enemyPositionNow,
                enemyInside,
                playerInside,
                player,
                homeBarrier,
                barriers);

            Assert.AreSame(homeBarrier, result, "Enemy should keep its assigned home barrier even if another barrier becomes closer.");

            Object.DestroyImmediate(barrierHome.gameObject);
            Object.DestroyImmediate(barrierOther.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void UsesAssignedBarrier_WhenPlayerInside_EnemyOutside_EvenIfBroken()
        {
            var barrierHome = new GameObject("BarrierHome").transform;
            barrierHome.position = new Vector2(-2f, 0f);

            // Mark barrier as broken.
            var health = barrierHome.gameObject.AddComponent<BarrierHealth>();
            health.TakeDamage(health.MaxHealth);

            var spawnPosition = new Vector2(-3f, 0f);
            var barriers = new List<Transform> { barrierHome };

            var homeBarrier = CastleTargetSelector.AssignHomeBarrier(spawnPosition, barriers);

            var player = new GameObject("Player").transform;
            player.position = new Vector2(0f, 0f);

            bool enemyInside = false;
            bool playerInside = true;

            var enemyPositionNow = new Vector2(-5f, 0f);

            var result = CastleTargetSelector.ChooseTargetWithHome(
                enemyPositionNow,
                enemyInside,
                playerInside,
                player,
                homeBarrier,
                barriers);

            Assert.AreSame(homeBarrier, result, "While outside, enemy should continue to target its assigned barrier, even if it is already broken.");

            Object.DestroyImmediate(barrierHome.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void TargetsPlayerAfterHomeBarrierBroken()
        {
            var barrierHome = new GameObject("BarrierHome").transform;
            barrierHome.position = new Vector2(-2f, 0f);

            var health = barrierHome.gameObject.AddComponent<BarrierHealth>();
            health.TakeDamage(health.MaxHealth);

            var spawnPosition = new Vector2(-3f, 0f);
            var barriers = new List<Transform> { barrierHome };

            var homeBarrier = CastleTargetSelector.AssignHomeBarrier(spawnPosition, barriers);

            var player = new GameObject("Player").transform;
            player.position = new Vector2(0f, 0f);

            bool enemyInside = true; // after passing through the broken barrier
            bool playerInside = true;

            var enemyPositionNow = new Vector2(-1f, 0f);

            var result = CastleTargetSelector.ChooseTargetWithHome(
                enemyPositionNow,
                enemyInside,
                playerInside,
                player,
                homeBarrier,
                barriers);

            Assert.AreSame(player, result, "Once the home barrier is broken and the enemy is inside, target should switch to the player.");

            Object.DestroyImmediate(barrierHome.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }
    }
}

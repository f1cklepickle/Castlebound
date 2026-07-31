using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class CastleTargetSelectorHomeBarrierTests
    {
        [Test]
        public void HomeBarrierAssignment_ConsumesHealthRegistryWithoutTransformCollectionBuilder()
        {
            var selectorMethod = typeof(CastleTargetSelector).GetMethod(
                "SelectNearestBarrier",
                new[] { typeof(Vector2), typeof(IReadOnlyList<BarrierHealth>) });
            var obsoleteBuilder = typeof(EnemyTargeting).GetMethod(
                "GetAllBarrierTransforms",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(selectorMethod, "Home-barrier selection must consume the lifecycle-aware BarrierHealth registry directly.");
            Assert.IsNull(obsoleteBuilder, "EnemyTargeting must not rebuild a Transform collection during home-barrier assignment.");
        }

        [Test]
        public void AssignsHomeBarrier_BasedOnSpawnPosition()
        {
            var barrierNear = new GameObject("BarrierNear").AddComponent<BarrierHealth>();
            barrierNear.transform.position = new Vector2(-2f, 0f);

            var barrierFar = new GameObject("BarrierFar").AddComponent<BarrierHealth>();
            barrierFar.transform.position = new Vector2(10f, 0f);

            var spawnPosition = new Vector2(-3f, 0f);
            var barriers = new List<BarrierHealth> { barrierFar, barrierNear };

            var homeBarrier = CastleTargetSelector.SelectNearestBarrier(spawnPosition, barriers);

            Assert.AreSame(barrierNear.transform, homeBarrier, "Enemy should assign the nearest barrier to its spawn as the home barrier.");

            Object.DestroyImmediate(barrierNear.gameObject);
            Object.DestroyImmediate(barrierFar.gameObject);
        }

        [Test]
        public void KeepsHomeBarrier_EvenIfAnotherBecomesCloser()
        {
            var barrierHome = new GameObject("BarrierHome").AddComponent<BarrierHealth>();
            barrierHome.transform.position = new Vector2(-2f, 0f);

            var barrierOther = new GameObject("BarrierOther").AddComponent<BarrierHealth>();
            barrierOther.transform.position = new Vector2(8f, 0f);

            var spawnPosition = new Vector2(-3f, 0f);
            var barriers = new List<BarrierHealth> { barrierOther, barrierHome };

            var homeBarrier = CastleTargetSelector.SelectNearestBarrier(spawnPosition, barriers);

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
                new List<Transform> { barrierOther.transform, barrierHome.transform });

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
            var barriers = new List<BarrierHealth> { health };

            var homeBarrier = CastleTargetSelector.SelectNearestBarrier(spawnPosition, barriers);

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
                new List<Transform> { barrierHome });

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
            var barriers = new List<BarrierHealth> { health };

            var homeBarrier = CastleTargetSelector.SelectNearestBarrier(spawnPosition, barriers);

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
                new List<Transform> { barrierHome });

            Assert.AreSame(player, result, "Once the home barrier is broken and the enemy is inside, target should switch to the player.");

            Object.DestroyImmediate(barrierHome.gameObject);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void AssignHomeBarrier_EqualDistanceUsesOrdinalNameTieBreak()
        {
            var barrierB = new GameObject("Barrier_B").AddComponent<BarrierHealth>();
            barrierB.transform.position = Vector2.left;
            var barrierA = new GameObject("Barrier_A").AddComponent<BarrierHealth>();
            barrierA.transform.position = Vector2.right;

            var result = CastleTargetSelector.SelectNearestBarrier(
                Vector2.zero,
                new List<BarrierHealth> { barrierB, barrierA });

            Assert.AreSame(barrierA.transform, result);

            Object.DestroyImmediate(barrierB.gameObject);
            Object.DestroyImmediate(barrierA.gameObject);
        }
    }
}

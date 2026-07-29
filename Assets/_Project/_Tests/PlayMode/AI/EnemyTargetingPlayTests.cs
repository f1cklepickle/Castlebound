using System.Collections;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyTargetingPlayTests
    {
        [UnityTest]
        public IEnumerator DestroyedHomeBarrier_ReassignsFromLiveRegistry()
        {
            var enemy = new GameObject("Enemy");
            var player = new GameObject("Player");
            var nearBarrier = new GameObject("NearBarrier");
            nearBarrier.transform.position = Vector2.left;
            nearBarrier.AddComponent<BarrierHealth>();
            var disabledBarrier = new GameObject("DisabledBarrier");
            disabledBarrier.transform.position = Vector2.zero;
            disabledBarrier.AddComponent<BarrierHealth>();
            disabledBarrier.SetActive(false);
            var farBarrier = new GameObject("FarBarrier");
            farBarrier.transform.position = Vector2.right * 10f;
            farBarrier.AddComponent<BarrierHealth>();
            var targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform);

            try
            {
                targeting.AssignHomeBarrierIfNeeded(Vector2.zero);
                Assert.AreSame(nearBarrier.transform, targeting.HomeBarrier,
                    "Disabled barriers must leave the live registry before home-barrier selection.");

                Object.Destroy(nearBarrier);
                yield return null;

                targeting.Refresh(Vector2.zero, playerInside: true, enemyInside: false);

                Assert.AreSame(farBarrier.transform, targeting.HomeBarrier,
                    "Destroyed barriers must leave the registry so targeting can safely reassign.");
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(player);
                Object.Destroy(disabledBarrier);
                Object.Destroy(farBarrier);
            }
        }

        [UnityTest]
        public IEnumerator RepairedBarrierPushback_RestoresBarrierTarget()
        {
            var enemy = new GameObject("Enemy");
            var player = new GameObject("Player");
            var barrier = new GameObject("Barrier");
            var barrierHealth = barrier.AddComponent<BarrierHealth>();
            barrierHealth.TakeDamage(barrierHealth.MaxHealth);
            var targeting = enemy.AddComponent<EnemyTargeting>();
            targeting.Debug_Setup(player.transform, barrier.transform);

            try
            {
                targeting.Refresh(new Vector2(0.5f, 0f), playerInside: true, enemyInside: false);
                Assert.AreEqual(EnemyTargetType.Player, targeting.CurrentTargetType);

                barrierHealth.Repair();
                enemy.transform.position = new Vector2(2f, 0f);
                yield return null;

                targeting.Refresh(enemy.transform.position, playerInside: true, enemyInside: false);

                Assert.AreSame(barrier.transform, targeting.SteerTarget);
                Assert.AreSame(barrier.transform, targeting.AttackTarget);
                Assert.AreEqual(EnemyTargetType.Barrier, targeting.CurrentTargetType);
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(player);
                Object.Destroy(barrier);
            }
        }
    }
}

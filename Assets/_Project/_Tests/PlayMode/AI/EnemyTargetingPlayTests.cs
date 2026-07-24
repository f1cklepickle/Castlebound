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

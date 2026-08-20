using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Gate
{
    public class BarrierRepairOverlapPushTests
    {
        [Test]
        public void ResolveOverlap_PushesEnemyOut_WhenMostlyOutsideBarrier()
        {
            GameObject barrierObject = null;
            GameObject enemyObject = null;

            try
            {
                barrierObject = new GameObject("Barrier");
                var barrier = barrierObject.AddComponent<BoxCollider2D>();
                barrier.size = new Vector2(2f, 2f);

                enemyObject = new GameObject("Enemy");
                var enemy = enemyObject.AddComponent<CircleCollider2D>();
                enemy.radius = 0.4f;
                enemyObject.transform.position = new Vector2(1.1f, 0f);
                Physics2D.SyncTransforms();

                Vector2 before = enemyObject.transform.position;
                bool pushedOutside = BarrierOverlapResolver.ResolveOverlap(
                    barrier,
                    enemy,
                    isPlayer: false);

                Assert.Greater(enemyObject.transform.position.x, before.x,
                    "Enemy should retain the historical minimum-separation behavior.");
                Assert.IsFalse(pushedOutside,
                    "Without an authored anchor, region ownership cannot be reconciled.");
            }
            finally
            {
                if (enemyObject != null) Object.DestroyImmediate(enemyObject);
                if (barrierObject != null) Object.DestroyImmediate(barrierObject);
            }
        }
    }
}

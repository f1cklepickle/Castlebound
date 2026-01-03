using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Gate
{
    public class BarrierOverlapResolverTests
    {
        [Test]
        public void IsMostlyInsideBarrier_ReturnsTrue_WhenThreeOfFivePointsInside()
        {
            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BoxCollider2D>();
            barrier.size = new Vector2(2f, 2f);
            barrier.isTrigger = false;

            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<CircleCollider2D>();
            enemy.radius = 0.4f;
            enemyGo.transform.position = Vector2.zero;

            Physics2D.SyncTransforms();

            bool result = BarrierOverlapResolver.IsMostlyInside(barrier, enemy);

            Assert.IsTrue(result, "Expected enemy to be mostly inside the barrier collider.");

            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(barrierGo);
        }

        [Test]
        public void IsMostlyInsideBarrier_ReturnsFalse_WhenTwoOrFewerPointsInside()
        {
            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BoxCollider2D>();
            barrier.size = new Vector2(2f, 2f);
            barrier.isTrigger = false;

            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<CircleCollider2D>();
            enemy.radius = 0.4f;
            enemyGo.transform.position = new Vector2(1.1f, 0f);

            Physics2D.SyncTransforms();

            bool result = BarrierOverlapResolver.IsMostlyInside(barrier, enemy);

            Assert.IsFalse(result, "Expected enemy to be mostly outside the barrier collider.");

            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(barrierGo);
        }
    }
}

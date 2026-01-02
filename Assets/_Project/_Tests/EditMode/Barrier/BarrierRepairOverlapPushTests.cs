using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Gate
{
    public class BarrierRepairOverlapPushTests
    {
        [Test]
        public void ResolveOverlap_PushesPlayerIntoBarrier_WhenOverlapping()
        {
            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BoxCollider2D>();
            barrier.size = new Vector2(2f, 2f);

            var hold = barrierGo.AddComponent<EnemyBarrierHoldBehavior>();
            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.position = new Vector2(2f, 0f);
            hold.Debug_SetAnchor(anchorGo.transform);

            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            var player = playerGo.AddComponent<CircleCollider2D>();
            player.radius = 0.4f;

            playerGo.transform.position = new Vector2(1.1f, 0f);

            Physics2D.SyncTransforms();

            Vector2 before = playerGo.transform.position;
            BarrierOverlapResolver.ResolveOverlap(barrier, player, isPlayer: true);

            Vector2 after = playerGo.transform.position;
            Assert.Less(after.x, before.x, "Player should be pushed into the barrier area.");

            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(anchorGo);
            Object.DestroyImmediate(barrierGo);
        }

        [Test]
        public void ResolveOverlap_PushesEnemyOut_WhenMostlyOutsideBarrier()
        {
            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BoxCollider2D>();
            barrier.size = new Vector2(2f, 2f);

            var enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<CircleCollider2D>();
            enemy.radius = 0.4f;

            enemyGo.transform.position = new Vector2(1.1f, 0f);

            Physics2D.SyncTransforms();

            Vector2 before = enemyGo.transform.position;
            BarrierOverlapResolver.ResolveOverlap(barrier, enemy, isPlayer: false);

            Vector2 after = enemyGo.transform.position;
            Assert.Greater(after.x, before.x, "Enemy should be pushed out of the barrier.");

            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(barrierGo);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Gate
{
    public class BarrierOverlapResolverPushInTests
    {
        [Test]
        public void ResolveOverlap_PushesPlayerInward_AndClearsOverlap()
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
            playerGo.transform.position = new Vector2(-1.1f, 0f);

            Physics2D.SyncTransforms();

            Vector2 before = playerGo.transform.position;
            BarrierOverlapResolver.ResolveOverlap(barrier, player, isPlayer: true);

            Physics2D.SyncTransforms();

            Vector2 after = playerGo.transform.position;
            var dist = Physics2D.Distance(barrier, player);

            Assert.Less(after.x, before.x, "Player should be pushed inward opposite the anchor direction.");
            Assert.IsFalse(dist.isOverlapped, "Player should no longer overlap the barrier after resolve.");

            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(anchorGo);
            Object.DestroyImmediate(barrierGo);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Gate
{
    public class BarrierRepairOverlapIntegrationPlayTests
    {
        [UnityTest]
        public IEnumerator Repair_ResolvesPlayerOverlap_AndPushesInward()
        {
            var barrierGo = new GameObject("Barrier");
            var barrierCollider = barrierGo.AddComponent<BoxCollider2D>();
            barrierCollider.size = new Vector2(2f, 2f);
            barrierGo.AddComponent<SpriteRenderer>();

            var hold = barrierGo.AddComponent<EnemyBarrierHoldBehavior>();
            var anchorGo = new GameObject("Anchor");
            anchorGo.transform.position = new Vector2(2f, 0f);
            hold.Debug_SetAnchor(anchorGo.transform);

            var barrierHealth = barrierGo.AddComponent<BarrierHealth>();

            var playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            playerGo.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            playerGo.AddComponent<PlayerController>();
            var playerCollider = playerGo.AddComponent<CircleCollider2D>();
            playerCollider.radius = 0.4f;

            playerGo.transform.position = new Vector2(1.1f, 0f);

            Physics2D.SyncTransforms();

            Vector2 before = playerGo.transform.position;
            barrierHealth.Repair();

            yield return new WaitForFixedUpdate();
            Physics2D.SyncTransforms();

            Vector2 after = playerGo.transform.position;
            var dist = Physics2D.Distance(barrierCollider, playerCollider);

            Assert.Less(after.x, before.x, "Player should be pushed inward opposite the anchor direction.");
            Assert.IsFalse(dist.isOverlapped, "Player should no longer overlap the barrier after Repair.");

            Object.Destroy(barrierGo);
            Object.Destroy(anchorGo);
            Object.Destroy(playerGo);
        }
    }
}

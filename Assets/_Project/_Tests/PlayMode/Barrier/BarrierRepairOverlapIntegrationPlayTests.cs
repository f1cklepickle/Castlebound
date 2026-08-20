using System.Collections;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.Gate
{
    public class BarrierRepairOverlapIntegrationPlayTests
    {
        [UnityTest]
        public IEnumerator Repair_ResolvesPlayerOverlap_AndPushesInward()
        {
            GameObject barrierObject = null;
            GameObject anchorObject = null;
            GameObject playerObject = null;

            try
            {
                barrierObject = new GameObject("Barrier");
                var barrier = barrierObject.AddComponent<BoxCollider2D>();
                barrier.size = new Vector2(2f, 2f);
                barrierObject.AddComponent<SpriteRenderer>();

                var hold = barrierObject.AddComponent<EnemyBarrierHoldBehavior>();
                anchorObject = new GameObject("Anchor");
                anchorObject.transform.position = new Vector2(2f, 0f);
                hold.Debug_SetAnchor(anchorObject.transform);
                var health = barrierObject.AddComponent<BarrierHealth>();

                playerObject = new GameObject("Player");
                playerObject.tag = "Player";
                playerObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                var player = playerObject.AddComponent<CircleCollider2D>();
                player.radius = 0.4f;
                playerObject.AddComponent<PlayerController>();
                playerObject.transform.position = new Vector2(1.1f, 0f);
                Physics2D.SyncTransforms();

                Vector2 before = player.bounds.center;
                health.TakeDamage(1);
                Assert.IsTrue(health.Repair());

                Assert.Less(player.bounds.center.x, before.x);
                Assert.Less(player.bounds.center.x, barrier.bounds.center.x);
                Assert.IsFalse(Physics2D.Distance(barrier, player).isOverlapped);

                yield return new WaitForFixedUpdate();
                Physics2D.SyncTransforms();
                Assert.IsFalse(Physics2D.Distance(barrier, player).isOverlapped,
                    "Player must remain clear on the next physics step.");
            }
            finally
            {
                if (playerObject != null) Object.DestroyImmediate(playerObject);
                if (anchorObject != null) Object.DestroyImmediate(anchorObject);
                if (barrierObject != null) Object.DestroyImmediate(barrierObject);
            }
        }
    }
}

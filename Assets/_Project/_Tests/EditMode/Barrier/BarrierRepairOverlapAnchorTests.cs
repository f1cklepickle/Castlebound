using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Gate
{
    public class BarrierRepairOverlapAnchorTests
    {
        [Test]
        public void ResolveOverlap_UsesAnchorDirection_ForPlayerPushIn()
        {
            GameObject barrierObject = null;
            GameObject anchorObject = null;
            GameObject playerObject = null;

            try
            {
                barrierObject = new GameObject("Barrier");
                var barrier = barrierObject.AddComponent<BoxCollider2D>();
                barrier.size = new Vector2(2f, 2f);

                var hold = barrierObject.AddComponent<EnemyBarrierHoldBehavior>();
                anchorObject = new GameObject("Anchor");
                anchorObject.transform.position = new Vector2(2f, 0f);
                hold.Debug_SetAnchor(anchorObject.transform);

                playerObject = new GameObject("Player");
                playerObject.tag = "Player";
                var player = playerObject.AddComponent<CircleCollider2D>();
                player.radius = 0.4f;
                playerObject.transform.position = new Vector2(1.1f, 0f);
                Physics2D.SyncTransforms();

                Vector2 before = player.bounds.center;
                Vector2 inward = Vector2.left;
                float expectedBarrierExtent = Mathf.Abs(Vector2.Dot(barrier.bounds.extents, inward));
                float expectedPlayerExtent = Mathf.Abs(Vector2.Dot(player.bounds.extents, inward));
                Vector2 expectedHistoricalTarget = (Vector2)barrier.bounds.center +
                    inward * (expectedBarrierExtent + expectedPlayerExtent + 0.01f);
                Assert.IsTrue(Physics2D.Distance(barrier, player).isOverlapped);

                BarrierOverlapResolver.ResolveOverlap(barrier, player, isPlayer: true);
                Physics2D.SyncTransforms();

                Assert.Less(player.bounds.center.x, before.x,
                    "Player should be pushed inward opposite the approach anchor.");
                Assert.Less(player.bounds.center.x, barrier.bounds.center.x,
                    "The historical Player repair contract always chooses the inner side.");
                Assert.IsFalse(Physics2D.Distance(barrier, player).isOverlapped,
                    "Historical repair relocation must fully clear the Barrier.");
                Assert.That(playerObject.transform.position.x,
                    Is.EqualTo(expectedHistoricalTarget.x).Within(0.0001f),
                    "An already-clear historical target must not receive an extra correction.");
                Assert.That(playerObject.transform.position.y,
                    Is.EqualTo(expectedHistoricalTarget.y).Within(0.0001f));
            }
            finally
            {
                if (playerObject != null) Object.DestroyImmediate(playerObject);
                if (anchorObject != null) Object.DestroyImmediate(anchorObject);
                if (barrierObject != null) Object.DestroyImmediate(barrierObject);
            }
        }

        [Test]
        public void ResolveOverlap_CorrectsResidualPenetration_ForRotatedOffsetPlayer()
        {
            GameObject barrierObject = null;
            GameObject anchorObject = null;
            GameObject playerObject = null;

            try
            {
                barrierObject = new GameObject("Barrier");
                var barrier = barrierObject.AddComponent<BoxCollider2D>();
                barrier.size = new Vector2(2f, 2f);

                var hold = barrierObject.AddComponent<EnemyBarrierHoldBehavior>();
                anchorObject = new GameObject("Anchor");
                anchorObject.transform.position = new Vector2(2f, 0f);
                hold.Debug_SetAnchor(anchorObject.transform);

                playerObject = new GameObject("Player");
                playerObject.tag = "Player";
                playerObject.transform.SetPositionAndRotation(
                    new Vector2(1.1f, 0f),
                    Quaternion.Euler(0f, 0f, 90f));
                var body = playerObject.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                var player = playerObject.AddComponent<CircleCollider2D>();
                player.radius = 0.4f;
                player.offset = new Vector2(0f, -0.2f);
                Physics2D.SyncTransforms();

                Vector2 inward = Vector2.left;
                float barrierExtent = Mathf.Abs(Vector2.Dot(barrier.bounds.extents, inward));
                float playerExtent = Mathf.Abs(Vector2.Dot(player.bounds.extents, inward));
                Vector2 historicalTarget = (Vector2)barrier.bounds.center +
                    inward * (barrierExtent + playerExtent + 0.01f);

                body.position = historicalTarget;
                Physics2D.SyncTransforms();
                ColliderDistance2D historicalResult = Physics2D.Distance(barrier, player);
                Assert.IsTrue(historicalResult.isOverlapped,
                    "The rotated offset collider must reproduce residual penetration at the historical root target.");
                Assert.Less(historicalResult.distance, 0f);
                float expectedCorrection = -historicalResult.distance + 0.01f;

                body.position = new Vector2(1.1f, 0f);
                Physics2D.SyncTransforms();
                BarrierOverlapResolver.ResolveOverlap(barrier, player, isPlayer: true);
                Physics2D.SyncTransforms();

                ColliderDistance2D resolved = Physics2D.Distance(barrier, player);
                Assert.IsFalse(resolved.isOverlapped,
                    "The signed-distance correction must fully clear the actual Player collider.");
                Assert.That(body.position.x,
                    Is.EqualTo(historicalTarget.x - expectedCorrection).Within(0.001f));
                Assert.Less(player.bounds.center.x, barrier.bounds.center.x,
                    "Residual correction must preserve the authored inward side.");
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

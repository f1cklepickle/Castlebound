using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Barrier
{
    public class BarrierRepairPlayerRecoveryPlayTests
    {
        [UnityTest]
        public IEnumerator ActiveMovement_RepairPushesInwardAndResynchronizesMover()
        {
            CreateFixture(
                new Vector2(1.1f, 0f),
                new Vector2(0f, -0.2f),
                90f,
                out BarrierHealth health,
                out BoxCollider2D barrier,
                out GameObject anchor,
                out GameObject player,
                out CircleCollider2D playerCollider,
                out PlayerCollisionMove2D mover,
                out PlayerController controller);

            try
            {
                SetField(controller, "movementInput", Vector2.left);
                mover.SetMoveInput(Vector2.left);
                InvokeFixedUpdate(mover);

                Assert.IsTrue(health.Repair());
                AssertClear(playerCollider, barrier);
                Assert.Less(playerCollider.bounds.center.x, barrier.bounds.center.x,
                    "Historical repair relocation must choose the inner side.");

                float repairedX = player.transform.position.x;
                yield return new WaitForFixedUpdate();
                AssertClear(playerCollider, barrier);
                Assert.Less(player.transform.position.x, repairedX,
                    "The next FixedUpdate must continue from the repaired position.");

                SetField(controller, "movementInput", Vector2.right);
                for (int i = 0; i < 20; i++)
                {
                    yield return new WaitForFixedUpdate();
                    AssertClear(playerCollider, barrier);
                }

                float contactX = player.transform.position.x;
                yield return new WaitForFixedUpdate();
                Assert.That(player.transform.position.x, Is.EqualTo(contactX).Within(0.001f));
                AssertClear(playerCollider, barrier);

                SetField(controller, "movementInput", Vector2.left);
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.Less(player.transform.position.x, contactX - 0.05f,
                    "Player must move away from the repaired Barrier without sticking.");
            }
            finally
            {
                DestroyFixture(health.gameObject, anchor, player);
            }
        }

        [UnityTest]
        public IEnumerator PendingMovePositionIntoOpening_IsCancelledWhenBarrierRepairs()
        {
            CreateFixture(
                new Vector2(1.6f, 0f),
                Vector2.zero,
                0f,
                out BarrierHealth health,
                out BoxCollider2D barrier,
                out GameObject anchor,
                out GameObject player,
                out CircleCollider2D playerCollider,
                out PlayerCollisionMove2D mover,
                out PlayerController controller);

            try
            {
                SetField(controller, "movementInput", Vector2.left);
                mover.SetMoveInput(Vector2.left);
                InvokeFixedUpdate(mover);
                Assert.GreaterOrEqual(playerCollider.bounds.center.x, 1.59f,
                    "MovePosition should still be pending before the physics step.");

                Assert.IsTrue(health.Repair());
                AssertClear(playerCollider, barrier);

                yield return new WaitForFixedUpdate();
                AssertClear(playerCollider, barrier);
                Assert.GreaterOrEqual(playerCollider.bounds.center.x, 1.39f,
                    "The stale pre-repair target must not place Player inside the Barrier.");
            }
            finally
            {
                DestroyFixture(health.gameObject, anchor, player);
            }
        }

        private static void CreateFixture(
            Vector2 playerPosition,
            Vector2 colliderOffset,
            float playerRotation,
            out BarrierHealth health,
            out BoxCollider2D barrier,
            out GameObject anchor,
            out GameObject player,
            out CircleCollider2D playerCollider,
            out PlayerCollisionMove2D mover,
            out PlayerController controller)
        {
            health = null;
            barrier = null;
            anchor = null;
            player = null;
            playerCollider = null;
            mover = null;
            controller = null;
            GameObject barrierObject = null;

            try
            {
                int barrierLayer = LayerMask.NameToLayer("Barriers");
                int playerLayer = LayerMask.NameToLayer("Player");

                barrierObject = new GameObject("Barrier");
                barrierObject.layer = barrierLayer;
                barrier = barrierObject.AddComponent<BoxCollider2D>();
                barrier.size = new Vector2(2f, 2f);
                barrierObject.AddComponent<SpriteRenderer>();
                var hold = barrierObject.AddComponent<EnemyBarrierHoldBehavior>();
                anchor = new GameObject("OutsideAnchor");
                anchor.transform.position = new Vector2(2f, 0f);
                hold.Debug_SetAnchor(anchor.transform);
                health = barrierObject.AddComponent<BarrierHealth>();
                health.TakeDamage(health.MaxHealth);

                player = new GameObject("Player");
                player.tag = "Player";
                player.layer = playerLayer;
                player.transform.position = playerPosition;
                player.transform.rotation = Quaternion.Euler(0f, 0f, playerRotation);
                var body = player.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;
                body.gravityScale = 0f;
                playerCollider = player.AddComponent<CircleCollider2D>();
                playerCollider.radius = 0.4f;
                playerCollider.offset = colliderOffset;
                mover = player.AddComponent<PlayerCollisionMove2D>();
                mover.MoveSpeed = 12f;
                SetField(mover, "solidMask", (LayerMask)(1 << barrierLayer));
                controller = player.AddComponent<PlayerController>();
                Physics2D.SyncTransforms();
            }
            catch
            {
                DestroyFixture(barrierObject, anchor, player);
                throw;
            }
        }

        private static void InvokeFixedUpdate(PlayerCollisionMove2D mover)
        {
            MethodInfo method = typeof(PlayerCollisionMove2D).GetMethod(
                "FixedUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(mover, null);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(instance, value);
        }

        private static void AssertClear(Collider2D player, Collider2D barrier)
        {
            Physics2D.SyncTransforms();
            Assert.IsFalse(Physics2D.Distance(barrier, player).isOverlapped,
                "Player must be fully clear of the repaired Barrier.");
        }

        private static void DestroyFixture(
            GameObject barrier,
            GameObject anchor,
            GameObject player)
        {
            if (player != null) Object.DestroyImmediate(player);
            if (anchor != null) Object.DestroyImmediate(anchor);
            if (barrier != null) Object.DestroyImmediate(barrier);
        }
    }
}

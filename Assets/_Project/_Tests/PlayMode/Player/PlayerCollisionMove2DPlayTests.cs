using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Player
{
    public class PlayerCollisionMove2DPlayTests
    {
        private const float PlayerRadius = 0.25f;

        [UnityTest]
        public IEnumerator DiagonalEntryIntoIsolatedCorner_DoesNotOverlapOrTunnel()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(Vector2.zero, 1 << wallsLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D corner = CreateBox("IsolatedCorner", wallsLayer, new Vector2(0.65f, 0.65f), new Vector2(0.4f, 0.4f));

            try
            {
                mover.MoveSpeed = 30f;
                mover.SetMoveInput(Vector2.one);
                yield return WaitFixedSteps(2);

                AssertNotOverlapped(player, corner);
                Assert.That(player.transform.position.x, Is.LessThan(0.4f));
                Assert.That(player.transform.position.y, Is.LessThan(0.4f));
            }
            finally
            {
                Object.Destroy(corner.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator BarrierStraightEdge_SustainedDiagonalInputSlidesAndChangedInputReleases()
        {
            int barriersLayer = LayerMask.NameToLayer("Barriers");
            GameObject player = CreatePlayer(Vector2.zero, 1 << barriersLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D barrier = CreateBox("Barrier", barriersLayer, new Vector2(0.75f, 0f), new Vector2(0.5f, 20f));

            try
            {
                mover.SetMoveInput(Vector2.one);
                yield return WaitFixedSteps(12);

                AssertNotOverlapped(player, barrier);
                Assert.That(player.transform.position.x, Is.LessThanOrEqualTo(0.24f));
                Assert.That(player.transform.position.y, Is.GreaterThan(1f));

                float contactX = player.transform.position.x;
                mover.SetMoveInput(Vector2.left);
                yield return WaitFixedSteps(2);

                Assert.That(player.transform.position.x, Is.LessThan(contactX - 0.1f));
                AssertNotOverlapped(player, barrier);
            }
            finally
            {
                Object.Destroy(barrier.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator TwoWallInsideCorner_SustainedInputStopsDeterministicallyWithoutOverlap()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(Vector2.zero, 1 << wallsLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D vertical = CreateBox("VerticalWall", wallsLayer, new Vector2(0.75f, 0f), new Vector2(0.5f, 4f));
            BoxCollider2D horizontal = CreateBox("HorizontalWall", wallsLayer, new Vector2(0f, 0.75f), new Vector2(4f, 0.5f));

            try
            {
                mover.SetMoveInput(Vector2.one);
                yield return WaitFixedSteps(8);
                Vector2 settled = player.transform.position;
                yield return WaitFixedSteps(8);

                AssertNotOverlapped(player, vertical);
                AssertNotOverlapped(player, horizontal);
                Assert.That(Vector2.Distance(player.transform.position, settled), Is.LessThan(0.001f));
            }
            finally
            {
                Object.Destroy(vertical.gameObject);
                Object.Destroy(horizontal.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator SlopedRotatedBlocker_UsesContactNormalForTangentialMotion()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(Vector2.zero, 1 << wallsLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D slope = CreateBox(
                "SlopedBlocker",
                wallsLayer,
                new Vector2(0.85f, 0.45f),
                new Vector2(0.3f, 1.4f),
                -30f);

            try
            {
                mover.SetMoveInput(Vector2.right);
                yield return WaitFixedSteps(12);

                AssertNotOverlapped(player, slope);
                Assert.That(Mathf.Abs(player.transform.position.y), Is.GreaterThan(0.05f));
            }
            finally
            {
                Object.Destroy(slope.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator BarrierCornerAndSeam_RemainBlockingDuringSustainedSlide()
        {
            int barriersLayer = LayerMask.NameToLayer("Barriers");
            GameObject player = CreatePlayer(new Vector2(0f, -0.7f), 1 << barriersLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D lower = CreateBox("BarrierLower", barriersLayer, new Vector2(0.75f, -2f), new Vector2(0.5f, 4f));
            BoxCollider2D upper = CreateBox("BarrierUpper", barriersLayer, new Vector2(0.75f, 2f), new Vector2(0.5f, 4f));

            try
            {
                mover.SetMoveInput(Vector2.one);
                yield return WaitFixedSteps(18);

                AssertNotOverlapped(player, lower);
                AssertNotOverlapped(player, upper);
                Assert.That(player.transform.position.x, Is.LessThanOrEqualTo(0.24f));
                Assert.That(player.transform.position.y, Is.GreaterThan(1f));
            }
            finally
            {
                Object.Destroy(lower.gameObject);
                Object.Destroy(upper.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator VaultEdge_BlocksSustainedInputWithoutOverlap()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(new Vector2(0f, 0.7f), 1 << wallsLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D vault = CreateVault(wallsLayer, new Vector2(0.9f, 0.7f));

            try
            {
                mover.SetMoveInput(Vector2.right);
                yield return WaitFixedSteps(20);

                AssertNotOverlapped(player, vault);
                Assert.That(player.transform.position.x, Is.LessThanOrEqualTo(0.34f));
            }
            finally
            {
                Object.Destroy(vault.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator VaultCorner_DiagonalApproachCannotEnterCollider()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(Vector2.zero, 1 << wallsLayer, Vector2.zero, out PlayerCollisionMove2D mover);
            BoxCollider2D vault = CreateVault(wallsLayer, new Vector2(0.9f, 0.7f));

            try
            {
                mover.SetMoveInput(Vector2.one);
                yield return WaitFixedSteps(20);

                AssertNotOverlapped(player, vault);
            }
            finally
            {
                Object.Destroy(vault.gameObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator RapidFacingRotationAtWall_SweepsOffsetCenterAndRemainsRecoverable()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            GameObject player = CreatePlayer(Vector2.zero, 1 << wallsLayer, new Vector2(0f, -0.15f), out PlayerCollisionMove2D mover);
            BoxCollider2D wall = CreateBox("Wall", wallsLayer, new Vector2(0.75f, 0f), new Vector2(0.5f, 4f));

            try
            {
                mover.SetMoveInput(Vector2.right);
                yield return WaitFixedSteps(6);

                for (int i = 0; i < 12; i++)
                {
                    player.transform.rotation = Quaternion.Euler(0f, 0f, i * 90f);
                    yield return new WaitForFixedUpdate();
                    AssertNotOverlapped(player, wall);
                }

                float contactX = player.transform.position.x;
                mover.SetMoveInput(Vector2.left);
                yield return WaitFixedSteps(2);

                Assert.That(player.transform.position.x, Is.LessThan(contactX - 0.1f));
                AssertNotOverlapped(player, wall);
            }
            finally
            {
                Object.Destroy(wall.gameObject);
                Object.Destroy(player);
            }
        }

        private static GameObject CreatePlayer(
            Vector2 position,
            int solidMask,
            Vector2 colliderOffset,
            out PlayerCollisionMove2D mover)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.position = position;
            var body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = player.AddComponent<CircleCollider2D>();
            collider.radius = PlayerRadius;
            collider.offset = colliderOffset;
            mover = player.AddComponent<PlayerCollisionMove2D>();
            mover.MoveSpeed = 12f;
            SetField(mover, "solidMask", (LayerMask)solidMask);
            Physics2D.SyncTransforms();
            return player;
        }

        private static BoxCollider2D CreateVault(int layer, Vector2 position)
        {
            BoxCollider2D vault = CreateBox("Vault", layer, position, new Vector2(1.1f, 0.9f));
            vault.edgeRadius = 0.06f;
            return vault;
        }

        private static BoxCollider2D CreateBox(
            string name,
            int layer,
            Vector2 position,
            Vector2 size,
            float rotation = 0f)
        {
            var gameObject = new GameObject(name);
            gameObject.layer = layer;
            gameObject.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));
            var collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = size;
            Physics2D.SyncTransforms();
            return collider;
        }

        private static IEnumerator WaitFixedSteps(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        private static void AssertNotOverlapped(GameObject player, Collider2D blocker)
        {
            Physics2D.SyncTransforms();
            ColliderDistance2D distance = Physics2D.Distance(
                player.GetComponent<CircleCollider2D>(),
                blocker);
            Assert.IsTrue(distance.isValid);
            Assert.IsFalse(
                distance.isOverlapped,
                $"Player overlapped {blocker.name} at {player.transform.position}; " +
                $"separation={distance.distance}, normal={distance.normal}.");
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Expected field {fieldName} on {instance.GetType().Name}.");
            field.SetValue(instance, value);
        }
    }
}

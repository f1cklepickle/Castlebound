using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyControllerOrbitVsBarrierTests
    {
        [Test]
        public void TangentApplied_ForPlayerTarget()
        {
            var player = new GameObject("Player").transform;
            player.tag = "Player";
            var enemy = new GameObject("Enemy");
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.position = Vector2.zero;
            var controller = enemy.AddComponent<EnemyController2D>();

            // Setup player target and gaps.
            controller.Debug_SetupRefs(player, null);
            controller.SetAngularGaps(1f, 0f); // preference != 0
            player.position = new Vector2(2f, 0f);

            EnemyController2D.State state = EnemyController2D.State.CHASE;
            float prevDist = 0f;
            int distTrend = 0;
            Vector2 lastDir = Vector2.right;

            EnemyMovement.ComputeMovement(
                rb.position,
                player,
                null,
                GetPrivateField<float>(controller, "holdRadius"),
                GetPrivateField<float>(controller, "releaseMargin"),
                GetPrivateField<float>(controller, "reseatBias"),
                GetPrivateField<float>(controller, "speed"),
                GetPrivateField<float>(controller, "orbitBase"),
                GetPrivateField<float>(controller, "maxTangent"),
                GetPrivateField<int>(controller, "outrunFrames"),
                GetPrivateField<float>(controller, "epsilonDist"),
                1f,
                0f,
                ref state,
                ref prevDist,
                ref distTrend,
                ref lastDir,
                out Vector2 radial,
                out Vector2 tangent);

            Assert.IsTrue(tangent.sqrMagnitude > 0f, "Tangent should be applied when targeting player with non-zero gaps.");

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player.gameObject);
        }

        [Test]
        public void TangentZero_ForBarrierTarget()
        {
            var player = new GameObject("Player").transform;
            player.tag = "Player";
            var barrier = new GameObject("Barrier").transform;
            barrier.gameObject.AddComponent<BarrierHealth>();

            var enemy = new GameObject("Enemy");
            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.position = Vector2.zero;
            var controller = enemy.AddComponent<EnemyController2D>();

            controller.Debug_SetupRefs(player, barrier);
            barrier.position = new Vector2(2f, 0f);

            EnemyController2D.State state = EnemyController2D.State.CHASE;
            float prevDist = 0f;
            int distTrend = 0;
            Vector2 lastDir = Vector2.right;

            EnemyMovement.ComputeMovement(
                rb.position,
                barrier,
                barrier,
                GetPrivateField<float>(controller, "holdRadius"),
                GetPrivateField<float>(controller, "releaseMargin"),
                GetPrivateField<float>(controller, "reseatBias"),
                GetPrivateField<float>(controller, "speed"),
                GetPrivateField<float>(controller, "orbitBase"),
                GetPrivateField<float>(controller, "maxTangent"),
                GetPrivateField<int>(controller, "outrunFrames"),
                GetPrivateField<float>(controller, "epsilonDist"),
                1f,
                0f,
                ref state,
                ref prevDist,
                ref distTrend,
                ref lastDir,
                out Vector2 radial,
                out Vector2 tangent);

            Assert.AreEqual(0f, tangent.sqrMagnitude, "Tangent should be zero when targeting barrier (no orbit).");

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(barrier.gameObject);
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field != null ? (T)field.GetValue(obj) : default;
        }
    }
}

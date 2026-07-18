using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyRingEligibilityTests
    {
        [Test]
        public void Refresh_ExcludesIneligibleEnemyAndClearsItsStaleGaps()
        {
            var player = new GameObject("Player");
            var managerObject = new GameObject("EnemyRingManager");
            var created = new GameObject[3];

            try
            {
                player.tag = "Player";
                var manager = managerObject.AddComponent<EnemyRingManager>();
                SetManagerPlayer(manager, player.transform);
                created[0] = CreateEnemy("EligibleA", player.transform, new Vector2(2f, 0f));
                created[1] = CreateEnemy("EligibleB", player.transform, PolarPosition(2.1f, 30f));
                created[2] = CreateEnemy("Rooted", player.transform, PolarPosition(100f, 15f));

                created[2].GetComponent<EnemyRootReceiver>().RootForSeconds(10f);
                created[2].GetComponent<EnemyController2D>().SetAngularGaps(1f, 1f);

                InvokeRefresh(manager);

                GetGaps(created[0].GetComponent<EnemyController2D>(), out float eligibleCw, out float eligibleCcw);
                GetGaps(created[2].GetComponent<EnemyController2D>(), out float rootedCw, out float rootedCcw);

                Assert.That(Mathf.Max(eligibleCw, eligibleCcw), Is.GreaterThan(0f),
                    "Eligible melee neighbors should retain their existing angular-gap behavior.");
                Assert.That(rootedCw, Is.Zero);
                Assert.That(rootedCcw, Is.Zero,
                    "An enemy leaving eligibility must not retain stale ring influence.");
            }
            finally
            {
                for (int i = 0; i < created.Length; i++)
                    DestroyEnemy(created[i]);
                Object.DestroyImmediate(managerObject);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void Refresh_ExcludesEnemyWithoutMeleeEligibilityMarker()
        {
            var player = new GameObject("Player");
            var managerObject = new GameObject("EnemyRingManager");
            GameObject unmarked = null;

            try
            {
                player.tag = "Player";
                var manager = managerObject.AddComponent<EnemyRingManager>();
                SetManagerPlayer(manager, player.transform);
                unmarked = CreateEnemy("FutureRanged", player.transform, new Vector2(2f, 0f), addMarker: false);
                var controller = unmarked.GetComponent<EnemyController2D>();
                controller.SetAngularGaps(1f, 1f);

                InvokeRefresh(manager);
                GetGaps(controller, out float gapCw, out float gapCcw);

                Assert.That(gapCw, Is.Zero);
                Assert.That(gapCcw, Is.Zero);
            }
            finally
            {
                DestroyEnemy(unmarked);
                Object.DestroyImmediate(managerObject);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void DisabledController_ClearsCachedAngularGaps()
        {
            var enemy = new GameObject("Enemy");
            var controller = enemy.AddComponent<EnemyController2D>();

            try
            {
                controller.SetAngularGaps(1f, 2f);
                controller.enabled = false;
                InvokeControllerDisable(controller);
                GetGaps(controller, out float gapCw, out float gapCcw);

                Assert.That(gapCw, Is.Zero);
                Assert.That(gapCcw, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        private static GameObject CreateEnemy(
            string name,
            Transform player,
            Vector2 position,
            bool addMarker = true)
        {
            var enemy = new GameObject(name);
            enemy.transform.position = position;
            enemy.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            enemy.AddComponent<EnemyRootReceiver>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Debug_SetupRefs(player);
            SetTargetType(controller, EnemyTargetType.Player);
            InvokeControllerEnable(controller);
            if (addMarker) enemy.AddComponent<EnemySurroundEligibility>();
            return enemy;
        }

        private static Vector2 PolarPosition(float radius, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
        }

        private static void InvokeRefresh(EnemyRingManager manager)
        {
            typeof(EnemyRingManager)
                .GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(manager, null);
        }

        private static void InvokeControllerEnable(EnemyController2D controller)
        {
            typeof(EnemyController2D)
                .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(controller, null);
        }

        private static void InvokeControllerDisable(EnemyController2D controller)
        {
            typeof(EnemyController2D)
                .GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(controller, null);
        }

        private static void SetManagerPlayer(EnemyRingManager manager, Transform player)
        {
            typeof(EnemyRingManager)
                .GetField("player", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(manager, player);
        }

        private static void SetTargetType(EnemyController2D controller, EnemyTargetType targetType)
        {
            typeof(EnemyController2D)
                .GetField("_currentTargetType", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(controller, targetType);
        }

        private static void GetGaps(EnemyController2D controller, out float gapCw, out float gapCcw)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            gapCw = (float)typeof(EnemyController2D).GetField("_gapCW", flags).GetValue(controller);
            gapCcw = (float)typeof(EnemyController2D).GetField("_gapCCW", flags).GetValue(controller);
        }

        private static void DestroyEnemy(GameObject enemy)
        {
            if (enemy == null)
                return;

            var controller = enemy.GetComponent<EnemyController2D>();
            if (controller != null)
                InvokeControllerDisable(controller);
            Object.DestroyImmediate(enemy);
        }
    }
}

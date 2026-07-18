using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyRingEligibilityPlayTests
    {
        [UnityTest]
        public IEnumerator MixedTargetRing_OnlyPlayerTargetingMeleeEnemiesReceiveGaps()
        {
            var player = new GameObject("Player");
            var managerObject = new GameObject("EnemyRingManager");
            GameObject eligibleA = null;
            GameObject eligibleB = null;
            GameObject barrierTarget = null;

            try
            {
                player.tag = "Player";
                var manager = managerObject.AddComponent<EnemyRingManager>();
                eligibleA = CreateEnemy("EligibleA", player.transform, new Vector2(2f, 0f), EnemyTargetType.Player);
                eligibleB = CreateEnemy("EligibleB", player.transform, PolarPosition(2f, 30f), EnemyTargetType.Player);
                barrierTarget = CreateEnemy("BarrierTarget", player.transform, PolarPosition(2f, 15f), EnemyTargetType.Barrier);
                barrierTarget.GetComponent<EnemyController2D>().SetAngularGaps(1f, 1f);

                yield return null;
                SetTargetType(eligibleA.GetComponent<EnemyController2D>(), EnemyTargetType.Player);
                SetTargetType(eligibleB.GetComponent<EnemyController2D>(), EnemyTargetType.Player);
                SetTargetType(barrierTarget.GetComponent<EnemyController2D>(), EnemyTargetType.Barrier);
                InvokeRefresh(manager);

                GetGaps(eligibleA.GetComponent<EnemyController2D>(), out float eligibleCw, out float eligibleCcw);
                GetGaps(barrierTarget.GetComponent<EnemyController2D>(), out float barrierCw, out float barrierCcw);

                Assert.That(Mathf.Max(eligibleCw, eligibleCcw), Is.GreaterThan(0f));
                Assert.That(barrierCw, Is.Zero);
                Assert.That(barrierCcw, Is.Zero);
            }
            finally
            {
                DestroyEnemy(eligibleA);
                DestroyEnemy(eligibleB);
                DestroyEnemy(barrierTarget);
                Object.Destroy(managerObject);
                Object.Destroy(player);
            }
        }

        private static GameObject CreateEnemy(
            string name,
            Transform player,
            Vector2 position,
            EnemyTargetType targetType)
        {
            var enemy = new GameObject(name);
            enemy.transform.position = position;
            enemy.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            enemy.AddComponent<EnemyRootReceiver>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Debug_SetupRefs(player);
            SetTargetType(controller, targetType);
            enemy.AddComponent<EnemySurroundEligibility>();
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

            enemy.SetActive(false);
            Object.Destroy(enemy);
        }
    }
}

using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyApproachSpreadPlayTests
    {
        [UnityTest]
        public IEnumerator ClusteredMeleeGroup_FormsMultiplePathsEarlyInApproach()
        {
            var player = CreatePlayer();
            var managerObject = new GameObject("EnemyRingManager");
            var enemies = new[]
            {
                CreateEnemy(player.transform, new Vector2(12f, 0f)),
                CreateEnemy(player.transform, new Vector2(12f, 0f)),
                CreateEnemy(player.transform, new Vector2(12f, 0f)),
                CreateEnemy(player.transform, new Vector2(12f, 0f)),
                CreateEnemy(player.transform, new Vector2(12f, 0f))
            };
            try
            {
                managerObject.AddComponent<EnemyRingManager>();
                for (int i = 0; i < 15; i++) yield return new WaitForFixedUpdate();
                float[] lateralPositions = GetSortedY(enemies);
                Assert.That(lateralPositions[4] - lateralPositions[0], Is.GreaterThan(0.35f));
                Assert.That(CountDistinct(lateralPositions, 0.04f), Is.GreaterThanOrEqualTo(3));
                Assert.That(Vector2.Distance(enemies[2].transform.position, player.transform.position),
                    Is.GreaterThan(8f), "Spreading should be visible early in the approach.");
            }
            finally
            {
                DestroyEnemies(enemies);
                Object.Destroy(managerObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator AlreadySpacedGroup_ContinuesMostlyForward()
        {
            var player = CreatePlayer();
            var managerObject = new GameObject("EnemyRingManager");
            var enemies = new[]
            {
                CreateEnemy(player.transform, new Vector2(10f, -2f)),
                CreateEnemy(player.transform, new Vector2(10f, 0f)),
                CreateEnemy(player.transform, new Vector2(10f, 2f))
            };
            try
            {
                managerObject.AddComponent<EnemyRingManager>();
                for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
                float[] endY = GetSortedY(enemies);
                Assert.That(Mathf.Abs(endY[0]), Is.LessThan(2f),
                    "An uncrowded enemy should keep pursuing the player instead of peeling outward.");
                Assert.That(Mathf.Abs(endY[2]), Is.LessThan(2f));
                Assert.That(Mathf.Abs(endY[0] + endY[2]), Is.LessThan(0.02f),
                    "An already-spaced symmetric group should remain symmetric.");
            }
            finally
            {
                DestroyEnemies(enemies);
                Object.Destroy(managerObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator LoneMeleeEnemy_ApproachesWithoutSidewaysMotion()
        {
            var player = CreatePlayer();
            var managerObject = new GameObject("EnemyRingManager");
            var enemy = CreateEnemy(player.transform, new Vector2(8f, 0f));
            try
            {
                managerObject.AddComponent<EnemyRingManager>();
                for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
                Assert.That(enemy.transform.position.x, Is.LessThan(8f));
                Assert.That(Mathf.Abs(enemy.transform.position.y), Is.LessThan(0.01f));
            }
            finally
            {
                DestroyEnemies(new[] { enemy });
                Object.Destroy(managerObject);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator ForcedAttackReacquisition_ChasesDirectlyWithoutSpreading()
        {
            var player = CreatePlayer();
            var enemy = CreateEnemy(player.transform, new Vector2(8f, 0f));
            var controller = enemy.GetComponent<EnemyController2D>();
            try
            {
                controller.SetApproachSeparation(Vector2.up, true);
                controller.RequestChase();
                yield return new WaitForFixedUpdate();
                Assert.That(enemy.transform.position.x, Is.LessThan(8f));
                Assert.That(Mathf.Abs(enemy.transform.position.y), Is.LessThan(0.001f));
            }
            finally
            {
                DestroyEnemies(new[] { enemy });
                Object.Destroy(player);
            }
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            return player;
        }

        private static GameObject CreateEnemy(Transform player, Vector2 position)
        {
            var enemy = new GameObject("Enemy");
            enemy.transform.position = position;
            var body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            enemy.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            enemy.AddComponent<EnemyRootReceiver>();
            enemy.AddComponent<EnemySurroundEligibility>();
            enemy.AddComponent<EnemyApproachSpread>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Speed = 8f;
            SetField(controller, "useBarrierTargeting", false);
            controller.Debug_SetupRefs(player);
            SetField(controller, "_currentTargetType", EnemyTargetType.Player);
            return enemy;
        }

        private static float[] GetSortedY(GameObject[] enemies)
        {
            var values = new float[enemies.Length];
            for (int i = 0; i < enemies.Length; i++) values[i] = enemies[i].transform.position.y;
            System.Array.Sort(values);
            return values;
        }

        private static int CountDistinct(float[] sortedValues, float tolerance)
        {
            int count = sortedValues.Length > 0 ? 1 : 0;
            for (int i = 1; i < sortedValues.Length; i++)
                if (sortedValues[i] - sortedValues[i - 1] > tolerance) count++;
            return count;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(instance, value);
        }

        private static void DestroyEnemies(GameObject[] enemies)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) continue;
                enemies[i].SetActive(false);
                Object.Destroy(enemies[i]);
            }
        }
    }
}

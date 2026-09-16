using System.Collections;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyApproachSpreadPlayTests
    {
        [UnityTest]
        public IEnumerator ProductionGeometry_TrailingEnemyAvoidsHoldingFrontBeforeHardContact()
        {
            var player = CreatePlayer(true);
            player.transform.position = new Vector2(1000f, 1000f);
            var enemies = new[]
            {
                CreateEnemy(player.transform, (Vector2)player.transform.position + new Vector2(1.8f, 0f), true, true),
                CreateEnemy(player.transform, (Vector2)player.transform.position + new Vector2(3.2f, 0f), true, true)
            };
            // The front enemy is naturally inside the authored melee engagement range.
            // Both retain a normal chase speed; HOLD, rather than a speed override, stops the front.
            foreach (var enemy in enemies) enemy.GetComponent<EnemyController2D>().Speed = 3.5f;
            try
            {
                Physics2D.SyncTransforms();
                var front = enemies[0].GetComponent<EnemyController2D>();
                var trailing = enemies[1].GetComponent<EnemyController2D>();
                var frontBody = enemies[0].GetComponent<Rigidbody2D>();
                var trailingBody = enemies[1].GetComponent<Rigidbody2D>();
                var frontSensor = enemies[0].GetComponentInChildren<EnemySeparationCollider>();
                var trailingSensor = enemies[1].GetComponentInChildren<EnemySeparationCollider>();
                float hardMinimum = frontSensor.Collider.bounds.extents.x + trailingSensor.Collider.bounds.extents.x;
                Vector2 frontStart = frontBody.position;
                Vector2 previous = trailingBody.position;
                float startingX = previous.x;
                bool avoidedBeforeContact = false;
                bool hardContactObserved = false;
                Assert.That(hardMinimum, Is.EqualTo(0.4f).Within(0.0001f));
                for (int i = 0; i < 20; i++)
                {
                    float beforeDistance = Vector2.Distance(trailingBody.position, frontBody.position);
                    yield return new WaitForFixedUpdate();
                    Vector2 position = trailingBody.position;
                    float distance = Vector2.Distance(position, frontBody.position);
                    Assert.IsTrue(front.IsInHoldRange(), "The front enemy must be an actual settled obstruction.");
                    Assert.That(Vector2.Distance(frontBody.position, frontStart), Is.LessThan(0.001f));
                    if (!avoidedBeforeContact)
                    {
                        hardContactObserved |= distance <= hardMinimum + 0.02f;
                        if (Mathf.Abs(position.y - previous.y) > 0.005f)
                        {
                            Assert.IsFalse(hardContactObserved, "The turn must begin before reaching the hard footprint.");
                            Assert.That(beforeDistance, Is.GreaterThan(hardMinimum + 0.02f));
                            Assert.That(distance, Is.GreaterThan(hardMinimum + 0.02f));
                            Assert.That(frontSensor.DebugFallbackCorrectionCount +
                                trailingSensor.DebugFallbackCorrectionCount, Is.Zero,
                                "Physical overlap recovery must not be the source of lateral travel.");
                            avoidedBeforeContact = true;
                        }
                    }
                    Assert.That(distance, Is.GreaterThanOrEqualTo(hardMinimum - 0.01f));
                    Assert.That(Vector2.Distance(position, previous),
                        Is.LessThanOrEqualTo(trailing.Speed * Time.fixedDeltaTime + 0.003f));
                    Assert.IsFalse(enemies[1].GetComponent<EnemyLocomotion>().HasChaseApproachTarget);
                    previous = position;
                }
                Assert.IsTrue(avoidedBeforeContact, "The trailing enemy must predict and turn around the HOLD enemy.");
                Assert.That(previous.x, Is.LessThan(startingX - 0.2f), "Avoidance must preserve forward progress.");
            }
            finally
            {
                DestroyEnemies(enemies);
                Object.Destroy(player);
            }
        }
        [UnityTest]
        public IEnumerator LongitudinalGroup_PredictsClosingMotionWithoutLegacySpacing()
        {
            var player = CreatePlayer();
            var enemies = new[]
            {
                CreateEnemy(player.transform, new Vector2(16f, 0f), true),
                CreateEnemy(player.transform, new Vector2(15.4f, 0f), true),
                CreateEnemy(player.transform, new Vector2(14.5f, 0f), true),
                CreateEnemy(player.transform, new Vector2(13.8f, 0f), true),
                CreateEnemy(player.transform, new Vector2(12.6f, 0f), true)
            };
            enemies[4].GetComponent<EnemyController2D>().Speed = 2f;
            var startingX = new float[4];
            for (int i = 0; i < 4; i++) startingX[i] = enemies[i].transform.position.x;
            try
            {
                bool curvedBeforeContact = false;
                for (int step = 0; step < 10; step++)
                {
                    Vector2 before = enemies[3].GetComponent<Rigidbody2D>().position;
                    yield return new WaitForFixedUpdate();
                    Vector2 after = enemies[3].GetComponent<Rigidbody2D>().position;
                    float gap = Vector2.Distance(after, enemies[4].GetComponent<Rigidbody2D>().position);
                    curvedBeforeContact |= Mathf.Abs(after.y - before.y) > 0.005f && gap > 0.42f;
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        Assert.IsFalse(enemies[i].GetComponent<EnemyLocomotion>().HasChaseApproachTarget);
                        for (int j = 0; j < i; j++)
                            Assert.That(Vector2.Distance(enemies[i].transform.position, enemies[j].transform.position),
                                Is.GreaterThanOrEqualTo(0.39f));
                    }
                }
                Assert.IsTrue(curvedBeforeContact, "Faster followers must predict the slower front enemy.");
                for (int i = 0; i < 4; i++)
                    Assert.That(enemies[i].transform.position.x, Is.LessThan(startingX[i] - 0.2f),
                        "Every faster follower must continue approaching.");
            }
            finally
            {
                DestroyEnemies(enemies);
                Object.Destroy(player);
            }
        }
        [UnityTest]
        public IEnumerator PredictiveMeleeChase_IgnoresAlternatingLegacySpacingInput()
        {
            var player = CreatePlayer();
            var enemy = CreateEnemy(player.transform, new Vector2(20f, 0f));
            var controller = enemy.GetComponent<EnemyController2D>();
            var body = enemy.GetComponent<Rigidbody2D>();
            float startingX = body.position.x;
            float maximumDirectionChange = 0f;
            Vector2 previousDisplacement = Vector2.zero;

            try
            {
                for (int step = 0; step < 20; step++)
                {
                    float lateralSign = (step & 1) == 0 ? 1f : -1f;
                    controller.SetApproachSeparation(
                        new Vector2(0.03f, 0.04f * lateralSign),
                        true);
                    Vector2 before = body.position;

                    yield return new WaitForFixedUpdate();

                    Vector2 displacement = body.position - before;
                    if (previousDisplacement.sqrMagnitude > 0f && displacement.sqrMagnitude > 0f)
                    {
                        maximumDirectionChange = Mathf.Max(
                            maximumDirectionChange,
                            Vector2.Angle(previousDisplacement, displacement));
                    }

                    previousDisplacement = displacement;
                }

                Assert.That(maximumDirectionChange, Is.LessThan(2f),
                    "Ignored legacy soft-spacing input must not destabilize predictive melee pursuit.");
                Assert.That(body.position.x, Is.LessThan(startingX - 2f),
                    "Predictive melee pursuit must preserve forward progress despite alternating legacy input.");
                Assert.That(Vector2.Distance(body.position, player.transform.position),
                    Is.GreaterThan(13f),
                    "The regression must remain outside near-Player surround arrival.");
            }
            finally
            {
                DestroyEnemies(new[] { enemy });
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

        private static GameObject CreatePlayer(bool withCollider = false)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            if (withCollider)
            {
                player.layer = LayerMask.NameToLayer("Player");
                player.AddComponent<CircleCollider2D>().radius = 0.5f;
            }
            return player;
        }

        private static GameObject CreateEnemy(Transform player, Vector2 position,
            bool withSeparation = false, bool productionGeometry = false)
        {
            var enemy = new GameObject("Enemy");
            enemy.transform.position = position;
            var body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            if (withSeparation)
            {
                enemy.layer = LayerMask.NameToLayer("Enemies");
                GameObject sensorObject = enemy;
                if (productionGeometry)
                {
                    body.constraints = RigidbodyConstraints2D.FreezeRotation;
                    var primary = enemy.AddComponent<CircleCollider2D>();
                    primary.radius = 0.87684506f;
                    primary.excludeLayers = 1 << enemy.layer;
                    primary.layerOverridePriority = 2;
                    sensorObject = new GameObject("EnemySeparation");
                    sensorObject.layer = enemy.layer;
                    sensorObject.transform.SetParent(enemy.transform, false);
                }
                var sensor = sensorObject.AddComponent<CircleCollider2D>();
                sensor.radius = 0.2f;
                sensor.isTrigger = true;
                sensor.includeLayers = 1 << enemy.layer;
                sensor.excludeLayers = ~(1 << enemy.layer);
                sensorObject.AddComponent<EnemySeparationCollider>();
            }
            enemy.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            enemy.AddComponent<EnemyRootReceiver>();
            enemy.AddComponent<EnemySurroundEligibility>().AvoidanceGroup = PredictiveAvoidanceGroup.SmallMelee;
            enemy.AddComponent<EnemyApproachSpread>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Speed = 8f;
            controller.Debug_SetBarrierTargeting(false);
            controller.Debug_SetupRefs(player);
            controller.Debug_SetTargetDecision(player.transform, player.transform, EnemyTargetType.Player);
            return enemy;
        }

        private static float[] GetSortedY(GameObject[] enemies)
        {
            var values = new float[enemies.Length];
            for (int i = 0; i < enemies.Length; i++) values[i] = enemies[i].transform.position.y;
            System.Array.Sort(values);
            return values;
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

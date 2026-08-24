using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Projectile;
using Castlebound.Gameplay.World.Placement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemySeparationPlayTests
    {
        private const string MeleePath =
            "Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab";
        private const string RangedPath =
            "Assets/_Project/Prefabs/Enemy_Goblin_Ranged.prefab";
        private const string LurkerPath =
            "Assets/_Project/Prefabs/Enemy_Lurker.prefab";

        [UnityTest]
        public IEnumerator OpposingMovePositionEnemies_StopAtTheMinimumFootprint()
        {
            GameObject left = CreateEnemy(MeleePath, new Vector2(-1f, 0f));
            GameObject right = CreateEnemy(MeleePath, new Vector2(1f, 0f));

            try
            {
                for (int i = 0; i < 50; i++)
                {
                    left.GetComponent<Rigidbody2D>().MovePosition(
                        left.GetComponent<Rigidbody2D>().position + Vector2.right * 0.04f);
                    right.GetComponent<Rigidbody2D>().MovePosition(
                        right.GetComponent<Rigidbody2D>().position + Vector2.left * 0.04f);
                    yield return new WaitForFixedUpdate();
                }

                AssertMinimumSeparation(left, right);
                AssertSpritesCanOverlap(left, right);
            }
            finally
            {
                DestroyNow(left, right);
            }
        }

        [UnityTest]
        public IEnumerator SustainedInwardLocomotion_ClampsAtMinimumWithoutRepeatedHardCorrection()
        {
            GameObject left = CreateEnemy(MeleePath, new Vector2(-0.19f, 0f));
            GameObject right = CreateEnemy(RangedPath, new Vector2(0.19f, 0f));

            try
            {
                for (int i = 0; i < 3; i++)
                    yield return new WaitForFixedUpdate();

                EnemySeparationCollider leftSensor =
                    left.GetComponentInChildren<EnemySeparationCollider>();
                EnemySeparationCollider rightSensor =
                    right.GetComponentInChildren<EnemySeparationCollider>();
                leftSensor.Debug_ResetFallbackCorrectionCount();
                rightSensor.Debug_ResetFallbackCorrectionCount();

                Rigidbody2D leftBody = left.GetComponent<Rigidbody2D>();
                Rigidbody2D rightBody = right.GetComponent<Rigidbody2D>();
                EnemyLocomotion leftLocomotion = left.GetComponent<EnemyLocomotion>();
                EnemyLocomotion rightLocomotion = right.GetComponent<EnemyLocomotion>();
                float minimumDistance = leftSensor.Collider.bounds.extents.x +
                    rightSensor.Collider.bounds.extents.x;
                float minimumObserved = float.PositiveInfinity;
                float settledMinimum = float.PositiveInfinity;
                float settledMaximum = float.NegativeInfinity;
                Vector2 startingMidpoint = (leftBody.position + rightBody.position) * 0.5f;

                for (int step = 0; step < 30; step++)
                {
                    Vector2 pairDelta = rightSensor.Collider.bounds.center -
                        leftSensor.Collider.bounds.center;
                    Vector2 pairNormal = pairDelta.sqrMagnitude > 0f
                        ? pairDelta.normalized
                        : Vector2.right;
                    Vector2 sharedTangent = new Vector2(
                        -pairNormal.y,
                        pairNormal.x) * 0.5f;

                    Assert.IsTrue(leftLocomotion.ExecuteMovement(
                        leftBody,
                        pairNormal * 2f,
                        sharedTangent,
                        Time.fixedDeltaTime));
                    Assert.IsTrue(rightLocomotion.ExecuteMovement(
                        rightBody,
                        -pairNormal * 2f,
                        sharedTangent,
                        Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();

                    float distance = Vector2.Distance(
                        leftSensor.Collider.bounds.center,
                        rightSensor.Collider.bounds.center);
                    minimumObserved = Mathf.Min(minimumObserved, distance);
                    if (step >= 10)
                    {
                        settledMinimum = Mathf.Min(settledMinimum, distance);
                        settledMaximum = Mathf.Max(settledMaximum, distance);
                    }
                }

                Assert.That(minimumObserved,
                    Is.GreaterThanOrEqualTo(minimumDistance - 0.005f));
                Assert.That(settledMaximum - settledMinimum, Is.LessThan(0.01f),
                    "Sustained convergence must settle without threshold oscillation.");
                Assert.That(
                    leftSensor.DebugFallbackCorrectionCount +
                    rightSensor.DebugFallbackCorrectionCount,
                    Is.LessThanOrEqualTo(1),
                    "Ordinary locomotion must not rely on repeated fallback correction.");
                Vector2 endingMidpoint = (leftBody.position + rightBody.position) * 0.5f;
                Assert.That(Vector2.Distance(startingMidpoint, endingMidpoint),
                    Is.GreaterThan(0.2f),
                    "The hard minimum must preserve tangential movement.");
                AssertSpritesCanOverlap(left, right);
            }
            finally
            {
                DestroyNow(left, right);
            }
        }

        [UnityTest]
        public IEnumerator CoincidentMixedEnemies_RecoverWithoutLosingDenseOverlap()
        {
            GameObject melee = CreateEnemy(MeleePath, Vector2.zero);
            GameObject ranged = CreateEnemy(RangedPath, Vector2.zero);

            try
            {
                for (int i = 0; i < 12; i++)
                    yield return new WaitForFixedUpdate();

                AssertMinimumSeparation(melee, ranged);
                AssertSpritesCanOverlap(melee, ranged);
            }
            finally
            {
                DestroyNow(melee, ranged);
            }
        }

        [UnityTest]
        public IEnumerator DenseMixedGroup_ProgressesBesideWallVaultAndBarrier()
        {
            int wallsLayer = LayerMask.NameToLayer("Walls");
            int barriersLayer = LayerMask.NameToLayer("Barriers");
            var wall = CreateBlocker(
                "Wall",
                wallsLayer,
                new Vector2(0f, -1.8f),
                new Vector2(10f, 0.4f));
            var vault = CreateBlocker(
                "Vault",
                wallsLayer,
                new Vector2(0f, 1.8f),
                new Vector2(1.1f, 0.4f));
            var barrier = CreateBlocker(
                "Barrier",
                barriersLayer,
                new Vector2(1f, 0f),
                new Vector2(0.5f, 2.8f));
            GameObject[] enemies =
            {
                CreateEnemy(MeleePath, new Vector2(-3f, -0.55f)),
                CreateEnemy(RangedPath, new Vector2(-3f, -0.15f)),
                CreateEnemy(LurkerPath, new Vector2(-3f, 0.25f)),
                CreateEnemy(MeleePath, new Vector2(-3.45f, -0.35f)),
                CreateEnemy(RangedPath, new Vector2(-3.45f, 0.05f)),
                CreateEnemy(LurkerPath, new Vector2(-3.45f, 0.45f))
            };

            try
            {
                float startAverageX = AverageX(enemies);
                for (int step = 0; step < 100; step++)
                {
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        Rigidbody2D body = enemies[i].GetComponent<Rigidbody2D>();
                        body.MovePosition(body.position + Vector2.right * 0.04f);
                    }

                    yield return new WaitForFixedUpdate();
                }

                for (int i = 0; i < 4; i++)
                    yield return new WaitForFixedUpdate();

                Assert.That(AverageX(enemies), Is.GreaterThan(startAverageX + 1f),
                    "A dense mixed crowd must continue progressing toward its blocker.");
                AssertAllPairsSeparated(enemies);

                for (int i = 0; i < enemies.Length; i++)
                {
                    CircleCollider2D primary = enemies[i].GetComponent<CircleCollider2D>();
                    AssertStableAgainst(primary, wall);
                    AssertStableAgainst(primary, vault);
                    AssertStableAgainst(primary, barrier);
                    AssertSeparationIgnoresWorld(enemies[i], wall, vault, barrier);
                    Assert.That(enemies[i].GetComponent<Rigidbody2D>().velocity.sqrMagnitude,
                        Is.LessThan(100f),
                        "World-adjacent crowd contact must not launch an enemy.");
                }
            }
            finally
            {
                DestroyNow(enemies);
                DestroyNow(wall.gameObject, vault.gameObject, barrier.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator RootStaggerAndKnockbackRemainFunctionalInASeparatedCrowd()
        {
            GameObject rooted = CreateEnemy(MeleePath, Vector2.zero);
            GameObject rootNeighbor = CreateEnemy(RangedPath, new Vector2(0.3f, 0f));
            GameObject staggered = CreateEnemy(MeleePath, new Vector2(2f, 0f));
            GameObject staggerNeighbor = CreateEnemy(LurkerPath, new Vector2(2.3f, 0f));
            GameObject knocked = CreateEnemy(MeleePath, new Vector2(-3f, 0f));
            GameObject knockNeighbor = CreateEnemy(RangedPath, new Vector2(-2.55f, 0f));

            try
            {
                EnemyRootReceiver root = rooted.GetComponent<EnemyRootReceiver>() ??
                    rooted.AddComponent<EnemyRootReceiver>();
                root.RootAt(Vector2.zero, 1f);
                float rootContactCorrection = ContactCorrectionAllowance(rooted, rootNeighbor);
                Assert.IsFalse(rooted.GetComponent<EnemyLocomotion>().ExecuteMovement(
                    rooted.GetComponent<Rigidbody2D>(),
                    Vector2.right * 10f,
                    Vector2.zero,
                    0.1f),
                    "Root must reject locomotion even while resolving Enemy contact.");

                EnemyController2D staggerController = staggered.GetComponent<EnemyController2D>();
                staggerController.enabled = true;
                EnemyStaggerReceiver stagger = staggered.GetComponent<EnemyStaggerReceiver>();
                Assert.NotNull(stagger);
                Assert.IsTrue(stagger.TryStagger());
                float staggerContactCorrection = ContactCorrectionAllowance(
                    staggered,
                    staggerNeighbor);

                EnemyKnockbackReceiver knockback = knocked.GetComponent<EnemyKnockbackReceiver>();
                EnemyLocomotion locomotion = knocked.GetComponent<EnemyLocomotion>();
                knockback.AddKnockback(Vector2.right * 8f, 5f);
                Assert.IsTrue(locomotion.ExecuteMovement(
                    knocked.GetComponent<Rigidbody2D>(),
                    Vector2.zero,
                    Vector2.zero,
                    0.1f));

                for (int i = 0; i < 6; i++)
                    yield return new WaitForFixedUpdate();

                Assert.That(rooted.GetComponent<Rigidbody2D>().position.magnitude,
                    Is.LessThanOrEqualTo(rootContactCorrection),
                    "Root must remain immovable during hard separation.");
                Assert.IsTrue(root.IsRooted);
                AssertMinimumSeparation(rooted, rootNeighbor);
                Assert.That(Vector2.Distance(
                        staggered.GetComponent<Rigidbody2D>().position,
                        stagger.LockPosition),
                    Is.LessThanOrEqualTo(staggerContactCorrection),
                    "Stagger must retain its authored movement lock.");
                Assert.That(stagger.State, Is.EqualTo(EnemyStaggerState.Staggered));
                AssertMinimumSeparation(staggered, staggerNeighbor);
                Assert.That(knocked.GetComponent<Rigidbody2D>().position.x, Is.GreaterThan(-2.95f),
                    "Knockback displacement must still be consumed.");
                Assert.That(knockNeighbor.GetComponent<Rigidbody2D>().position.x, Is.LessThan(-1.5f),
                    "A nearby enemy must not be launched into a chain reaction.");
            }
            finally
            {
                DestroyNow(rooted, rootNeighbor, staggered, staggerNeighbor, knocked, knockNeighbor);
            }
        }

        [UnityTest]
        public IEnumerator LockedCoincidentPair_WaitsUntilOneEnemyCanMove()
        {
            GameObject first = CreateEnemy(MeleePath, Vector2.zero);
            GameObject second = CreateEnemy(RangedPath, Vector2.zero);

            try
            {
                EnemyRootReceiver firstRoot = first.GetComponent<EnemyRootReceiver>() ??
                    first.AddComponent<EnemyRootReceiver>();
                EnemyRootReceiver secondRoot = second.GetComponent<EnemyRootReceiver>() ??
                    second.AddComponent<EnemyRootReceiver>();
                firstRoot.RootAt(Vector2.zero, 1f);
                secondRoot.RootAt(Vector2.zero, 1f);

                for (int i = 0; i < 4; i++)
                    yield return new WaitForFixedUpdate();

                Assert.That(first.GetComponent<Rigidbody2D>().position.magnitude,
                    Is.LessThanOrEqualTo(Physics2D.defaultContactOffset));
                Assert.That(second.GetComponent<Rigidbody2D>().position.magnitude,
                    Is.LessThanOrEqualTo(Physics2D.defaultContactOffset));

                firstRoot.ClearRoot();
                for (int i = 0; i < 12; i++)
                    yield return new WaitForFixedUpdate();

                Assert.That(second.GetComponent<Rigidbody2D>().position.magnitude,
                    Is.LessThanOrEqualTo(Physics2D.defaultContactOffset),
                    "The still-rooted enemy must remain immovable.");
                AssertMinimumSeparation(first, second);
            }
            finally
            {
                DestroyNow(first, second);
            }
        }

        [UnityTest]
        public IEnumerator SeparationChild_DoesNotBecomeAHitProjectileOrTrapTarget()
        {
            int enemiesLayer = LayerMask.NameToLayer("Enemies");
            GameObject enemy = CreateEnemy(MeleePath, Vector2.zero);
            CircleCollider2D primary = enemy.GetComponent<CircleCollider2D>();
            Health health = enemy.GetComponent<Health>();
            int startingHealth = health.Current;
            primary.enabled = false;

            var hitboxObject = new GameObject("Hitbox");
            hitboxObject.layer = LayerMask.NameToLayer("Default");
            var hitCollider = hitboxObject.AddComponent<CircleCollider2D>();
            hitCollider.isTrigger = true;
            hitCollider.radius = 0.5f;
            Hitbox hitbox = hitboxObject.AddComponent<Hitbox>();
            hitbox.Activate();

            GameObject trapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/BearTrap.prefab");
            GameObject trapObject = Object.Instantiate(trapPrefab, Vector3.zero, Quaternion.identity);
            BearTrapTrigger trap = trapObject.GetComponent<BearTrapTrigger>();

            GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Projectile_Rock.prefab");
            GameObject projectileObject = Object.Instantiate(
                projectilePrefab,
                Vector3.zero,
                Quaternion.identity);
            ProjectileRuntime projectile = projectileObject.GetComponent<ProjectileRuntime>();
            projectile.Launch(new ProjectileLaunchContext(
                Vector2.zero,
                Vector2.right,
                null,
                0.01f,
                1,
                1f,
                (LayerMask)(1 << enemiesLayer)));

            try
            {
                yield return new WaitForFixedUpdate();
                yield return null;

                Assert.That(health.Current, Is.EqualTo(startingHealth));
                Assert.IsFalse(trap.IsSpent);
                Assert.NotNull(projectile);
                Assert.IsTrue(projectile.GetComponent<Collider2D>().enabled,
                    "The projectile must not impact the separation-only child.");
            }
            finally
            {
                DestroyNow(enemy, hitboxObject, trapObject, projectileObject);
            }
        }

        [UnityTest]
        public IEnumerator PlayerSweepStillStopsOnThePrimaryEnemyBody()
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int enemiesLayer = LayerMask.NameToLayer("Enemies");
            GameObject enemy = CreateEnemy(MeleePath, Vector2.zero);
            CircleCollider2D enemyPrimary = enemy.GetComponent<CircleCollider2D>();
            var player = new GameObject("Player");
            player.layer = playerLayer;
            player.transform.position = new Vector2(-3f, 0f);
            Rigidbody2D playerBody = player.AddComponent<Rigidbody2D>();
            playerBody.bodyType = RigidbodyType2D.Kinematic;
            CircleCollider2D playerCollider = player.AddComponent<CircleCollider2D>();
            playerCollider.radius = 0.4f;
            PlayerCollisionMove2D mover = player.AddComponent<PlayerCollisionMove2D>();
            mover.MoveSpeed = 3f;
            SetField(mover, "solidMask", (LayerMask)(1 << enemiesLayer));
            mover.SetMoveInput(Vector2.right);

            try
            {
                for (int i = 0; i < 80; i++)
                    yield return new WaitForFixedUpdate();

                Physics2D.SyncTransforms();
                Assert.IsFalse(Physics2D.Distance(playerCollider, enemyPrimary).isOverlapped);
                float centerDistance = Vector2.Distance(playerBody.position, enemy.transform.position);
                float expectedContactDistance = playerCollider.radius + enemyPrimary.radius;
                Assert.That(centerDistance,
                    Is.InRange(expectedContactDistance - 0.02f, expectedContactDistance + 0.08f),
                    "Player movement must retain the existing primary-body contact distance.");
            }
            finally
            {
                DestroyNow(enemy, player);
            }
        }

        private static GameObject CreateEnemy(string path, Vector2 position)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.NotNull(prefab, $"Missing enemy prefab at {path}.");
            GameObject enemy = Object.Instantiate(prefab, position, Quaternion.identity);
            EnemyController2D controller = enemy.GetComponent<EnemyController2D>();
            if (controller != null)
                controller.enabled = false;
            EnemyAttack attack = enemy.GetComponent<EnemyAttack>();
            if (attack != null)
                attack.enabled = false;
            return enemy;
        }

        private static BoxCollider2D CreateBlocker(
            string name,
            int layer,
            Vector2 position,
            Vector2 size)
        {
            var blocker = new GameObject(name);
            blocker.layer = layer;
            blocker.transform.position = position;
            BoxCollider2D collider = blocker.AddComponent<BoxCollider2D>();
            collider.size = size;
            return collider;
        }

        private static void AssertMinimumSeparation(GameObject a, GameObject b)
        {
            CircleCollider2D footprintA =
                a.GetComponentInChildren<EnemySeparationCollider>().Collider;
            CircleCollider2D footprintB =
                b.GetComponentInChildren<EnemySeparationCollider>().Collider;
            float minimum = footprintA.bounds.extents.x + footprintB.bounds.extents.x;
            float actual = Vector2.Distance(
                footprintA.bounds.center,
                footprintB.bounds.center);
            Assert.That(actual, Is.GreaterThanOrEqualTo(minimum - 0.03f),
                "Enemies must not settle effectively center-on-center.");
        }

        private static void AssertSpritesCanOverlap(GameObject a, GameObject b)
        {
            SpriteRenderer spriteA = a.GetComponentInChildren<SpriteRenderer>(true);
            SpriteRenderer spriteB = b.GetComponentInChildren<SpriteRenderer>(true);
            float visibleWidth = Mathf.Min(spriteA.bounds.size.x, spriteB.bounds.size.x);
            float actual = Vector2.Distance(a.transform.position, b.transform.position);
            Assert.That(actual, Is.LessThan(visibleWidth),
                "The minimum footprint must remain smaller than the visible sprites.");
        }

        private static void AssertAllPairsSeparated(GameObject[] enemies)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                for (int j = i + 1; j < enemies.Length; j++)
                    AssertMinimumSeparation(enemies[i], enemies[j]);
            }
        }

        private static void AssertStableAgainst(Collider2D actor, Collider2D blocker)
        {
            Physics2D.SyncTransforms();
            ColliderDistance2D distance = Physics2D.Distance(actor, blocker);
            Assert.That(distance.distance,
                Is.GreaterThanOrEqualTo(-Physics2D.defaultContactOffset),
                $"{actor.name} must remain stable beside {blocker.name}; " +
                $"signed distance was {distance.distance}.");
        }

        private static void AssertSeparationIgnoresWorld(
            GameObject enemy,
            params Collider2D[] blockers)
        {
            CircleCollider2D separation =
                enemy.GetComponentInChildren<EnemySeparationCollider>().Collider;
            for (int i = 0; i < blockers.Length; i++)
            {
                Assert.IsFalse(separation.IsTouching(blockers[i]),
                    $"Separation-only contact must ignore {blockers[i].name}.");
            }
        }

        private static float ContactCorrectionAllowance(GameObject a, GameObject b)
        {
            Physics2D.SyncTransforms();
            CircleCollider2D footprintA =
                a.GetComponentInChildren<EnemySeparationCollider>().Collider;
            CircleCollider2D footprintB =
                b.GetComponentInChildren<EnemySeparationCollider>().Collider;
            ColliderDistance2D initialDistance = Physics2D.Distance(footprintA, footprintB);
            float initialPenetration = Mathf.Max(0f, -initialDistance.distance);
            return initialPenetration + Physics2D.defaultContactOffset;
        }

        private static float AverageX(GameObject[] enemies)
        {
            float sum = 0f;
            for (int i = 0; i < enemies.Length; i++)
                sum += enemies[i].transform.position.x;
            return sum / enemies.Length;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            field.SetValue(instance, value);
        }

        private static void DestroyNow(params GameObject[] objects)
        {
            if (objects == null)
                return;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                    Object.DestroyImmediate(objects[i]);
            }
        }
    }
}

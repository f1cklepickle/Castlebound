using System.Reflection;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.World.Placement;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.World.Placement
{
    public class BearTrapTriggerTests
    {
        [Test]
        public void Defaults_MatchIssueContract()
        {
            var trapObject = new GameObject("BearTrap");
            var trap = trapObject.AddComponent<BearTrapTrigger>();

            try
            {
                Assert.That(trap.Damage, Is.EqualTo(2));
                Assert.That(trap.HoldDurationSeconds, Is.EqualTo(5f));
                Assert.That(trap.WaveLifetime, Is.EqualTo(1));
                Assert.IsTrue(trap.DisappearAfterWaveEnd);
                Assert.IsTrue(trap.IsArmed);
                Assert.IsFalse(trap.IsSpent);
            }
            finally
            {
                Object.DestroyImmediate(trapObject);
            }
        }

        [Test]
        public void Trigger_DamagesOneEnemyRootsItAndSpendsTrap()
        {
            var trapObject = CreateTrap(out var trap);
            var enemyObject = CreateEnemy(out var health, out var rootReceiver);

            try
            {
                trapObject.transform.position = new Vector3(3f, -2f, 0f);
                enemyObject.transform.position = new Vector3(3.4f, -2f, 0f);

                InvokeTriggerEnter(trap, enemyObject.GetComponent<Collider2D>());

                Assert.That(health.Current, Is.EqualTo(8));
                Assert.IsTrue(rootReceiver.IsRooted);
                Assert.That(rootReceiver.RemainingRootSeconds, Is.EqualTo(5f).Within(0.001f));
                Assert.That((Vector2)enemyObject.transform.position, Is.EqualTo((Vector2)trapObject.transform.position));
                Assert.IsTrue(trap.IsSpent);
                Assert.IsFalse(trap.IsArmed);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(trapObject);
            }
        }

        [Test]
        public void Trigger_IgnoresSecondEnemyAfterSpent()
        {
            var trapObject = CreateTrap(out var trap);
            var firstEnemy = CreateEnemy(out var firstHealth, out var firstRoot);
            var secondEnemy = CreateEnemy(out var secondHealth, out var secondRoot);

            try
            {
                InvokeTriggerEnter(trap, firstEnemy.GetComponent<Collider2D>());
                InvokeTriggerEnter(trap, secondEnemy.GetComponent<Collider2D>());

                Assert.That(firstHealth.Current, Is.EqualTo(8));
                Assert.IsTrue(firstRoot.IsRooted);
                Assert.That(secondHealth.Current, Is.EqualTo(10));
                Assert.IsFalse(secondRoot.IsRooted);
            }
            finally
            {
                Object.DestroyImmediate(secondEnemy);
                Object.DestroyImmediate(firstEnemy);
                Object.DestroyImmediate(trapObject);
            }
        }

        [Test]
        public void Trigger_IgnoresEnemyAlreadyRootedByAnotherTrap()
        {
            var firstTrapObject = CreateTrap(out var firstTrap);
            var secondTrapObject = CreateTrap(out var secondTrap);
            var enemyObject = CreateEnemy(out var health, out var rootReceiver);

            try
            {
                firstTrapObject.transform.position = new Vector3(1f, 0f, 0f);
                secondTrapObject.transform.position = new Vector3(2f, 0f, 0f);

                InvokeTriggerEnter(firstTrap, enemyObject.GetComponent<Collider2D>());
                InvokeTriggerEnter(secondTrap, enemyObject.GetComponent<Collider2D>());

                Assert.That(health.Current, Is.EqualTo(8));
                Assert.IsTrue(rootReceiver.IsRooted);
                Assert.IsTrue(firstTrap.IsSpent);
                Assert.IsTrue(secondTrap.IsArmed, "A rooted enemy should not consume another armed trap.");
                Assert.That((Vector2)enemyObject.transform.position, Is.EqualTo((Vector2)firstTrapObject.transform.position));
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(secondTrapObject);
                Object.DestroyImmediate(firstTrapObject);
            }
        }

        [Test]
        public void Trigger_OtherEnemyCanTriggerAnotherArmedTrap()
        {
            var firstTrapObject = CreateTrap(out var firstTrap);
            var secondTrapObject = CreateTrap(out var secondTrap);
            var firstEnemy = CreateEnemy(out var firstHealth, out _);
            var secondEnemy = CreateEnemy(out var secondHealth, out _);

            try
            {
                firstTrapObject.transform.position = new Vector3(1f, 0f, 0f);
                secondTrapObject.transform.position = new Vector3(2f, 0f, 0f);

                InvokeTriggerEnter(firstTrap, firstEnemy.GetComponent<Collider2D>());
                InvokeTriggerEnter(secondTrap, secondEnemy.GetComponent<Collider2D>());

                Assert.That(firstHealth.Current, Is.EqualTo(8));
                Assert.That(secondHealth.Current, Is.EqualTo(8));
                Assert.IsTrue(firstTrap.IsSpent);
                Assert.IsTrue(secondTrap.IsSpent);
            }
            finally
            {
                Object.DestroyImmediate(secondEnemy);
                Object.DestroyImmediate(firstEnemy);
                Object.DestroyImmediate(secondTrapObject);
                Object.DestroyImmediate(firstTrapObject);
            }
        }

        [Test]
        public void WaveEnd_DestroyPolicyDeletesTrapAfterLifetime()
        {
            var trapObject = CreateTrap(out var trap);
            var enemyObject = CreateEnemy(out _, out _);

            try
            {
                InvokeTriggerEnter(trap, enemyObject.GetComponent<Collider2D>());

                trap.HandleWaveEnded();

                Assert.IsTrue(trapObject == null);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                if (trapObject != null)
                {
                    Object.DestroyImmediate(trapObject);
                }
            }
        }

        [Test]
        public void WaveEnd_ResetPolicyRearmsSpentTrapAfterLifetime()
        {
            var trapObject = CreateTrap(out var trap);
            var enemyObject = CreateEnemy(out _, out _);

            try
            {
                SetPrivateField(trap, "disappearAfterWaveEnd", false);
                InvokeTriggerEnter(trap, enemyObject.GetComponent<Collider2D>());

                trap.HandleWaveEnded();

                Assert.IsTrue(trap.IsArmed);
                Assert.IsFalse(trap.IsSpent);
                Assert.IsNotNull(trapObject);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(trapObject);
            }
        }

        [Test]
        public void RootReceiver_ReleasesAfterDuration()
        {
            var enemyObject = new GameObject("Enemy");
            var rootReceiver = enemyObject.AddComponent<EnemyRootReceiver>();

            try
            {
                rootReceiver.RootForSeconds(5f);
                rootReceiver.Tick(2f);
                Assert.IsTrue(rootReceiver.IsRooted);

                rootReceiver.Tick(3f);
                Assert.IsFalse(rootReceiver.IsRooted);
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
            }
        }

        [Test]
        public void EnemyController_DoesNotMoveWhileRooted()
        {
            var playerObject = new GameObject("Player");
            var enemyObject = new GameObject("Enemy");

            try
            {
                playerObject.transform.position = new Vector3(5f, 0f, 0f);
                var body = enemyObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var rootReceiver = enemyObject.AddComponent<EnemyRootReceiver>();
                var controller = enemyObject.AddComponent<EnemyController2D>();
                controller.Speed = 3f;
                controller.Debug_SetupRefs(playerObject.transform);
                rootReceiver.RootForSeconds(5f);

                InvokeFixedUpdate(controller);

                Assert.That(body.position, Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.DestroyImmediate(enemyObject);
                Object.DestroyImmediate(playerObject);
            }
        }

        private static GameObject CreateTrap(out BearTrapTrigger trap)
        {
            var trapObject = new GameObject("BearTrap");
            var collider = trapObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            trap = trapObject.AddComponent<BearTrapTrigger>();
            return trapObject;
        }

        private static GameObject CreateEnemy(out Health health, out EnemyRootReceiver rootReceiver)
        {
            var enemyObject = new GameObject("Enemy");
            enemyObject.tag = "Enemy";
            enemyObject.layer = LayerMask.NameToLayer("Enemies");
            enemyObject.AddComponent<BoxCollider2D>();
            health = enemyObject.AddComponent<Health>();
            health.ConfigureMaxHealth(10, true);
            rootReceiver = enemyObject.AddComponent<EnemyRootReceiver>();
            return enemyObject;
        }

        private static void InvokeTriggerEnter(BearTrapTrigger trap, Collider2D collider)
        {
            var method = typeof(BearTrapTrigger).GetMethod(
                "OnTriggerEnter2D",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, "Expected BearTrapTrigger.OnTriggerEnter2D for trigger contract.");
            method.Invoke(trap, new object[] { collider });
        }

        private static void InvokeFixedUpdate(EnemyController2D controller)
        {
            var method = typeof(EnemyController2D).GetMethod(
                "FixedUpdate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, "Expected EnemyController2D.FixedUpdate for root movement regression guard.");
            method.Invoke(controller, null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(target, value);
        }
    }
}

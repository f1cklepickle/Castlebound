using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTests
    {
        private class DummyDamageable : IDamageable
        {
            public int DamageTaken { get; private set; }
            public void TakeDamage(int amount) => DamageTaken += amount;
        }

        [Test]
        public void DefaultDelivery_RemainsMeleeForExistingEnemies()
        {
            var enemy = new GameObject("Enemy");
            try
            {
                var attack = enemy.AddComponent<EnemyAttack>();

                Assert.IsInstanceOf<EnemyMeleeAttackDelivery>(attack.AttackDeliverySource);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void EnemyAttack_DealsDamage_ToIDamageableTarget()
        {
            // Arrange
            var go = new GameObject("Enemy");
            var attack = go.AddComponent<EnemyAttack>();

            // These members do NOT exist yet. Test must fail until we implement them:
            attack.Damage = 3;

            var dummy = new DummyDamageable();

            // Act
            attack.DealDamage(dummy);

            // Assert
            Assert.AreEqual(3, dummy.DamageTaken,
                "EnemyAttack should deal its configured Damage to any IDamageable target.");
        }

        [Test]
        public void DealDamage_ReducesBarrierHealthByDamageAmount()
        {
            // Arrange
            var enemy = new GameObject("Enemy");
            enemy.AddComponent<EnemyController2D>();
            var attack = enemy.AddComponent<EnemyAttack>();
            attack.Damage = 2;

            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BarrierHealth>();
            barrier.MaxHealth = 5;
            barrier.CurrentHealth = 5;

            // Act
            attack.DealDamage(barrier);

            // Assert
            Assert.That(barrier.CurrentHealth, Is.EqualTo(3), "Barrier health should be reduced by EnemyAttack.Damage.");
            Assert.IsFalse(barrier.IsBroken, "Non-lethal damage should not mark the barrier as broken.");

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(barrierGo);
        }

        [Test]
        public void DealDamage_BreaksBarrier_AndDisablesColliderAndSprite()
        {
            // Arrange
            var enemy = new GameObject("Enemy");
            enemy.AddComponent<EnemyController2D>();
            var attack = enemy.AddComponent<EnemyAttack>();
            attack.Damage = 10;

            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BarrierHealth>();

            // Give the barrier visuals + collision so we can assert they get disabled
            var collider = barrierGo.AddComponent<BoxCollider2D>();
            var sprite = barrierGo.AddComponent<SpriteRenderer>();

            barrier.MaxHealth = 5;
            barrier.CurrentHealth = 5;

            // Act
            attack.DealDamage(barrier);

            // Assert: barrier state
            Assert.That(barrier.CurrentHealth, Is.EqualTo(0), "Barrier health should be zero after lethal damage.");
            Assert.IsTrue(barrier.IsBroken, "Barrier should be marked broken after lethal damage.");

            // Assert: collider & sprite disabled
            Assert.IsFalse(collider.enabled, "Barrier collider should be disabled when broken.");
            Assert.IsFalse(sprite.enabled, "Barrier sprite should be disabled when broken.");

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(barrierGo);
        }

        [Test]
        public void DealDamage_DoesNotApplyDamageWhileEnemyRooted()
        {
            var enemy = new GameObject("Enemy");
            enemy.AddComponent<EnemyController2D>();
            var rootReceiver = enemy.AddComponent<EnemyRootReceiver>();
            var attack = enemy.AddComponent<EnemyAttack>();
            attack.Damage = 3;

            var dummy = new DummyDamageable();

            try
            {
                rootReceiver.RootForSeconds(5f);

                attack.DealDamage(dummy);

                Assert.That(dummy.DamageTaken, Is.EqualTo(0), "Rooted trap-held enemies should not deal attack damage.");
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void AdvanceAttack_WhenParryCancelsReentrantly_ClearsClockAndSwingSnapshot()
        {
            var player = new GameObject("Player");
            var enemy = new GameObject("Enemy");
            try
            {
                player.tag = "Player";
                player.transform.position = new Vector2(0f, 0.25f);
                player.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
                var defense = player.AddComponent<PlayerDefenseController>();
                defense.Configure(0.15f, 0.15f, 120f, 0.6f);
                bool hitResolved = false;
                PlayerHitResult observed = default;
                defense.HitResolved += result =>
                {
                    hitResolved = true;
                    observed = result;
                };
                defense.SetDefensePressed(true);

                enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
                var controller = enemy.AddComponent<EnemyController2D>();
                controller.Debug_SetupRefs(player.transform);
                enemy.GetComponent<EnemyFacing>().InitializeAimDirection(Vector2.up);
                enemy.GetComponent<EnemyEngagement>().Debug_SetTuning(1f, 0f);
                enemy.GetComponent<EnemyLocomotion>()
                    .SetMovementState(EnemyController2D.State.HOLD);

                var attack = enemy.AddComponent<EnemyAttack>();
                var delivery = attack.AttackDeliverySource as EnemyMeleeAttackDelivery;
                delivery.Damage = 4;
                var stagger = enemy.AddComponent<EnemyStaggerReceiver>();
                stagger.Configure(true, 1f, attack);
                SetField(attack, "staggerReceiver", stagger);
                SetField(delivery, "staggerReceiver", stagger);
                attack.AttackDeliverySource = delivery;
                SetField(attack, "windupSeconds", 0.01f);
                attack.CooldownSeconds = 1f;
                InvokePrivate(attack, "Awake");

                Assert.That(controller.Target, Is.SameAs(player.transform));
                Assert.IsTrue(enemy.GetComponent<EnemyEngagement>()
                    .IsWithinEngagementDistance(player.transform));
                Assert.IsTrue(enemy.GetComponent<EnemyFacing>()
                    .IsAlignedWith(enemy.transform.position, player.transform));
                Assert.That(stagger.State, Is.EqualTo(EnemyStaggerState.Inactive));
                Assert.That(GetField<IEnemyAttackDelivery>(attack, "attackDelivery"), Is.SameAs(delivery));
                Assert.IsTrue(InvokePrivate<bool>(attack, "TryBeginAttack"));
                Assert.IsTrue(attack.IsAttackActive);

                InvokePrivate(attack, "AdvanceAttack", 0.5f);

                Assert.IsTrue(hitResolved, "The active clock should reach contextual melee delivery.");
                Assert.That(observed.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
                Assert.IsFalse(attack.IsAttackActive);
                Assert.IsNull(GetField<Transform>(attack, "lockedTarget"));
                Assert.IsFalse(GetField<bool>(attack, "impactDelivered"));
                Assert.That(stagger.State, Is.EqualTo(EnemyStaggerState.Staggered));
                Assert.That(player.GetComponent<Health>().Current, Is.EqualTo(10));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(player);
            }
        }

        private static T GetField<T>(object instance, string fieldName)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            return (T)field.GetValue(instance);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            field.SetValue(instance, value);
        }

        private static void InvokePrivate(object instance, string methodName, params object[] arguments)
        {
            MethodInfo method = instance.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Missing method {methodName}.");
            method.Invoke(instance, arguments);
        }

        private static T InvokePrivate<T>(object instance, string methodName, params object[] arguments)
        {
            MethodInfo method = instance.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Missing method {methodName}.");
            return (T)method.Invoke(instance, arguments);
        }
    }
}

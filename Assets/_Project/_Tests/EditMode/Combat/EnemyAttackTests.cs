using NUnit.Framework;
using UnityEngine;

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
    }
}

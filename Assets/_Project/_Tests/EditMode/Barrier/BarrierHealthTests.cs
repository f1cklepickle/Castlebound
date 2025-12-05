using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Gate
{
    public class BarrierHealthTests
    {
        [Test]
        public void Breaks_WhenHealthReachesZero()
        {
            var go = new GameObject("Gate");
            var health = go.AddComponent<BarrierHealth>();

            health.MaxHealth = 10;
            health.CurrentHealth = 10;

            health.TakeDamage(10);

            Assert.IsTrue(health.IsBroken, "Gate should be broken when health reaches zero.");
        }

        [Test]
        public void DisablesColliderAndSprite_WhenBroken()
        {
            var go = new GameObject("Barrier");
            var collider = go.AddComponent<BoxCollider2D>();
            var sprite = go.AddComponent<SpriteRenderer>();
            var health = go.AddComponent<BarrierHealth>();

            health.MaxHealth = 5;
            health.CurrentHealth = 5;
            health.TakeDamage(5);

            Assert.IsFalse(collider.enabled, "Broken barrier should disable its collider.");
            Assert.IsFalse(sprite.enabled, "Broken barrier should disable its sprite.");
        }

        [Test]
        public void TakeDamage_DoesNotReduceHealthBelowZero()
        {
            // Arrange
            var go = new GameObject("Barrier");
            var barrier = go.AddComponent<BarrierHealth>();

            barrier.MaxHealth = 3;
            barrier.CurrentHealth = 3;

            // Act: apply more damage than current health
            barrier.TakeDamage(10);

            // Assert: clamped to zero, flagged as broken
            Assert.That(barrier.CurrentHealth, Is.EqualTo(0), "Barrier health should clamp to zero when overkilled.");
            Assert.IsTrue(barrier.IsBroken, "Barrier should be marked broken when health reaches zero.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void TakeDamage_DoesNothing_WhenAlreadyBroken()
        {
            // Arrange
            var go = new GameObject("Barrier");
            var barrier = go.AddComponent<BarrierHealth>();

            barrier.MaxHealth = 2;
            barrier.CurrentHealth = 2;

            // First hit: break the barrier
            barrier.TakeDamage(2);

            Assert.IsTrue(barrier.IsBroken, "Precondition: barrier should be broken after lethal damage.");
            var healthAfterBreak = barrier.CurrentHealth;

            // Act: attempt to damage again after broken
            barrier.TakeDamage(1);

            // Assert: no further changes
            Assert.That(barrier.CurrentHealth, Is.EqualTo(healthAfterBreak), "Health should not change once barrier is broken.");
            Assert.IsTrue(barrier.IsBroken, "Barrier should remain broken after additional damage attempts.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Repair_RestoresHealth_AndEnablesColliderAndSprite()
        {
            // Arrange
            var go = new GameObject("Barrier");
            var barrier = go.AddComponent<BarrierHealth>();

            // Give the barrier visuals + collision so we can assert they change.
            var collider = go.AddComponent<BoxCollider2D>();
            var sprite = go.AddComponent<SpriteRenderer>();

            barrier.MaxHealth = 5;
            barrier.CurrentHealth = 5;

            // Break the barrier first.
            barrier.TakeDamage(barrier.MaxHealth);

            Assert.IsTrue(barrier.IsBroken, "Precondition: barrier should be broken after lethal damage.");
            Assert.That(barrier.CurrentHealth, Is.EqualTo(0), "Precondition: health should be zero after lethal damage.");
            Assert.IsFalse(collider.enabled, "Precondition: collider should be disabled when broken.");
            Assert.IsFalse(sprite.enabled, "Precondition: sprite should be disabled when broken.");

            // Act
            barrier.Repair();

            // Assert
            Assert.That(barrier.CurrentHealth, Is.EqualTo(barrier.MaxHealth), "Repair should restore health to MaxHealth.");
            Assert.IsFalse(barrier.IsBroken, "Repair should clear the broken flag.");
            Assert.IsTrue(collider.enabled, "Repair should re-enable the collider.");
            Assert.IsTrue(sprite.enabled, "Repair should re-enable the sprite.");

            Object.DestroyImmediate(go);
        }
    }
}

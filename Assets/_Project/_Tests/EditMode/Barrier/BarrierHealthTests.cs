using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace Castlebound.Tests.Gate
{
    public class BarrierHealthTests
    {
        [Test]
        public void TakeDamage_WhenBroken_DisablesOnlyConfiguredGateRenderer()
        {
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BoxCollider2D>();
            var health = barrier.AddComponent<BarrierHealth>();
            health.MaxHealth = 1;
            health.CurrentHealth = 1;

            var ground = CreateRenderer(barrier.transform, "Ground");
            var gate = CreateRenderer(barrier.transform, "Gate");
            var wall = CreateRenderer(barrier.transform, "Wall");
            var arch = CreateRenderer(barrier.transform, "Arch");

            var field = typeof(BarrierHealth).GetField("barrierGateRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(health, gate);

            health.TakeDamage(1);

            Assert.IsTrue(ground.enabled);
            Assert.IsFalse(gate.enabled);
            Assert.IsTrue(wall.enabled);
            Assert.IsTrue(arch.enabled);

            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void Repair_AfterBreak_RaisesRepairedEvent()
        {
            var barrier = new GameObject("Barrier");
            var health = barrier.AddComponent<BarrierHealth>();
            health.MaxHealth = 1;
            health.CurrentHealth = 1;
            var repairedCount = 0;
            health.OnRepaired += () => repairedCount++;

            health.TakeDamage(1);
            health.Repair();

            Assert.That(repairedCount, Is.EqualTo(1));
            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void Repair_WhenDamagedButNotBroken_RestoresHealthAndRaisesRepairedEvent()
        {
            var barrier = new GameObject("Barrier");
            var health = barrier.AddComponent<BarrierHealth>();
            health.MaxHealth = 10;
            health.CurrentHealth = 10;
            var repairedCount = 0;
            health.OnRepaired += () => repairedCount++;

            health.TakeDamage(4);
            var repaired = health.Repair();

            Assert.IsTrue(repaired);
            Assert.IsFalse(health.IsBroken);
            Assert.IsFalse(health.IsDamaged);
            Assert.IsFalse(health.CanRepair);
            Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
            Assert.That(repairedCount, Is.EqualTo(1));

            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void Repair_WhenFullHealth_DoesNotRaiseRepairedEvent()
        {
            var barrier = new GameObject("Barrier");
            var health = barrier.AddComponent<BarrierHealth>();
            health.MaxHealth = 10;
            health.CurrentHealth = 10;
            var repairedCount = 0;
            health.OnRepaired += () => repairedCount++;

            var repaired = health.Repair();

            Assert.IsFalse(repaired);
            Assert.IsFalse(health.IsDamaged);
            Assert.IsFalse(health.CanRepair);
            Assert.That(repairedCount, Is.EqualTo(0));

            Object.DestroyImmediate(barrier);
        }

        private static SpriteRenderer CreateRenderer(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.AddComponent<SpriteRenderer>();
        }

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

        [Test]
        public void OnBroken_FiresOncePerBreak()
        {
            var go = new GameObject("Barrier");
            var barrier = go.AddComponent<BarrierHealth>();

            barrier.MaxHealth = 2;
            barrier.CurrentHealth = 2;

            int brokenCount = 0;
            barrier.OnBroken += () => brokenCount++;

            barrier.TakeDamage(2);
            barrier.TakeDamage(1);

            Assert.That(brokenCount, Is.EqualTo(1), "OnBroken should fire once per break.");

            barrier.Repair();
            barrier.TakeDamage(2);

            Assert.That(brokenCount, Is.EqualTo(2), "OnBroken should fire again after repair and re-break.");

            Object.DestroyImmediate(go);
        }
    }
}

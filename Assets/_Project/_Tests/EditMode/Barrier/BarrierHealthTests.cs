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
    }
}

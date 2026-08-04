using System.Reflection;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerAttackSnapshotTests
    {
        [Test]
        public void ConfiguredSwingSnapshot_DrivesDamageAndResetsPerSwingDeduplication()
        {
            var hitboxObject = new GameObject("Hitbox");
            var enemy = new GameObject("Enemy");

            try
            {
                hitboxObject.AddComponent<BoxCollider2D>();
                var hitbox = hitboxObject.AddComponent<Hitbox>();
                enemy.tag = "Enemy";
                var enemyCollider = enemy.AddComponent<BoxCollider2D>();
                var health = enemy.AddComponent<Health>();
                health.ConfigureMaxHealth(10, refill: true);

                hitbox.ConfigureSwing(CreateSnapshot(3), default);
                hitbox.Activate();
                InvokeTryHit(hitbox, enemyCollider);
                Assert.That(health.Current, Is.EqualTo(7));

                hitbox.ConfigureSwing(CreateSnapshot(5), default);
                InvokeTryHit(hitbox, enemyCollider);
                Assert.That(health.Current, Is.EqualTo(2),
                    "A new swing snapshot should clear per-swing recipient deduplication.");
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(hitboxObject);
            }
        }

        private static CombatEquipmentSnapshot CreateSnapshot(int damage)
        {
            return new CombatEquipmentSnapshot(
                "test",
                damage,
                1f,
                0f,
                0f,
                CombatEquipmentCapability.MeleeDelivery,
                null,
                null,
                0f,
                0f,
                0f);
        }

        private static void InvokeTryHit(Hitbox hitbox, Collider2D target)
        {
            typeof(Hitbox)
                .GetMethod("TryHit", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(hitbox, new object[] { target });
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Inventory
{
    public class PotionConsumeTests
    {
        private class FixedTimeProvider : ITimeProvider
        {
            public float Time { get; private set; }

            public FixedTimeProvider(float time)
            {
                Time = time;
            }

            public void SetTime(float time)
            {
                Time = time;
            }
        }

        private class DummyHealable : IHealable
        {
            public int Healed { get; private set; }

            public void Heal(int amount)
            {
                Healed += amount;
            }
        }

        [Test]
        public void ConsumePotion_WithStock_HealsAndDecrementsStack()
        {
            var inventory = new InventoryState();
            inventory.TryAddPotion("potion_basic", 2);

            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.ItemId = "potion_basic";
            potion.HealAmount = 10;
            potion.CooldownSeconds = 3f;

            var resolver = new TestPotionResolver(potion);
            var time = new FixedTimeProvider(0f);
            var healable = new DummyHealable();
            var controller = new PotionConsumeController(resolver, time, healable);

            var result = controller.TryConsume(inventory);

            Assert.IsTrue(result);
            Assert.AreEqual(1, inventory.PotionCount);
            Assert.AreEqual(10, healable.Healed);

            Object.DestroyImmediate(potion);
        }

        [Test]
        public void ConsumePotion_WhenCooldownActive_IsBlocked()
        {
            var inventory = new InventoryState();
            inventory.TryAddPotion("potion_basic", 2);

            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.ItemId = "potion_basic";
            potion.HealAmount = 10;
            potion.CooldownSeconds = 3f;

            var resolver = new TestPotionResolver(potion);
            var time = new FixedTimeProvider(0f);
            var healable = new DummyHealable();
            var controller = new PotionConsumeController(resolver, time, healable);

            Assert.IsTrue(controller.TryConsume(inventory));

            time.SetTime(1f);
            var result = controller.TryConsume(inventory);

            Assert.IsFalse(result);
            Assert.AreEqual(1, inventory.PotionCount);
            Assert.AreEqual(10, healable.Healed);

            Object.DestroyImmediate(potion);
        }

        [Test]
        public void ConsumePotion_WhenNoStock_IsBlocked()
        {
            var inventory = new InventoryState();

            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.ItemId = "potion_basic";
            potion.HealAmount = 10;
            potion.CooldownSeconds = 3f;

            var resolver = new TestPotionResolver(potion);
            var time = new FixedTimeProvider(0f);
            var healable = new DummyHealable();
            var controller = new PotionConsumeController(resolver, time, healable);

            var result = controller.TryConsume(inventory);

            Assert.IsFalse(result);
            Assert.AreEqual(0, healable.Healed);

            Object.DestroyImmediate(potion);
        }

        [Test]
        public void Cooldown_Expires_AllowsNextUse()
        {
            var inventory = new InventoryState();
            inventory.TryAddPotion("potion_basic", 2);

            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.ItemId = "potion_basic";
            potion.HealAmount = 10;
            potion.CooldownSeconds = 3f;

            var resolver = new TestPotionResolver(potion);
            var time = new FixedTimeProvider(0f);
            var healable = new DummyHealable();
            var controller = new PotionConsumeController(resolver, time, healable);

            Assert.IsTrue(controller.TryConsume(inventory));

            time.SetTime(3.1f);
            var result = controller.TryConsume(inventory);

            Assert.IsTrue(result);
            Assert.AreEqual(0, inventory.PotionCount);
            Assert.AreEqual(20, healable.Healed);

            Object.DestroyImmediate(potion);
        }

        private sealed class TestPotionResolver : IPotionDefinitionResolver
        {
            private readonly PotionDefinition definition;

            public TestPotionResolver(PotionDefinition definition)
            {
                this.definition = definition;
            }

            public PotionDefinition Resolve(string potionId)
            {
                return definition != null && definition.ItemId == potionId ? definition : null;
            }
        }
    }
}

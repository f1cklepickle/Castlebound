using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Inventory
{
    public class PotionUseControllerTests
    {
        private class DummyHealable : MonoBehaviour, IHealable
        {
            public int Healed { get; private set; }

            public void Heal(int amount)
            {
                Healed += amount;
            }
        }

        [Test]
        public void TryConsume_ConsumesOnePotion_AndHeals()
        {
            var playerGo = new GameObject("Player");
            var inventory = playerGo.AddComponent<InventoryStateComponent>();
            inventory.State.TryAddPotion("potion_basic", 2);

            var healable = playerGo.AddComponent<DummyHealable>();

            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.ItemId = "potion_basic";
            potion.HealAmount = 10;
            potion.CooldownSeconds = 0f;

            var resolverGo = new GameObject("Resolver");
            var resolver = resolverGo.AddComponent<PotionDefinitionResolverComponent>();
            resolver.GetType()
                .GetField("definitions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(resolver, new[] { potion });

            var controller = playerGo.AddComponent<PotionUseController>();
            controller.SetResolverSource(resolver);
            controller.SetInventorySource(inventory);
            controller.SetHealTargetSource(healable);

            controller.TryConsume();

            Assert.AreEqual(1, inventory.State.PotionCount);
            Assert.AreEqual(10, healable.Healed);

            Object.DestroyImmediate(resolverGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(potion);
        }

        [Test]
        public void TryConsume_WithNoStock_DoesNothing()
        {
            var playerGo = new GameObject("Player");
            var inventory = playerGo.AddComponent<InventoryStateComponent>();

            var healable = playerGo.AddComponent<DummyHealable>();

            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.ItemId = "potion_basic";
            potion.HealAmount = 10;
            potion.CooldownSeconds = 0f;

            var resolverGo = new GameObject("Resolver");
            var resolver = resolverGo.AddComponent<PotionDefinitionResolverComponent>();
            resolver.GetType()
                .GetField("definitions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(resolver, new[] { potion });

            var controller = playerGo.AddComponent<PotionUseController>();
            controller.SetResolverSource(resolver);
            controller.SetInventorySource(inventory);
            controller.SetHealTargetSource(healable);

            var result = controller.TryConsume();

            Assert.IsFalse(result);
            Assert.AreEqual(0, inventory.State.PotionCount);
            Assert.AreEqual(0, healable.Healed);

            Object.DestroyImmediate(resolverGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(potion);
        }
    }
}

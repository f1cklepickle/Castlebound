using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Barrier
{
    public class BarrierUpgradeTests
    {
        [Test]
        public void GetMaxHealthForTier_UsesLinearScaling()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseMaxHealth = 10;
            config.MaxHealthPerTier = 2;

            Assert.That(config.GetMaxHealthForTier(0), Is.EqualTo(10), "Tier 0 should use the base value.");
            Assert.That(config.GetMaxHealthForTier(3), Is.EqualTo(16), "Tier 3 should scale linearly.");
        }

        [Test]
        public void GetMaxHealthForTier_UsesBalanceTableWhenAssigned()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();

            try
            {
                table.BaseMaxHealth = 30;
                table.MaxHealthPerTier = 7;
                station.Barrier = table;
                config.BalanceStation = station;
                config.BaseMaxHealth = 10;
                config.MaxHealthPerTier = 2;

                Assert.That(config.GetMaxHealthForTier(2), Is.EqualTo(44));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void GetUpgradeCost_ForHigherTier_IsGreaterThanBaseTier()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 20;
            config.CostPerTier = 10;

            Assert.That(config.GetUpgradeCostForTier(0), Is.LessThan(config.GetUpgradeCostForTier(1)),
                "Higher tiers should cost more than tier 0.");
        }

        [Test]
        public void GetUpgradeCostForTier_UsesBalanceTableWhenAssigned()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();

            try
            {
                table.BaseCost = 15;
                table.CostPerTier = 6;
                station.Barrier = table;
                config.BalanceStation = station;
                config.BaseCost = 20;
                config.CostPerTier = 10;

                Assert.That(config.GetUpgradeCostForTier(3), Is.EqualTo(33));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void TryPurchaseUpgrade_UsesBalanceTableCost_WhenAssigned()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();

            try
            {
                table.BaseCost = 15;
                table.CostPerTier = 5;
                station.Barrier = table;
                config.BalanceStation = station;
                config.BaseCost = 100;
                config.CostPerTier = 100;

                var state = new BarrierUpgradeState();
                var inventory = new InventoryState();
                inventory.AddGold(15);

                bool upgraded = BarrierUpgradeService.TryPurchaseUpgrade(state, config, inventory);

                Assert.IsTrue(upgraded, "Upgrade should use the balance table cost when assigned.");
                Assert.That(state.Tier, Is.EqualTo(1));
                Assert.That(inventory.Gold, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void TryPurchaseUpgrade_SpendsGoldAndIncrementsTier_WhenEnoughGold()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 20;
            config.CostPerTier = 10;

            var state = new BarrierUpgradeState();
            var inventory = new InventoryState();
            inventory.AddGold(25);

            bool upgraded = BarrierUpgradeService.TryPurchaseUpgrade(state, config, inventory);

            Assert.IsTrue(upgraded, "Upgrade should succeed when enough gold is available.");
            Assert.That(state.Tier, Is.EqualTo(1), "Successful upgrade should increment the tier.");
            Assert.That(inventory.Gold, Is.EqualTo(5), "Gold should be spent only on success.");
        }

        [Test]
        public void TryPurchaseUpgrade_DoesNotSpendGold_WhenInsufficient()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 20;
            config.CostPerTier = 10;

            var state = new BarrierUpgradeState();
            var inventory = new InventoryState();
            inventory.AddGold(10);

            bool upgraded = BarrierUpgradeService.TryPurchaseUpgrade(state, config, inventory);

            Assert.IsFalse(upgraded, "Upgrade should fail when gold is insufficient.");
            Assert.That(state.Tier, Is.EqualTo(0), "Tier should not change on failure.");
            Assert.That(inventory.Gold, Is.EqualTo(10), "Gold should not be spent on failure.");
        }
    }
}

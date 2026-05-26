using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Barrier;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Balance
{
    public class BarrierBalanceTableTests
    {
        private const string BalanceStationPath = "Assets/_Project/Balance/GameBalanceStation.asset";
        private const string BarrierBalanceTablePath = "Assets/_Project/Balance/BarrierBalanceTable.asset";
        private const string BarrierUpgradeConfigPath = "Assets/_Project/Barrier/BarrierUpgradeConfig.asset";

        [Test]
        public void Defaults_MirrorCurrentBarrierUpgradeConfigTuning()
        {
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();

            try
            {
                Assert.That(table.BaseMaxHealth, Is.EqualTo(10));
                Assert.That(table.MaxHealthPerTier, Is.EqualTo(2));
                Assert.That(table.BaseCost, Is.EqualTo(20));
                Assert.That(table.CostPerTier, Is.EqualTo(10));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ScalarProperties_ClampToSafeValues()
        {
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();

            try
            {
                table.BaseMaxHealth = -1;
                table.MaxHealthPerTier = -1;
                table.BaseCost = -1;
                table.CostPerTier = -1;

                Assert.That(table.BaseMaxHealth, Is.EqualTo(0));
                Assert.That(table.MaxHealthPerTier, Is.EqualTo(0));
                Assert.That(table.BaseCost, Is.EqualTo(0));
                Assert.That(table.CostPerTier, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void GetMaxHealthForTier_UsesLinearScalingAndClampsTier()
        {
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();

            try
            {
                table.BaseMaxHealth = 10;
                table.MaxHealthPerTier = 2;

                Assert.That(table.GetMaxHealthForTier(-1), Is.EqualTo(10));
                Assert.That(table.GetMaxHealthForTier(0), Is.EqualTo(10));
                Assert.That(table.GetMaxHealthForTier(3), Is.EqualTo(16));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void GetUpgradeCostForTier_UsesLinearScalingAndClampsTier()
        {
            var table = ScriptableObject.CreateInstance<BarrierBalanceTable>();

            try
            {
                table.BaseCost = 20;
                table.CostPerTier = 10;

                Assert.That(table.GetUpgradeCostForTier(-1), Is.EqualTo(20));
                Assert.That(table.GetUpgradeCostForTier(0), Is.EqualTo(20));
                Assert.That(table.GetUpgradeCostForTier(3), Is.EqualTo(50));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ProjectAssets_WireBarrierConfigThroughCentralBalanceStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<BarrierBalanceTable>(BarrierBalanceTablePath);
            var config = AssetDatabase.LoadAssetAtPath<BarrierUpgradeConfig>(BarrierUpgradeConfigPath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "BarrierBalanceTable asset must exist.");
            Assert.NotNull(config, "BarrierUpgradeConfig asset must exist.");
            Assert.AreSame(table, station.Barrier, "Central station should reference the authored barrier table.");
            Assert.AreSame(station, config.BalanceStation, "Barrier config should resolve through the central station.");
            Assert.That(table.BaseMaxHealth, Is.EqualTo(25), "Authored table should preserve current barrier config health tuning.");
            Assert.That(table.MaxHealthPerTier, Is.EqualTo(5), "Authored table should preserve current barrier config tier health tuning.");
            Assert.That(table.BaseCost, Is.EqualTo(10), "Authored table should preserve current barrier config cost tuning.");
            Assert.That(table.CostPerTier, Is.EqualTo(10), "Authored table should preserve current barrier config tier cost tuning.");
        }
    }
}

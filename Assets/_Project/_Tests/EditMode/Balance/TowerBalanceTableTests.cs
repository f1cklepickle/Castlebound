using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Balance
{
    public class TowerBalanceTableTests
    {
        private const string BalanceStationPath = "Assets/_Project/Balance/GameBalanceStation.asset";
        private const string TowerBalanceTablePath = "Assets/_Project/Balance/TowerBalanceTable.asset";
        private const string TowerBuildConfigPath = "Assets/_Project/Tower/TowerBuildConfig.asset";
        private const string TowerUpgradeConfigPath = "Assets/_Project/Tower/BaseTowerUpgradeConfig.asset";

        [Test]
        public void Defaults_MirrorCurrentTowerBuildAndRuntimeTuning()
        {
            var table = ScriptableObject.CreateInstance<TowerBalanceTable>();

            try
            {
                Assert.That(table.BuildCost, Is.EqualTo(50));
                Assert.That(table.BaseMaxHealth, Is.EqualTo(10));
                Assert.That(table.BaseDamage, Is.EqualTo(1));
                Assert.That(table.BaseUpgradeCost, Is.EqualTo(75));
                Assert.That(table.BaseCooldownSeconds, Is.EqualTo(1f).Within(0.001f));
                Assert.That(table.BaseMaxRange, Is.EqualTo(5f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ScalarProperties_ClampToSafeValues()
        {
            var table = ScriptableObject.CreateInstance<TowerBalanceTable>();

            try
            {
                table.BuildCost = -1;
                table.BaseMaxHealth = -1;
                table.BaseDamage = -1;
                table.BaseUpgradeCost = -1;
                table.BaseCooldownSeconds = -1f;
                table.BaseMaxRange = -1f;

                Assert.That(table.BuildCost, Is.EqualTo(0));
                Assert.That(table.BaseMaxHealth, Is.EqualTo(0));
                Assert.That(table.BaseDamage, Is.EqualTo(0));
                Assert.That(table.BaseUpgradeCost, Is.EqualTo(0));
                Assert.That(table.BaseCooldownSeconds, Is.EqualTo(0f));
                Assert.That(table.BaseMaxRange, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void UpgradeTracks_MirrorCurrentBaseTowerUpgradeConfig()
        {
            var table = ScriptableObject.CreateInstance<TowerBalanceTable>();

            try
            {
                AssertTrack(table.Damage, maxLevel: 5, baseValue: 1f, valuePerLevel: 1f, minValue: 0f, baseCost: 75, costPerLevel: 25);
                AssertTrack(table.FireRate, maxLevel: 4, baseValue: 1f, valuePerLevel: -0.1f, minValue: 0.45f, baseCost: 85, costPerLevel: 30);
                AssertTrack(table.Health, maxLevel: 3, baseValue: 10f, valuePerLevel: 3f, minValue: 1f, baseCost: 60, costPerLevel: 20);
                AssertTrack(table.Range, maxLevel: 3, baseValue: 5f, valuePerLevel: 0.5f, minValue: 0f, baseCost: 80, costPerLevel: 25);
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void GetTrack_ReturnsMatchingTrackConfig()
        {
            var table = ScriptableObject.CreateInstance<TowerBalanceTable>();

            try
            {
                Assert.AreSame(table.Damage, table.GetTrack(TowerUpgradeTrack.Damage));
                Assert.AreSame(table.FireRate, table.GetTrack(TowerUpgradeTrack.FireRate));
                Assert.AreSame(table.Health, table.GetTrack(TowerUpgradeTrack.Health));
                Assert.AreSame(table.Range, table.GetTrack(TowerUpgradeTrack.Range));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ResolvedValues_MirrorCurrentBaseTowerUpgradeConfigAtBaseLevel()
        {
            var table = ScriptableObject.CreateInstance<TowerBalanceTable>();
            var state = new TowerUpgradeState();

            try
            {
                Assert.That(table.GetDamage(state), Is.EqualTo(1));
                Assert.That(table.GetCooldownSeconds(state), Is.EqualTo(1f).Within(0.001f));
                Assert.That(table.GetMaxHealth(state), Is.EqualTo(10));
                Assert.That(table.GetMaxRange(state), Is.EqualTo(5f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ProjectAssets_WireTowerConfigsThroughCentralBalanceStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<TowerBalanceTable>(TowerBalanceTablePath);
            var buildConfig = AssetDatabase.LoadAssetAtPath<TowerBuildConfig>(TowerBuildConfigPath);
            var upgradeConfig = AssetDatabase.LoadAssetAtPath<TowerUpgradeConfig>(TowerUpgradeConfigPath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "TowerBalanceTable asset must exist.");
            Assert.NotNull(buildConfig, "TowerBuildConfig asset must exist.");
            Assert.NotNull(upgradeConfig, "BaseTowerUpgradeConfig asset must exist.");
            Assert.AreSame(table, station.Tower, "Central station should reference the authored tower table.");
            Assert.AreSame(station, buildConfig.BalanceStation, "Tower build config should resolve through the central station.");
            Assert.AreSame(station, upgradeConfig.BalanceStation, "Tower upgrade config should resolve through the central station.");
            Assert.NotNull(table.TowerPrefab, "Tower table should reference the buildable tower prefab.");
        }

        private static void AssertTrack(
            TowerUpgradeTrackConfig track,
            int maxLevel,
            float baseValue,
            float valuePerLevel,
            float minValue,
            int baseCost,
            int costPerLevel)
        {
            Assert.NotNull(track);
            Assert.IsTrue(track.Enabled);
            Assert.That(track.MaxLevel, Is.EqualTo(maxLevel));
            Assert.That(track.BaseValue, Is.EqualTo(baseValue).Within(0.001f));
            Assert.That(track.ValuePerLevel, Is.EqualTo(valuePerLevel).Within(0.001f));
            Assert.That(track.MinValue, Is.EqualTo(minValue).Within(0.001f));
            Assert.That(track.BaseCost, Is.EqualTo(baseCost));
            Assert.That(track.CostPerLevel, Is.EqualTo(costPerLevel));
        }
    }
}

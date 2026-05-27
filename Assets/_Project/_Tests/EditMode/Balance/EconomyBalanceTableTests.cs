using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Balance
{
    public class EconomyBalanceTableTests
    {
        private const string BalanceStationPath = "Assets/_Project/Balance/GameBalanceStation.asset";
        private const string EconomyBalanceTablePath = "Assets/_Project/Balance/EconomyBalanceTable.asset";

        [Test]
        public void Defaults_MirrorCurrentInventoryStartTuning()
        {
            var table = ScriptableObject.CreateInstance<EconomyBalanceTable>();

            try
            {
                Assert.That(table.StartingGold, Is.EqualTo(0));
                Assert.That(table.StartingXp, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ScalarProperties_ClampToSafeValues()
        {
            var table = ScriptableObject.CreateInstance<EconomyBalanceTable>();

            try
            {
                table.StartingGold = -1;
                table.StartingXp = -1;

                Assert.That(table.StartingGold, Is.EqualTo(0));
                Assert.That(table.StartingXp, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void InventoryStateComponent_UsesEconomyTableStartingCurrencyWhenAssigned()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<EconomyBalanceTable>();
            var gameObject = new GameObject("Inventory");
            var inventory = gameObject.AddComponent<InventoryStateComponent>();

            try
            {
                table.StartingGold = 7;
                table.StartingXp = 3;
                station.Economy = table;
                inventory.BalanceStation = station;
                inventory.StartingGold = 99;
                inventory.StartingXp = 99;

                Assert.That(inventory.State.Gold, Is.EqualTo(7));
                Assert.That(inventory.State.Xp, Is.EqualTo(3));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void InventoryStateComponent_FallsBackToSerializedStartingCurrencyWithoutTable()
        {
            var gameObject = new GameObject("Inventory");
            var inventory = gameObject.AddComponent<InventoryStateComponent>();

            try
            {
                inventory.StartingGold = 5;
                inventory.StartingXp = 2;

                Assert.That(inventory.State.Gold, Is.EqualTo(5));
                Assert.That(inventory.State.Xp, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ProjectAssets_WireEconomyTableThroughCentralBalanceStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<EconomyBalanceTable>(EconomyBalanceTablePath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "EconomyBalanceTable asset must exist.");
            Assert.AreSame(table, station.Economy, "Central station should reference the authored economy table.");
            Assert.That(table.StartingGold, Is.EqualTo(0), "Authored table should preserve current starting gold tuning.");
            Assert.That(table.StartingXp, Is.EqualTo(0), "Authored table should preserve current starting XP tuning.");
        }
    }
}

using Castlebound.Gameplay.Balance;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Balance
{
    public class PlayerBalanceTableTests
    {
        private const string BalanceStationPath = "Assets/_Project/Balance/GameBalanceStation.asset";
        private const string PlayerBalanceTablePath = "Assets/_Project/Balance/PlayerBalanceTable.asset";

        [Test]
        public void Defaults_MirrorCurrentPlayerRuntimeTuning()
        {
            var table = ScriptableObject.CreateInstance<PlayerBalanceTable>();

            try
            {
                Assert.That(table.BaseMaxHealth, Is.EqualTo(200));
                Assert.That(table.BaseMoveSpeed, Is.EqualTo(12f).Within(0.001f));
                Assert.That(table.BaseRepairRange, Is.EqualTo(2f).Within(0.001f));
                Assert.That(table.BaseRepairCooldownSeconds, Is.EqualTo(1f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ScalarProperties_ClampToSafeValues()
        {
            var table = ScriptableObject.CreateInstance<PlayerBalanceTable>();

            try
            {
                table.BaseMaxHealth = -1;
                table.BaseMoveSpeed = -1f;
                table.BaseRepairRange = -1f;
                table.BaseRepairCooldownSeconds = -1f;

                Assert.That(table.BaseMaxHealth, Is.EqualTo(0));
                Assert.That(table.BaseMoveSpeed, Is.EqualTo(0f));
                Assert.That(table.BaseRepairRange, Is.EqualTo(0f));
                Assert.That(table.BaseRepairCooldownSeconds, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ProjectAssets_WirePlayerTableThroughCentralBalanceStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<PlayerBalanceTable>(PlayerBalanceTablePath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "PlayerBalanceTable asset must exist.");
            Assert.AreSame(table, station.Player, "Central station should reference the authored player table.");
            Assert.That(table.BaseMaxHealth, Is.EqualTo(200));
            Assert.That(table.BaseMoveSpeed, Is.EqualTo(12f).Within(0.001f));
            Assert.That(table.BaseRepairRange, Is.EqualTo(2f).Within(0.001f));
            Assert.That(table.BaseRepairCooldownSeconds, Is.EqualTo(1f).Within(0.001f));
        }
    }
}

using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Loot;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Balance
{
    public class EnemyBalanceTableTests
    {
        private const string BalanceStationPath = "Assets/_Project/Balance/GameBalanceStation.asset";
        private const string EnemyBalanceTablePath = "Assets/_Project/Balance/EnemyBalanceTable.asset";
        private const string GruntLootProfilePath = "Assets/_Project/Items/LootTables/EnemyLootProfile_Grunt.asset";

        [Test]
        public void Defaults_MirrorCurrentGruntRuntimeTuning()
        {
            var table = ScriptableObject.CreateInstance<EnemyBalanceTable>();

            try
            {
                var grunt = table.Find("grunt");

                Assert.NotNull(grunt);
                Assert.That(grunt.MaxHealth, Is.EqualTo(10));
                Assert.That(grunt.MoveSpeed, Is.EqualTo(8f).Within(0.001f));
                Assert.That(grunt.AttackDamage, Is.EqualTo(1));
                Assert.That(grunt.AttackCooldownSeconds, Is.EqualTo(0.8f).Within(0.001f));
                Assert.That(grunt.XpReward, Is.EqualTo(5));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void EntryProperties_ClampToSafeValues()
        {
            var entry = new EnemyBalanceEntry();

            entry.MaxHealth = -1;
            entry.MoveSpeed = -1f;
            entry.AttackDamage = -1;
            entry.AttackCooldownSeconds = -1f;
            entry.XpReward = -1;

            Assert.That(entry.MaxHealth, Is.EqualTo(0));
            Assert.That(entry.MoveSpeed, Is.EqualTo(0f));
            Assert.That(entry.AttackDamage, Is.EqualTo(0));
            Assert.That(entry.AttackCooldownSeconds, Is.EqualTo(0f));
            Assert.That(entry.XpReward, Is.EqualTo(0));
        }

        [Test]
        public void Find_ReturnsMatchingEnemyTypeOnly()
        {
            var table = ScriptableObject.CreateInstance<EnemyBalanceTable>();
            try
            {
                var grunt = new EnemyBalanceEntry { EnemyTypeId = "grunt", MaxHealth = 10 };
                table.Enemies = new[] { grunt };

                Assert.AreSame(grunt, table.Find("grunt"));
                Assert.IsNull(table.Find("missing"));
                Assert.IsNull(table.Find(""));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ProjectAssets_WireGruntEnemyBalanceThroughCentralStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<EnemyBalanceTable>(EnemyBalanceTablePath);
            var profile = AssetDatabase.LoadAssetAtPath<EnemyLootProfile>(GruntLootProfilePath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "EnemyBalanceTable asset must exist.");
            Assert.NotNull(profile, "Grunt loot profile asset must exist.");
            Assert.AreSame(table, station.Enemy, "Central station should reference the authored enemy table.");

            var grunt = table.Find("grunt");
            Assert.NotNull(grunt, "The only current enemy type should be authored as grunt.");
            Assert.AreSame(profile, grunt.LootProfile, "Grunt should use the authored grunt loot profile.");
            Assert.That(profile.LootTables, Is.Not.Null);
            Assert.That(profile.LootTables.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(profile.GlobalMaxTables, Is.EqualTo(6));
        }
    }
}

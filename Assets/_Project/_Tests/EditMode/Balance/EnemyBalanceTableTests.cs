using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Loot;
using Castlebound.Gameplay.Spawning;
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
        private const string GoblinLootProfilePath = "Assets/_Project/Items/LootTables/EnemyLootProfile_Goblin.asset";

        [Test]
        public void Defaults_MirrorCurrentGoblinAndLurkerRuntimeTuning()
        {
            var table = ScriptableObject.CreateInstance<EnemyBalanceTable>();

            try
            {
                var melee = table.Find(EnemyArchetypeIds.GoblinMelee);

                Assert.NotNull(melee);
                Assert.That(melee.MaxHealth, Is.EqualTo(10));
                Assert.That(melee.MoveSpeed, Is.EqualTo(8f).Within(0.001f));
                Assert.That(melee.AttackDamage, Is.EqualTo(1));
                Assert.That(melee.AttackCooldownSeconds, Is.EqualTo(0.8f).Within(0.001f));
                Assert.That(melee.XpReward, Is.EqualTo(5));

                var ranged = table.Find(EnemyArchetypeIds.GoblinRanged);
                Assert.NotNull(ranged);
                Assert.That(ranged.MaxHealth, Is.EqualTo(10));
                Assert.That(ranged.MoveSpeed, Is.EqualTo(8f).Within(0.001f));

                var lurker = table.Find(EnemyArchetypeIds.Lurker);
                Assert.NotNull(lurker);
                Assert.That(lurker.MaxHealth, Is.EqualTo(35));
                Assert.That(lurker.MoveSpeed, Is.EqualTo(3f).Within(0.001f));
                Assert.That(lurker.MoveSpeed, Is.LessThan(melee.MoveSpeed));
                Assert.That(lurker.AttackDamage, Is.EqualTo(1));
                Assert.That(lurker.AttackCooldownSeconds, Is.EqualTo(0.8f).Within(0.001f));
                Assert.That(lurker.XpReward, Is.EqualTo(5));
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
                var melee = new EnemyBalanceEntry { EnemyTypeId = EnemyArchetypeIds.GoblinMelee, MaxHealth = 10 };
                table.Enemies = new[] { melee };

                Assert.AreSame(melee, table.Find(EnemyArchetypeIds.GoblinMelee));
                Assert.AreSame(melee, table.Find(EnemyArchetypeIds.LegacyGrunt));
                Assert.IsNull(table.Find("missing"));
                Assert.IsNull(table.Find(""));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ProjectAssets_WireGoblinEnemyBalanceThroughCentralStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<EnemyBalanceTable>(EnemyBalanceTablePath);
            var profile = AssetDatabase.LoadAssetAtPath<EnemyLootProfile>(GoblinLootProfilePath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "EnemyBalanceTable asset must exist.");
            Assert.NotNull(profile, "Goblin loot profile asset must exist.");
            Assert.AreSame(table, station.Enemy, "Central station should reference the authored enemy table.");

            var melee = table.Find(EnemyArchetypeIds.GoblinMelee);
            Assert.NotNull(melee, "Melee goblin balance must be authored.");
            Assert.AreSame(profile, melee.LootProfile, "Melee goblins should use the authored goblin loot profile.");

            var ranged = table.Find(EnemyArchetypeIds.GoblinRanged);
            Assert.NotNull(ranged, "Ranged goblin balance must be authored.");
            Assert.AreSame(profile, ranged.LootProfile, "Ranged goblins should use the authored goblin loot profile.");

            var lurker = table.Find(EnemyArchetypeIds.Lurker);
            Assert.NotNull(lurker, "Lurker enemy balance must be authored.");
            Assert.That(lurker.MaxHealth, Is.EqualTo(35));
            Assert.That(lurker.MoveSpeed, Is.EqualTo(3f).Within(0.001f));
            Assert.That(lurker.MoveSpeed, Is.LessThan(melee.MoveSpeed));
            Assert.AreSame(profile, lurker.LootProfile, "Lurker should reuse the current enemy loot profile until it has bespoke drops.");

            Assert.That(profile.LootTables, Is.Not.Null);
            Assert.That(profile.LootTables.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(profile.GlobalMaxTables, Is.EqualTo(6));
        }
    }
}

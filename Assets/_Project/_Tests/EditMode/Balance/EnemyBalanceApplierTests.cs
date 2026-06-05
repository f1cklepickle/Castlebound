using System.Reflection;
using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Loot;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Balance
{
    public class EnemyBalanceApplierTests
    {
        [Test]
        public void Apply_ConfiguresEnemyStatsAndLootFromBalanceEntry()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<EnemyBalanceTable>();
            var lootProfile = ScriptableObject.CreateInstance<EnemyLootProfile>();
            var lootTable = ScriptableObject.CreateInstance<LootTable>();
            var pickupPrefab = new GameObject("PickupPrefab").AddComponent<ItemPickupComponent>();
            var enemy = new GameObject("Enemy");

            try
            {
                var controller = enemy.AddComponent<EnemyController2D>();
                var attack = enemy.AddComponent<EnemyAttack>();
                var health = enemy.AddComponent<Health>();
                var dropper = enemy.AddComponent<LootDropper>();
                var applier = enemy.AddComponent<EnemyBalanceApplier>();

                var mapping = new LootDropper.LootTableMapping(lootTable, pickupPrefab, 2, 3);
                lootProfile.LootTables = new[] { mapping };
                lootProfile.GlobalMaxTables = 4;

                table.Enemies = new[]
                {
                    new EnemyBalanceEntry
                    {
                        EnemyTypeId = "grunt",
                        MaxHealth = 23,
                        MoveSpeed = 6.5f,
                        AttackDamage = 4,
                        AttackCooldownSeconds = 1.25f,
                        XpReward = 9,
                        LootProfile = lootProfile
                    }
                };
                station.Enemy = table;
                applier.BalanceStation = station;
                applier.EnemyTypeId = "grunt";

                Assert.IsTrue(applier.Apply(3));

                Assert.That(health.Max, Is.EqualTo(23));
                Assert.That(health.Current, Is.EqualTo(23));
                Assert.That(controller.Speed, Is.EqualTo(6.5f).Within(0.001f));
                Assert.That(attack.Damage, Is.EqualTo(4));
                Assert.That(attack.CooldownSeconds, Is.EqualTo(1.25f).Within(0.001f));
                Assert.That(GetPrivate<int>(dropper, "xpAmount"), Is.EqualTo(9));
                Assert.That(GetPrivate<int>(dropper, "globalMaxTables"), Is.EqualTo(4));
                Assert.AreSame(lootTable, GetPrivate<LootDropper.LootTableMapping[]>(dropper, "lootTables")[0].Table);
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(pickupPrefab.gameObject);
                Object.DestroyImmediate(lootTable);
                Object.DestroyImmediate(lootProfile);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void Apply_WhenEnemyTypeMissing_LeavesPrefabAuthoredValues()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<EnemyBalanceTable>();
            var enemy = new GameObject("Enemy");

            try
            {
                var controller = enemy.AddComponent<EnemyController2D>();
                var attack = enemy.AddComponent<EnemyAttack>();
                var health = enemy.AddComponent<Health>();
                var applier = enemy.AddComponent<EnemyBalanceApplier>();

                health.ConfigureMaxHealth(10, true);
                controller.Speed = 8f;
                attack.Damage = 1;
                attack.CooldownSeconds = 0.8f;
                table.Enemies = new[] { new EnemyBalanceEntry { EnemyTypeId = "grunt", MaxHealth = 99 } };
                station.Enemy = table;
                applier.BalanceStation = station;
                applier.EnemyTypeId = "missing";

                Assert.IsFalse(applier.Apply(1));

                Assert.That(health.Max, Is.EqualTo(10));
                Assert.That(controller.Speed, Is.EqualTo(8f).Within(0.001f));
                Assert.That(attack.Damage, Is.EqualTo(1));
                Assert.That(attack.CooldownSeconds, Is.EqualTo(0.8f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        private static T GetPrivate<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            return (T)field.GetValue(target);
        }
    }
}

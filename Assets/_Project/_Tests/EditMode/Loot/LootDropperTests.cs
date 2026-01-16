using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using Random = System.Random;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Loot
{
    public class LootDropperTests
    {
        private sealed class FixedRandom : Random
        {
            public override double NextDouble()
            {
                return 0.0;
            }

            public override int Next(int minValue, int maxValue)
            {
                return minValue;
            }
        }

        [Test]
        public void PreRoll_ThenSpawnDrops_UsesLootTableEntries()
        {
            var lootTable = ScriptableObject.CreateInstance<LootTable>();
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_a";
            lootTable.Entries = new LootEntry[]
            {
                new LootEntry(weapon, 1, 1, 1f)
            };

            var pickupPrefab = new GameObject("PickupPrefab");
            var pickupComponent = pickupPrefab.AddComponent<ItemPickupComponent>();

            var dropperGo = new GameObject("Dropper");
            var dropper = dropperGo.AddComponent<LootDropper>();
            dropper.SetLootTable(lootTable);
            dropper.SetPickupPrefab(pickupComponent);

            dropper.PreRoll(new Random(123), 1, 1);
            var spawned = dropper.SpawnDrops(Vector3.zero);

            Assert.AreEqual(1, spawned.Length);
            Assert.AreEqual(ItemPickupKind.Weapon, spawned[0].Kind);
            Assert.AreEqual(weapon, spawned[0].ItemDefinition);
            Assert.AreEqual(1, spawned[0].Amount);

            Object.DestroyImmediate(dropperGo);
            Object.DestroyImmediate(pickupPrefab);
            Object.DestroyImmediate(lootTable);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void OnDied_GrantsXp_ToPlayerInventory()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            var inventoryComponent = player.AddComponent<InventoryStateComponent>();

            var dropperGo = new GameObject("Dropper");
            var health = dropperGo.AddComponent<Health>();
            var dropper = dropperGo.AddComponent<LootDropper>();
            dropper.SetXpAmount(5);

            typeof(LootDropper).GetMethod("HandleDied", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(dropper, null);

            Assert.AreEqual(5, inventoryComponent.State.Xp);

            Object.DestroyImmediate(dropperGo);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void PreRoll_GlobalCap_PrefersRarerTables()
        {
            var commonTable = ScriptableObject.CreateInstance<LootTable>();
            var rareTable = ScriptableObject.CreateInstance<LootTable>();
            commonTable.DropChance = 1f;
            rareTable.DropChance = 0.1f;

            var commonItem = ScriptableObject.CreateInstance<PotionDefinition>();
            commonItem.ItemId = "potion_common";
            var rareItem = ScriptableObject.CreateInstance<WeaponDefinition>();
            rareItem.ItemId = "weapon_rare";

            commonTable.Entries = new LootEntry[]
            {
                new LootEntry(commonItem, 1, 1, 1f)
            };
            rareTable.Entries = new LootEntry[]
            {
                new LootEntry(rareItem, 1, 1, 1f)
            };

            var commonPrefabGo = new GameObject("CommonPrefab");
            var rarePrefabGo = new GameObject("RarePrefab");
            var commonPrefab = commonPrefabGo.AddComponent<ItemPickupComponent>();
            var rarePrefab = rarePrefabGo.AddComponent<ItemPickupComponent>();

            var dropperGo = new GameObject("Dropper");
            var dropper = dropperGo.AddComponent<LootDropper>();
            var mappings = new LootDropper.LootTableMapping[]
            {
                new LootDropper.LootTableMapping(commonTable, commonPrefab, 1, 0),
                new LootDropper.LootTableMapping(rareTable, rarePrefab, 1, 0)
            };

            typeof(LootDropper).GetField("lootTables", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(dropper, mappings);

            dropper.PreRoll(new FixedRandom(), 1, 1);
            var spawned = dropper.SpawnDrops(Vector3.zero);

            Assert.AreEqual(1, spawned.Length);
            Assert.AreEqual(rareItem, spawned[0].ItemDefinition);

            Object.DestroyImmediate(dropperGo);
            Object.DestroyImmediate(commonPrefabGo);
            Object.DestroyImmediate(rarePrefabGo);
            Object.DestroyImmediate(commonTable);
            Object.DestroyImmediate(rareTable);
            Object.DestroyImmediate(commonItem);
            Object.DestroyImmediate(rareItem);
        }

        [Test]
        public void SpawnDrops_SplitsGoldIntoMultiplePickups_WithCap()
        {
            var goldItem = ScriptableObject.CreateInstance<ItemDefinition>();
            goldItem.ItemId = "gold_test";
            var goldTable = ScriptableObject.CreateInstance<LootTable>();
            goldTable.Entries = new[]
            {
                new LootEntry(goldItem, 1, 1, 1f)
            };

            var pickupPrefab = new GameObject("GoldPickupPrefab");
            var pickupComponent = pickupPrefab.AddComponent<ItemPickupComponent>();

            var dropperGo = new GameObject("Dropper");
            var dropper = dropperGo.AddComponent<LootDropper>();
            var results = new[]
            {
                new LootRollResult(goldItem, 25)
            };

            typeof(LootDropper).GetField("cachedResults", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(dropper, results);

            var mappings = new LootDropper.LootTableMapping[]
            {
                new LootDropper.LootTableMapping(goldTable, pickupComponent, 10, 10)
            };

            typeof(LootDropper).GetField("lootTables", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(dropper, mappings);

            var spawned = dropper.SpawnDrops(Vector3.zero);

            int total = 0;
            for (int i = 0; i < spawned.Length; i++)
            {
                total += spawned[i].Amount;
            }

            Assert.AreEqual(10, spawned.Length);
            Assert.AreEqual(25, total);

            Object.DestroyImmediate(dropperGo);
            Object.DestroyImmediate(pickupPrefab);
            Object.DestroyImmediate(goldItem);
            Object.DestroyImmediate(goldTable);
        }
    }
}

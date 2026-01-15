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

            Assert.AreEqual(10, spawned.Length);
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
    }
}

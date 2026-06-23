using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using Castlebound.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Loot
{
    public class PickupMagnetMotionTests
    {
        [Test]
        public void Step_AttractsEligiblePickupThroughCollider()
        {
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();
            economy.PickupMagnetRange = 3f;
            economy.PickupMagnetSpeed = 4f;
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            station.Economy = economy;
            var player = new GameObject("Player");
            player.AddComponent<InventoryStateComponent>();
            var field = player.AddComponent<PickupMagnetField>();
            field.BalanceStation = station;
            var wall = new GameObject("Wall");
            wall.transform.position = Vector3.right;
            wall.AddComponent<BoxCollider2D>();
            var pickupObject = CreateGoldPickup(new Vector3(2f, 0f));
            var motion = pickupObject.AddComponent<PickupMagnetMotion>();
            motion.MagnetField = field;

            motion.Step(0.25f);

            Assert.AreEqual(1f, pickupObject.transform.position.x, 0.001f);

            Object.DestroyImmediate(pickupObject);
            Object.DestroyImmediate(wall);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(station);
            Object.DestroyImmediate(economy);
        }

        [Test]
        public void Step_WaitsForSpillMotionToFinish()
        {
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();
            economy.PickupMagnetRange = 10f;
            economy.PickupMagnetSpeed = 4f;
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            station.Economy = economy;
            var player = new GameObject("Player");
            player.AddComponent<InventoryStateComponent>();
            var field = player.AddComponent<PickupMagnetField>();
            field.BalanceStation = station;
            var pickupObject = CreateGoldPickup(new Vector3(2f, 0f));
            var spill = pickupObject.AddComponent<LootSpillMotion>();
            spill.Initialize(new Vector3(3f, 0f), 1f);
            var motion = pickupObject.AddComponent<PickupMagnetMotion>();
            motion.MagnetField = field;

            motion.Step(0.25f);

            Assert.AreEqual(2f, pickupObject.transform.position.x, 0.001f);

            spill.Step(1f);
            motion.Step(0.25f);

            Assert.AreEqual(2f, pickupObject.transform.position.x, 0.001f);

            Object.DestroyImmediate(pickupObject);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(station);
            Object.DestroyImmediate(economy);
        }

        [Test]
        public void Step_DoesNotAttractWeapon_WhenInventoryIsFull()
        {
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();
            economy.PickupMagnetRange = 10f;
            economy.PickupMagnetSpeed = 4f;
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            station.Economy = economy;
            var player = new GameObject("Player");
            var inventory = player.AddComponent<InventoryStateComponent>();
            inventory.State.AddWeapon("weapon_a");
            inventory.State.AddWeapon("weapon_b");
            var field = player.AddComponent<PickupMagnetField>();
            field.BalanceStation = station;
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.ItemId = "weapon_c";
            var pickupObject = new GameObject("Weapon Pickup");
            pickupObject.transform.position = new Vector3(2f, 0f);
            var pickup = pickupObject.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = definition;
            var motion = pickupObject.AddComponent<PickupMagnetMotion>();
            motion.MagnetField = field;

            motion.Step(0.25f);

            Assert.AreEqual(2f, pickupObject.transform.position.x, 0.001f);

            Object.DestroyImmediate(pickupObject);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(station);
            Object.DestroyImmediate(economy);
        }

        private static GameObject CreateGoldPickup(Vector3 position)
        {
            var pickupObject = new GameObject("Gold Pickup");
            pickupObject.transform.position = position;
            var pickup = pickupObject.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Gold;
            pickup.Amount = 1;
            return pickupObject;
        }
    }
}

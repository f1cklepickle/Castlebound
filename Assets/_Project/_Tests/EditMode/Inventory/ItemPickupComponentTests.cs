using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Inventory
{
    public class ItemPickupComponentTests
    {
        [Test]
        public void AutoPickup_ConsumesPickup_WhenInventoryAcceptsWeapon()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();
            var pickupCollider = playerGo.AddComponent<Castlebound.Gameplay.Player.PlayerPickupCollider>();

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_basic";

            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = weapon;

            var result = pickup.TryAutoPickup(inventoryComponent.State);

            Assert.IsTrue(result);
            Assert.IsTrue(pickup.IsConsumed);
            Assert.AreEqual("weapon_basic", inventoryComponent.State.GetWeaponId(0));

            Object.DestroyImmediate(pickupGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void AutoPickup_DoesNotConsume_WhenWeaponSlotsFull()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();
            var pickupCollider = playerGo.AddComponent<Castlebound.Gameplay.Player.PlayerPickupCollider>();
            inventoryComponent.State.AddWeapon("weapon_a");
            inventoryComponent.State.AddWeapon("weapon_b");
            inventoryComponent.State.SetActiveWeaponSlot(0);

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_c";

            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = weapon;

            var result = pickup.TryAutoPickup(inventoryComponent.State);

            Assert.IsFalse(result);
            Assert.IsFalse(pickup.IsConsumed);
            Assert.AreEqual("weapon_a", inventoryComponent.State.GetWeaponId(0));
            Assert.AreEqual("weapon_b", inventoryComponent.State.GetWeaponId(1));

            Object.DestroyImmediate(pickupGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void AutoPickup_ConsumesWeaponIntoBackpack_WhenWeaponSlotsFull()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();
            var backpackComponent = playerGo.AddComponent<BackpackInventoryStateComponent>();
            var pickupCollider = playerGo.AddComponent<Castlebound.Gameplay.Player.PlayerPickupCollider>();
            inventoryComponent.State.AddWeapon("weapon_a");
            inventoryComponent.State.AddWeapon("weapon_b");
            backpackComponent.MaxItemCount = 1;

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_c";

            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = weapon;
            var context = new InventoryPickupContext(inventoryComponent.State, backpackComponent.State);

            var result = pickup.TryAutoPickup(context);

            Assert.IsTrue(result);
            Assert.IsTrue(pickup.IsConsumed);
            Assert.AreEqual("weapon_a", inventoryComponent.State.GetWeaponId(0));
            Assert.AreEqual("weapon_b", inventoryComponent.State.GetWeaponId(1));
            Assert.That(backpackComponent.State.GetCount("weapon_c"), Is.EqualTo(1));

            Object.DestroyImmediate(pickupGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void AutoPickup_DoesNotConsumeWeapon_WhenInventoryAndBackpackAreFull()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();
            var backpackComponent = playerGo.AddComponent<BackpackInventoryStateComponent>();
            var pickupCollider = playerGo.AddComponent<Castlebound.Gameplay.Player.PlayerPickupCollider>();
            inventoryComponent.State.AddWeapon("weapon_a");
            inventoryComponent.State.AddWeapon("weapon_b");
            backpackComponent.MaxItemCount = 1;
            backpackComponent.State.AddItem("weapon_backpack", 1);

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_c";

            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = weapon;
            var context = new InventoryPickupContext(inventoryComponent.State, backpackComponent.State);

            var result = pickup.TryAutoPickup(context);

            Assert.IsFalse(result);
            Assert.IsFalse(pickup.IsConsumed);
            Assert.That(backpackComponent.State.GetCount("weapon_c"), Is.EqualTo(0));

            Object.DestroyImmediate(pickupGo);
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void AutoPickup_RespectsPickupDelay()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();
            var pickupCollider = playerGo.AddComponent<Castlebound.Gameplay.Player.PlayerPickupCollider>();

            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Gold;
            pickup.Amount = 1;

            pickup.SetPickupDelay(1f);

            var blocked = pickup.TryAutoPickup(inventoryComponent.State);

            Assert.IsFalse(blocked);
            Assert.IsFalse(pickup.IsConsumed);

            pickup.SetPickupDelay(0f);

            var allowed = pickup.TryAutoPickup(inventoryComponent.State);

            Assert.IsTrue(allowed);
            Assert.IsTrue(pickup.IsConsumed);

            Object.DestroyImmediate(pickupGo);
            Object.DestroyImmediate(playerGo);
        }

        [Test]
        public void CanAutoPickup_ReusesCachedPickup_WithoutAllocating()
        {
            var inventory = new InventoryState();
            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Gold;
            pickup.Amount = 1;
            pickup.CanAutoPickup(inventory);

            long before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                pickup.CanAutoPickup(inventory);
            }
            long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.AreEqual(0L, allocated);

            Object.DestroyImmediate(pickupGo);
        }

        [Test]
        public void CachedPickup_Invalidates_WhenConfigurationChanges()
        {
            var inventory = new InventoryState();
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_basic";
            var pickupGo = new GameObject("Pickup");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Gold;
            pickup.Amount = 1;

            Assert.IsTrue(pickup.CanAutoPickup(inventory));

            pickup.Amount = 0;
            Assert.IsFalse(pickup.CanAutoPickup(inventory));

            pickup.Amount = 1;
            pickup.Kind = ItemPickupKind.Weapon;
            Assert.IsFalse(pickup.CanAutoPickup(inventory));

            pickup.ItemDefinition = weapon;
            Assert.IsTrue(pickup.CanAutoPickup(inventory));

            pickup.ItemDefinition = null;
            Assert.IsFalse(pickup.CanAutoPickup(inventory));

            Object.DestroyImmediate(pickupGo);
            Object.DestroyImmediate(weapon);
        }
    }
}

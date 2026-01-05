using NUnit.Framework;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Inventory
{
    public class ItemPickupTests
    {
        [Test]
        public void WeaponAutoPickup_WhenSlotEmpty_AddsWeapon()
        {
            var inventory = new InventoryState();
            var pickup = ItemPickup.Weapon("weapon_basic");

            var result = pickup.TryAutoPickup(inventory);

            Assert.IsTrue(result);
            Assert.AreEqual("weapon_basic", inventory.GetWeaponId(0));
        }

        [Test]
        public void WeaponAutoPickup_WhenBothSlotsFull_IsBlocked()
        {
            var inventory = new InventoryState();
            inventory.AddWeapon("weapon_a");
            inventory.AddWeapon("weapon_b");
            inventory.SetActiveWeaponSlot(0);

            var pickup = ItemPickup.Weapon("weapon_c");

            var result = pickup.TryAutoPickup(inventory);

            Assert.IsFalse(result);
            Assert.AreEqual("weapon_a", inventory.GetWeaponId(0));
            Assert.AreEqual("weapon_b", inventory.GetWeaponId(1));
        }

        [Test]
        public void WeaponManualPickup_WhenSlotsFull_SwapsActive()
        {
            var inventory = new InventoryState();
            inventory.AddWeapon("weapon_a");
            inventory.AddWeapon("weapon_b");
            inventory.SetActiveWeaponSlot(0);

            var pickup = ItemPickup.Weapon("weapon_c");

            var result = pickup.TryManualPickup(inventory);

            Assert.IsTrue(result);
            Assert.AreEqual("weapon_c", inventory.GetWeaponId(0));
            Assert.AreEqual("weapon_b", inventory.GetWeaponId(1));
        }

        [Test]
        public void PotionAutoPickup_WhenEmptyOrSameType_AddsToStack()
        {
            var inventory = new InventoryState();
            var pickupA = ItemPickup.Potion("potion_a", 2);
            var pickupB = ItemPickup.Potion("potion_a", 1);

            var first = pickupA.TryAutoPickup(inventory);
            var second = pickupB.TryAutoPickup(inventory);

            Assert.IsTrue(first);
            Assert.IsTrue(second);
            Assert.AreEqual("potion_a", inventory.PotionId);
            Assert.AreEqual(3, inventory.PotionCount);
        }

        [Test]
        public void PotionAutoPickup_WhenDifferentType_IsBlocked()
        {
            var inventory = new InventoryState();
            inventory.TryAddPotion("potion_a", 1);

            var pickup = ItemPickup.Potion("potion_b", 1);

            var result = pickup.TryAutoPickup(inventory);

            Assert.IsFalse(result);
            Assert.AreEqual("potion_a", inventory.PotionId);
            Assert.AreEqual(1, inventory.PotionCount);
        }

        [Test]
        public void CurrencyAutoPickup_AddsGoldAndXp()
        {
            var inventory = new InventoryState();
            var gold = ItemPickup.Gold(5);
            var xp = ItemPickup.Xp(3);

            var goldResult = gold.TryAutoPickup(inventory);
            var xpResult = xp.TryAutoPickup(inventory);

            Assert.IsTrue(goldResult);
            Assert.IsTrue(xpResult);
            Assert.AreEqual(5, inventory.Gold);
            Assert.AreEqual(3, inventory.Xp);
        }
    }
}

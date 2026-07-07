using Castlebound.Gameplay.Inventory;
using NUnit.Framework;

namespace Castlebound.Tests.Inventory
{
    public class BackpackWeaponEquipControllerTests
    {
        [Test]
        public void TryEquip_BackpackWeaponToMainSlot_DisplacesExistingWeaponToBackpack()
        {
            var activeInventory = new InventoryState();
            var backpack = new BackpackInventoryState(maxItemCount: 2);
            activeInventory.AddWeapon("weapon_sword");
            backpack.AddItem("weapon_dagger", 1);

            var result = BackpackWeaponEquipController.TryEquip(activeInventory, backpack, "weapon_dagger", 0);

            Assert.IsTrue(result);
            Assert.That(activeInventory.GetWeaponId(0), Is.EqualTo("weapon_dagger"));
            Assert.That(backpack.GetCount("weapon_dagger"), Is.EqualTo(0));
            Assert.That(backpack.GetCount("weapon_sword"), Is.EqualTo(1));
        }

        [Test]
        public void TryEquip_InvalidSlot_DoesNotMutateBackpackOrActiveInventory()
        {
            var activeInventory = new InventoryState();
            var backpack = new BackpackInventoryState(maxItemCount: 2);
            activeInventory.AddWeapon("weapon_sword");
            backpack.AddItem("weapon_dagger", 1);

            var result = BackpackWeaponEquipController.TryEquip(activeInventory, backpack, "weapon_dagger", 2);

            Assert.IsFalse(result);
            Assert.That(activeInventory.GetWeaponId(0), Is.EqualTo("weapon_sword"));
            Assert.That(backpack.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void TryEquip_RepeatedSwapsBetweenBackpackAndMainSlot_RemainsAllowed()
        {
            var activeInventory = new InventoryState();
            var backpack = new BackpackInventoryState(maxItemCount: 2);
            activeInventory.AddWeapon("weapon_sword");
            backpack.AddItem("weapon_dagger", 1);

            Assert.IsTrue(BackpackWeaponEquipController.TryEquip(activeInventory, backpack, "weapon_dagger", 0));
            Assert.IsTrue(BackpackWeaponEquipController.TryEquip(activeInventory, backpack, "weapon_sword", 0));
            Assert.IsTrue(BackpackWeaponEquipController.TryEquip(activeInventory, backpack, "weapon_dagger", 0));

            Assert.That(activeInventory.GetWeaponId(0), Is.EqualTo("weapon_dagger"));
            Assert.That(backpack.GetCount("weapon_sword"), Is.EqualTo(1));
            Assert.That(backpack.GetCount("weapon_dagger"), Is.EqualTo(0));
        }
    }
}

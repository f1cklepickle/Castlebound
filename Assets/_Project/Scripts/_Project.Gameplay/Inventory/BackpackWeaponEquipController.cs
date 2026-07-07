using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class BackpackWeaponEquipController : MonoBehaviour
    {
        [SerializeField] private InventoryStateComponent activeInventorySource;
        [SerializeField] private BackpackInventoryStateComponent backpackSource;

        public void SetActiveInventorySource(InventoryStateComponent source)
        {
            activeInventorySource = source;
        }

        public void SetBackpackSource(BackpackInventoryStateComponent source)
        {
            backpackSource = source;
        }

        public bool TryEquip(string weaponId, int slotIndex)
        {
            InventoryState activeInventory = activeInventorySource != null ? activeInventorySource.State : null;
            BackpackInventoryState backpack = backpackSource != null ? backpackSource.State : null;
            return TryEquip(activeInventory, backpack, weaponId, slotIndex);
        }

        public static bool TryEquip(InventoryState activeInventory, BackpackInventoryState backpack, string weaponId, int slotIndex)
        {
            if (activeInventory == null || backpack == null || string.IsNullOrWhiteSpace(weaponId))
            {
                return false;
            }

            if (slotIndex < 0 || slotIndex >= InventoryState.WeaponSlotCount || backpack.GetCount(weaponId) <= 0)
            {
                return false;
            }

            if (!backpack.TryRemoveItem(weaponId, 1))
            {
                return false;
            }

            if (!activeInventory.TrySetWeaponAtSlot(slotIndex, weaponId, out string displacedWeaponId))
            {
                backpack.AddItem(weaponId, 1);
                return false;
            }

            if (string.IsNullOrEmpty(displacedWeaponId))
            {
                return true;
            }

            if (backpack.AddItem(displacedWeaponId, 1))
            {
                return true;
            }

            activeInventory.TrySetWeaponAtSlot(slotIndex, displacedWeaponId, out _);
            backpack.AddItem(weaponId, 1);
            return false;
        }
    }
}

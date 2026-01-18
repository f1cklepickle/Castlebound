using System;

namespace Castlebound.Gameplay.Inventory
{
    [Flags]
    public enum InventoryChangeFlags
    {
        None = 0,
        Weapons = 1 << 0,
        Potions = 1 << 1,
        Currency = 1 << 2
    }

    public class InventoryState
    {
        public const int WeaponSlotCount = 2;

        private readonly string[] weaponIds = new string[WeaponSlotCount];

        public int ActiveWeaponSlotIndex { get; private set; }
        public string PotionId { get; private set; }
        public int PotionCount { get; private set; }
        public int Gold { get; private set; }
        public int Xp { get; private set; }

        public event Action<InventoryChangeFlags> OnInventoryChanged;

        public string GetWeaponId(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return null;
            }

            return weaponIds[slotIndex];
        }

        public bool AddWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                return false;
            }

            int emptySlot = FindEmptyWeaponSlot();
            if (emptySlot >= 0)
            {
                weaponIds[emptySlot] = weaponId;
                RaiseChanged(InventoryChangeFlags.Weapons);
                return true;
            }

            int activeIndex = ClampActiveWeaponSlotIndex();
            weaponIds[activeIndex] = weaponId;
            RaiseChanged(InventoryChangeFlags.Weapons);
            return true;
        }

        public bool SetActiveWeaponSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex) || ActiveWeaponSlotIndex == slotIndex)
            {
                return false;
            }

            ActiveWeaponSlotIndex = slotIndex;
            RaiseChanged(InventoryChangeFlags.Weapons);
            return true;
        }

        public bool RemoveWeaponAtSlot(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex) || weaponIds[slotIndex] == null)
            {
                return false;
            }

            weaponIds[slotIndex] = null;
            RaiseChanged(InventoryChangeFlags.Weapons);
            return true;
        }

        public bool TryAddPotion(string potionId, int amount)
        {
            if (string.IsNullOrEmpty(potionId) || amount <= 0)
            {
                return false;
            }

            if (PotionCount == 0)
            {
                PotionId = potionId;
                PotionCount = amount;
                RaiseChanged(InventoryChangeFlags.Potions);
                return true;
            }

            if (!string.Equals(PotionId, potionId, StringComparison.Ordinal))
            {
                return false;
            }

            PotionCount += amount;
            RaiseChanged(InventoryChangeFlags.Potions);
            return true;
        }

        public bool TryConsumePotion(int amount)
        {
            if (amount <= 0 || PotionCount < amount)
            {
                return false;
            }

            PotionCount -= amount;
            if (PotionCount == 0)
            {
                PotionId = null;
            }

            RaiseChanged(InventoryChangeFlags.Potions);
            return true;
        }

        public bool AddGold(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            Gold += amount;
            RaiseChanged(InventoryChangeFlags.Currency);
            return true;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            RaiseChanged(InventoryChangeFlags.Currency);
            return true;
        }

        public bool AddXp(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            Xp += amount;
            RaiseChanged(InventoryChangeFlags.Currency);
            return true;
        }

        private int FindEmptyWeaponSlot()
        {
            for (int i = 0; i < weaponIds.Length; i++)
            {
                if (weaponIds[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < weaponIds.Length;
        }

        private int ClampActiveWeaponSlotIndex()
        {
            if (!IsValidSlotIndex(ActiveWeaponSlotIndex))
            {
                ActiveWeaponSlotIndex = 0;
            }

            return ActiveWeaponSlotIndex;
        }

        private void RaiseChanged(InventoryChangeFlags flags)
        {
            OnInventoryChanged?.Invoke(flags);
        }
    }
}

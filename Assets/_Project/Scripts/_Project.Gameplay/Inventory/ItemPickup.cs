using System;

namespace Castlebound.Gameplay.Inventory
{
    public enum ItemPickupKind
    {
        Weapon,
        Potion,
        Gold,
        Xp
    }

    public class ItemPickup
    {
        public ItemPickupKind Kind { get; }
        public string ItemId { get; }
        public int Amount { get; }

        public ItemPickup(ItemPickupKind kind, string itemId, int amount)
        {
            Kind = kind;
            ItemId = itemId;
            Amount = amount;
        }

        public static ItemPickup Weapon(string weaponId)
        {
            return new ItemPickup(ItemPickupKind.Weapon, weaponId, 1);
        }

        public static ItemPickup Potion(string potionId, int amount)
        {
            return new ItemPickup(ItemPickupKind.Potion, potionId, amount);
        }

        public static ItemPickup Gold(int amount)
        {
            return new ItemPickup(ItemPickupKind.Gold, null, amount);
        }

        public static ItemPickup Xp(int amount)
        {
            return new ItemPickup(ItemPickupKind.Xp, null, amount);
        }

        public bool CanAutoPickup(InventoryState inventory)
        {
            if (inventory == null)
            {
                return false;
            }

            switch (Kind)
            {
                case ItemPickupKind.Weapon:
                    return !string.IsNullOrWhiteSpace(ItemId) && HasEmptyWeaponSlot(inventory);
                case ItemPickupKind.Potion:
                    if (string.IsNullOrWhiteSpace(ItemId) || Amount <= 0)
                    {
                        return false;
                    }

                    return inventory.PotionCount == 0 ||
                        string.Equals(inventory.PotionId, ItemId, StringComparison.Ordinal);
                case ItemPickupKind.Gold:
                case ItemPickupKind.Xp:
                    return Amount > 0;
                default:
                    return false;
            }
        }

        public bool TryAutoPickup(InventoryState inventory)
        {
            if (!CanAutoPickup(inventory))
            {
                return false;
            }

            return Apply(inventory);
        }

        public bool TryManualPickup(InventoryState inventory)
        {
            if (inventory == null)
            {
                return false;
            }

            return Apply(inventory);
        }

        private bool Apply(InventoryState inventory)
        {
            switch (Kind)
            {
                case ItemPickupKind.Weapon:
                    return inventory.AddWeapon(ItemId);
                case ItemPickupKind.Potion:
                    return inventory.TryAddPotion(ItemId, Amount);
                case ItemPickupKind.Gold:
                    return inventory.AddGold(Amount);
                case ItemPickupKind.Xp:
                    return inventory.AddXp(Amount);
                default:
                    return false;
            }
        }

        private static bool HasEmptyWeaponSlot(InventoryState inventory)
        {
            for (int i = 0; i < InventoryState.WeaponSlotCount; i++)
            {
                if (inventory.GetWeaponId(i) == null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

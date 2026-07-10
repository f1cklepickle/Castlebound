using System;
using System.Collections.Generic;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Castle
{
    public static class CastleShopPurchaseService
    {
        public static CastleShopPurchaseResult TryPurchase(
            IReadOnlyList<CastleShopOffer> offers,
            string itemId,
            bool isShopAvailable,
            InventoryState activeInventory,
            BackpackInventoryState backpack)
        {
            if (!isShopAvailable)
            {
                return CastleShopPurchaseResult.Unavailable;
            }

            if (!TryFindOffer(offers, itemId, out var offer))
            {
                return CastleShopPurchaseResult.InvalidOffer;
            }

            if (activeInventory == null)
            {
                return CastleShopPurchaseResult.Unavailable;
            }

            if (!CanAcceptOffer(offer, activeInventory, backpack))
            {
                return CastleShopPurchaseResult.InventoryFull;
            }

            if (offer.Price > 0 && !activeInventory.TrySpendGold(offer.Price))
            {
                return CastleShopPurchaseResult.InsufficientGold;
            }

            if (ApplyOffer(offer, activeInventory, backpack))
            {
                return CastleShopPurchaseResult.Success;
            }

            if (offer.Price > 0)
            {
                activeInventory.AddGold(offer.Price);
            }

            return CastleShopPurchaseResult.InventoryFull;
        }

        private static bool TryFindOffer(IReadOnlyList<CastleShopOffer> offers, string itemId, out CastleShopOffer offer)
        {
            if (offers != null && !string.IsNullOrWhiteSpace(itemId))
            {
                for (int i = 0; i < offers.Count; i++)
                {
                    if (string.Equals(offers[i].ItemId, itemId, StringComparison.Ordinal))
                    {
                        offer = offers[i];
                        return true;
                    }
                }
            }

            offer = default(CastleShopOffer);
            return false;
        }

        private static bool CanAcceptOffer(CastleShopOffer offer, InventoryState activeInventory, BackpackInventoryState backpack)
        {
            switch (offer.ItemId)
            {
                case "weapon_sword":
                case "weapon_iron_club":
                    return backpack != null && backpack.CanAddItem(offer.ItemId, 1);
                case "potion_basic":
                    return activeInventory.PotionCount == 0 ||
                        string.Equals(activeInventory.PotionId, offer.ItemId, StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        private static bool ApplyOffer(CastleShopOffer offer, InventoryState activeInventory, BackpackInventoryState backpack)
        {
            switch (offer.ItemId)
            {
                case "weapon_sword":
                case "weapon_iron_club":
                    return backpack != null && backpack.AddItem(offer.ItemId, 1);
                case "potion_basic":
                    return activeInventory.TryAddPotion(offer.ItemId, 1);
                default:
                    return false;
            }
        }
    }
}

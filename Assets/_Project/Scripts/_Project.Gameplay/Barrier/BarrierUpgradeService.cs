using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Barrier
{
    public static class BarrierUpgradeService
    {
        public static bool TryPurchaseUpgrade(
            BarrierUpgradeState state,
            BarrierUpgradeConfig config,
            InventoryState inventory)
        {
            if (state == null || config == null || inventory == null)
            {
                return false;
            }

            int cost = config.GetUpgradeCostForTier(state.Tier);
            if (!inventory.TrySpendGold(cost))
            {
                return false;
            }

            state.IncrementTier();
            return true;
        }
    }
}

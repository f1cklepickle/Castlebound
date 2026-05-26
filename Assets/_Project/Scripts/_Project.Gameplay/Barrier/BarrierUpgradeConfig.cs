using Castlebound.Gameplay.Balance;
using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    [CreateAssetMenu(menuName = "Castlebound/Barrier/Barrier Upgrade Config")]
    public class BarrierUpgradeConfig : ScriptableObject
    {
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private int baseMaxHealth = 10;
        [SerializeField] private int maxHealthPerTier = 2;
        [SerializeField] private int baseCost = 20;
        [SerializeField] private int costPerTier = 10;

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public int BaseMaxHealth
        {
            get => ActiveBarrierTable != null ? ActiveBarrierTable.BaseMaxHealth : baseMaxHealth;
            set => baseMaxHealth = Mathf.Max(0, value);
        }

        public int MaxHealthPerTier
        {
            get => ActiveBarrierTable != null ? ActiveBarrierTable.MaxHealthPerTier : maxHealthPerTier;
            set => maxHealthPerTier = Mathf.Max(0, value);
        }

        public int BaseCost
        {
            get => ActiveBarrierTable != null ? ActiveBarrierTable.BaseCost : baseCost;
            set => baseCost = Mathf.Max(0, value);
        }

        public int CostPerTier
        {
            get => ActiveBarrierTable != null ? ActiveBarrierTable.CostPerTier : costPerTier;
            set => costPerTier = Mathf.Max(0, value);
        }

        public int GetMaxHealthForTier(int tier)
        {
            if (ActiveBarrierTable != null)
            {
                return ActiveBarrierTable.GetMaxHealthForTier(tier);
            }

            int safeTier = Mathf.Max(0, tier);
            return BaseMaxHealth + MaxHealthPerTier * safeTier;
        }

        public int GetUpgradeCostForTier(int tier)
        {
            if (ActiveBarrierTable != null)
            {
                return ActiveBarrierTable.GetUpgradeCostForTier(tier);
            }

            int safeTier = Mathf.Max(0, tier);
            return BaseCost + CostPerTier * safeTier;
        }

        private BarrierBalanceTable ActiveBarrierTable => balanceStation != null ? balanceStation.Barrier : null;
    }
}

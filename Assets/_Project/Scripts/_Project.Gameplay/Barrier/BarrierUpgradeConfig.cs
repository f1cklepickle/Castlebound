using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    [CreateAssetMenu(menuName = "Castlebound/Barrier/Barrier Upgrade Config")]
    public class BarrierUpgradeConfig : ScriptableObject
    {
        [SerializeField] private int baseMaxHealth = 10;
        [SerializeField] private int maxHealthPerTier = 2;
        [SerializeField] private int baseCost = 20;
        [SerializeField] private int costPerTier = 10;

        public int BaseMaxHealth
        {
            get => baseMaxHealth;
            set => baseMaxHealth = Mathf.Max(0, value);
        }

        public int MaxHealthPerTier
        {
            get => maxHealthPerTier;
            set => maxHealthPerTier = Mathf.Max(0, value);
        }

        public int BaseCost
        {
            get => baseCost;
            set => baseCost = Mathf.Max(0, value);
        }

        public int CostPerTier
        {
            get => costPerTier;
            set => costPerTier = Mathf.Max(0, value);
        }

        public int GetMaxHealthForTier(int tier)
        {
            int safeTier = Mathf.Max(0, tier);
            return BaseMaxHealth + MaxHealthPerTier * safeTier;
        }

        public int GetUpgradeCostForTier(int tier)
        {
            int safeTier = Mathf.Max(0, tier);
            return BaseCost + CostPerTier * safeTier;
        }
    }
}

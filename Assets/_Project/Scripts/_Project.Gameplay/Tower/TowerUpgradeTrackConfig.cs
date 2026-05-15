using System;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [Serializable]
    public class TowerUpgradeTrackConfig
    {
        [SerializeField] private bool enabled;
        [SerializeField] private int maxLevel = 3;
        [SerializeField] private float baseValue;
        [SerializeField] private float valuePerLevel;
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue = 9999f;
        [SerializeField] private int baseCost = 50;
        [SerializeField] private int costPerLevel = 25;

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public int MaxLevel
        {
            get => maxLevel;
            set => maxLevel = Mathf.Max(0, value);
        }

        public float BaseValue
        {
            get => baseValue;
            set => baseValue = value;
        }

        public float ValuePerLevel
        {
            get => valuePerLevel;
            set => valuePerLevel = value;
        }

        public float MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
                maxValue = Mathf.Max(minValue, maxValue);
            }
        }

        public float MaxValue
        {
            get => maxValue;
            set => maxValue = Mathf.Max(minValue, value);
        }

        public int BaseCost
        {
            get => baseCost;
            set => baseCost = Mathf.Max(0, value);
        }

        public int CostPerLevel
        {
            get => costPerLevel;
            set => costPerLevel = Mathf.Max(0, value);
        }

        public bool CanUpgrade(int currentLevel)
        {
            return enabled && currentLevel >= 0 && currentLevel < maxLevel;
        }

        public int GetCostForLevel(int currentLevel)
        {
            int safeLevel = Mathf.Max(0, currentLevel);
            return baseCost + costPerLevel * safeLevel;
        }

        public float GetValueForLevel(int level)
        {
            int safeLevel = Mathf.Clamp(level, 0, maxLevel);
            return Mathf.Clamp(baseValue + valuePerLevel * safeLevel, minValue, maxValue);
        }

        public void Normalize()
        {
            maxLevel = Mathf.Max(0, maxLevel);
            maxValue = Mathf.Max(minValue, maxValue);
            baseCost = Mathf.Max(0, baseCost);
            costPerLevel = Mathf.Max(0, costPerLevel);
        }
    }
}

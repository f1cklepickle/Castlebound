using System;
using Castlebound.Gameplay.Inventory;
using UnityEngine;

namespace Castlebound.Gameplay.Loot
{
    [Serializable]
    public struct LootEntry
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int minAmount;
        [SerializeField] private int maxAmount;
        [SerializeField] private float weight;

        public LootEntry(ItemDefinition item, int minAmount, int maxAmount, float weight)
        {
            this.item = item;
            this.minAmount = minAmount;
            this.maxAmount = maxAmount;
            this.weight = weight;
        }

        public ItemDefinition Item => item;
        public int MinAmount => minAmount;
        public int MaxAmount => maxAmount;
        public float Weight => weight;
    }
}

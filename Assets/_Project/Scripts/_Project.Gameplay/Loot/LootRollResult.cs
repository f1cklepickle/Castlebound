using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Loot
{
    public readonly struct LootRollResult
    {
        public LootRollResult(ItemDefinition item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public ItemDefinition Item { get; }
        public int Amount { get; }
        public bool IsEmpty => Item == null || Amount <= 0;
    }
}

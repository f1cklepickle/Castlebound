using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Loot
{
    public readonly struct LootSpawnRequest
    {
        public LootSpawnRequest(ItemPickupComponent prefab, ItemPickupKind kind, ItemDefinition item, int amount)
        {
            Prefab = prefab;
            Kind = kind;
            Item = item;
            Amount = amount;
        }

        public ItemPickupComponent Prefab { get; }
        public ItemPickupKind Kind { get; }
        public ItemDefinition Item { get; }
        public int Amount { get; }
    }
}

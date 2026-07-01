namespace Castlebound.Gameplay.Inventory
{
    public sealed class CastleInventoryEntry
    {
        public CastleInventoryEntry(string itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        public string ItemId { get; }
        public int Count { get; }
    }
}

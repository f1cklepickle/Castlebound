namespace Castlebound.Gameplay.Castle
{
    public readonly struct CastleShopOffer
    {
        public CastleShopOffer(string itemId, string displayName, int price)
        {
            ItemId = itemId;
            DisplayName = displayName;
            Price = price < 0 ? 0 : price;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
        public int Price { get; }
    }
}

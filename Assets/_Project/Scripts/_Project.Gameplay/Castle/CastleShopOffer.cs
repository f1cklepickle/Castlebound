namespace Castlebound.Gameplay.Castle
{
    public readonly struct CastleShopOffer
    {
        public CastleShopOffer(string itemId, string displayName)
        {
            ItemId = itemId;
            DisplayName = displayName;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
    }
}

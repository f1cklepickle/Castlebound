using System.Collections.Generic;

namespace Castlebound.Gameplay.Castle
{
    public static class CastleShopCatalog
    {
        private static readonly CastleShopOffer[] defaultOffers =
        {
            new CastleShopOffer("weapon_sword", "Sword", 250),
            new CastleShopOffer("weapon_iron_club", "Iron Club", 250),
            new CastleShopOffer("potion_basic", "Health Potion", 50)
        };

        public static IReadOnlyList<CastleShopOffer> DefaultOffers => defaultOffers;
    }
}

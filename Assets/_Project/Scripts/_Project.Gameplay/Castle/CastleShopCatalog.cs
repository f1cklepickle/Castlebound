using System.Collections.Generic;

namespace Castlebound.Gameplay.Castle
{
    public static class CastleShopCatalog
    {
        private static readonly CastleShopOffer[] defaultOffers =
        {
            new CastleShopOffer("weapon_sword", "Sword"),
            new CastleShopOffer("weapon_iron_club", "Iron Club"),
            new CastleShopOffer("potion_basic", "Health Potion")
        };

        public static IReadOnlyList<CastleShopOffer> DefaultOffers => defaultOffers;
    }
}

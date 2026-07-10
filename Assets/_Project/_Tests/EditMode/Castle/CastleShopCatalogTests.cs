using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using System.Linq;

namespace Castlebound.Tests.Castle
{
    public class CastleShopCatalogTests
    {
        [Test]
        public void DefaultOffers_ContainOnlySwordIronClubAndHealthPotion()
        {
            var offers = CastleShopCatalog.DefaultOffers;

            Assert.That(offers.Count, Is.EqualTo(3));
            Assert.That(offers.Select(offer => offer.ItemId), Is.EqualTo(new[]
            {
                "weapon_sword",
                "weapon_iron_club",
                "potion_basic"
            }));
            Assert.That(offers.Select(offer => offer.DisplayName), Is.EqualTo(new[]
            {
                "Sword",
                "Iron Club",
                "Health Potion"
            }));
            Assert.That(offers.Select(offer => offer.Price), Is.EqualTo(new[]
            {
                250,
                250,
                50
            }));
        }
    }
}

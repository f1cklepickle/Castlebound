using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;

namespace Castlebound.Tests.Castle
{
    public class CastleShopPurchaseServiceTests
    {
        [Test]
        public void TryPurchase_Sword_SpendsGoldAndRoutesToBackpack()
        {
            var inventory = CreateInventory(300);
            var backpack = new BackpackInventoryState();

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "weapon_sword",
                true,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.Success));
            Assert.That(inventory.Gold, Is.EqualTo(50));
            Assert.That(backpack.GetCount("weapon_sword"), Is.EqualTo(1));
            Assert.That(inventory.GetWeaponId(0), Is.Null);
        }

        [Test]
        public void TryPurchase_IronClub_RoutesToBackpack()
        {
            var inventory = CreateInventory(250);
            var backpack = new BackpackInventoryState();

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "weapon_iron_club",
                true,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.Success));
            Assert.That(inventory.Gold, Is.EqualTo(0));
            Assert.That(backpack.GetCount("weapon_iron_club"), Is.EqualTo(1));
        }

        [Test]
        public void TryPurchase_HealthPotion_SpendsGoldAndRoutesToActivePotionInventory()
        {
            var inventory = CreateInventory(75);
            var backpack = new BackpackInventoryState();

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "potion_basic",
                true,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.Success));
            Assert.That(inventory.Gold, Is.EqualTo(25));
            Assert.That(inventory.PotionId, Is.EqualTo("potion_basic"));
            Assert.That(inventory.PotionCount, Is.EqualTo(1));
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
        }

        [Test]
        public void TryPurchase_FailsWithoutShopAccess()
        {
            var inventory = CreateInventory(300);
            var backpack = new BackpackInventoryState();

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "weapon_sword",
                false,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.Unavailable));
            Assert.That(inventory.Gold, Is.EqualTo(300));
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
        }

        [Test]
        public void TryPurchase_FailsWithInsufficientGoldWithoutMutatingInventory()
        {
            var inventory = CreateInventory(249);
            var backpack = new BackpackInventoryState();

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "weapon_sword",
                true,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.InsufficientGold));
            Assert.That(inventory.Gold, Is.EqualTo(249));
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
        }

        [Test]
        public void TryPurchase_FailsForUnknownOffer()
        {
            var inventory = CreateInventory(300);
            var backpack = new BackpackInventoryState();

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "weapon_missing",
                true,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.InvalidOffer));
            Assert.That(inventory.Gold, Is.EqualTo(300));
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
        }

        [Test]
        public void TryPurchase_FailsWhenBackpackIsFullWithoutSpendingGold()
        {
            var inventory = CreateInventory(300);
            var backpack = new BackpackInventoryState(maxItemCount: 0);

            var result = CastleShopPurchaseService.TryPurchase(
                CastleShopCatalog.DefaultOffers,
                "weapon_sword",
                true,
                inventory,
                backpack);

            Assert.That(result, Is.EqualTo(CastleShopPurchaseResult.InventoryFull));
            Assert.That(inventory.Gold, Is.EqualTo(300));
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
        }

        private static InventoryState CreateInventory(int gold)
        {
            var inventory = new InventoryState();
            inventory.AddGold(gold);
            return inventory;
        }
    }
}

using System.Collections;
using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using Castlebound.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.Loot
{
    public class PickupMagnetSweepPlayTests
    {
        [UnityTest]
        public IEnumerator OverflowWeaponPickup_GoesToBackpack_WhenActiveWeaponSlotsAreFull()
        {
            var player = new GameObject("Player");
            var inventory = player.AddComponent<InventoryStateComponent>();
            var backpack = player.AddComponent<BackpackInventoryStateComponent>();
            player.AddComponent<PlayerPickupCollider>();
            var playerCollider = player.AddComponent<CircleCollider2D>();
            playerCollider.isTrigger = true;
            inventory.State.AddWeapon("weapon_a");
            inventory.State.AddWeapon("weapon_b");
            backpack.MaxItemCount = 1;

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_c";

            var pickupObject = new GameObject("Weapon Pickup");
            pickupObject.transform.position = player.transform.position;
            var body = pickupObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var pickupCollider = pickupObject.AddComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            var pickup = pickupObject.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = weapon;

            yield return new WaitForFixedUpdate();

            Assert.IsTrue(pickup == null || pickup.IsConsumed);
            Assert.AreEqual("weapon_a", inventory.State.GetWeaponId(0));
            Assert.AreEqual("weapon_b", inventory.State.GetWeaponId(1));
            Assert.That(backpack.State.GetCount("weapon_c"), Is.EqualTo(1));

            Object.Destroy(player);
            if (pickupObject != null)
            {
                Object.Destroy(pickupObject);
            }
            Object.Destroy(weapon);
        }

        [UnityTest]
        public IEnumerator WaveEnd_SweepsDistantGoldThroughWall_IntoInventory()
        {
            var economy = ScriptableObject.CreateInstance<EconomyBalanceTable>();
            economy.PickupMagnetRange = 8f;
            economy.PickupMagnetSpeed = 10.4f;
            economy.PickupSweepRange = 30f;
            economy.PickupSweepSpeed = 26f;
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            station.Economy = economy;

            var player = new GameObject("Player");
            var inventory = player.AddComponent<InventoryStateComponent>();
            player.AddComponent<PlayerPickupCollider>();
            var playerCollider = player.AddComponent<CircleCollider2D>();
            playerCollider.isTrigger = true;
            var field = player.AddComponent<PickupMagnetField>();
            field.BalanceStation = station;
            field.HandleWaveEnded();

            var wall = new GameObject("Wall");
            wall.transform.position = new Vector3(5f, 0f);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 4f);

            var pickupObject = new GameObject("Gold Pickup");
            pickupObject.transform.position = new Vector3(10f, 0f);
            var body = pickupObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var pickupCollider = pickupObject.AddComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            var pickup = pickupObject.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Gold;
            pickup.Amount = 1;
            var motion = pickupObject.AddComponent<PickupMagnetMotion>();
            motion.MagnetField = field;

            Assert.IsFalse(10f <= economy.PickupMagnetRange);

            for (int i = 0; i < 40 && pickup != null && !pickup.IsConsumed; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(pickup == null || pickup.IsConsumed);
            Assert.AreEqual(1, inventory.State.Gold);

            Object.Destroy(player);
            Object.Destroy(wall);
            if (pickupObject != null)
            {
                Object.Destroy(pickupObject);
            }
            Object.Destroy(station);
            Object.Destroy(economy);
        }
    }
}

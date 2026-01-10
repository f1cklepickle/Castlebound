using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Player
{
    public class PlayerWeaponSlotSwapInputTests
    {
        [Test]
        public void HandleWeaponSlotSwap_NonZeroScroll_SwapsOnce_AndRespectsCooldown()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();
            inventoryComponent.State.AddWeapon("weapon_basic");

            var controller = playerGo.AddComponent<PlayerController>();

            controller.HandleWeaponSlotSwap(0.1f, 0f);

            Assert.AreEqual(1, inventoryComponent.State.ActiveWeaponSlotIndex);

            controller.HandleWeaponSlotSwap(-0.2f, 0.2f);

            Assert.AreEqual(1, inventoryComponent.State.ActiveWeaponSlotIndex);

            controller.HandleWeaponSlotSwap(0.3f, 0.6f);

            Assert.AreEqual(0, inventoryComponent.State.ActiveWeaponSlotIndex);

            Object.DestroyImmediate(playerGo);
        }
    }
}

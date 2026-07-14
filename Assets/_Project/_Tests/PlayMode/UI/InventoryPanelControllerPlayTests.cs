using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Castlebound.Tests.UI
{
    public class InventoryPanelControllerPlayTests
    {
        [UnityTest]
        public IEnumerator InventoryPanel_OpensBackpack_AndDoesNotOwnWorldVaultView()
        {
            var root = new GameObject("InventoryPanelPlayRoot", typeof(Canvas));

            try
            {
                var backpack = root.AddComponent<BackpackInventoryStateComponent>();
                var vault = root.AddComponent<CastleInventoryStateComponent>();
                var phase = new WavePhaseTracker();
                var panel = root.AddComponent<InventoryPanelController>();

                panel.SetBackpackSource(backpack);
                panel.SetCastleInventorySource(vault);
                panel.SetPhaseTracker(phase);

                backpack.State.AddItem("weapon_sword", 1);
                panel.TogglePanel();
                yield return null;

                Assert.IsTrue(panel.IsOpen);
                Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Backpack));

                Assert.IsFalse(panel.OpenVaultFromWorld());
                yield return null;

                Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Backpack));
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator InventoryPanel_BackpackContextMenu_EquipsWeaponToMainSlot()
        {
            var root = new GameObject("InventoryPanelContextPlayRoot", typeof(Canvas));

            try
            {
                var backpack = root.AddComponent<BackpackInventoryStateComponent>();
                var activeInventory = root.AddComponent<InventoryStateComponent>();
                var vault = root.AddComponent<CastleInventoryStateComponent>();
                var resolver = root.AddComponent<TestWeaponResolver>();
                var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
                weapon.ItemId = "weapon_dagger";
                resolver.Weapon = weapon;

                var panel = root.AddComponent<InventoryPanelController>();
                panel.SetBackpackSource(backpack);
                panel.SetActiveInventorySource(activeInventory);
                panel.SetCastleInventorySource(vault);
                panel.SetWeaponDefinitionResolver(resolver);

                activeInventory.State.AddWeapon("weapon_sword");
                backpack.State.AddItem("weapon_dagger", 1);
                panel.TogglePanel();
                yield return null;

                var trigger = root.GetComponentInChildren<InventoryContextMenuTrigger>(true);
                Assert.NotNull(trigger);
                trigger.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
                yield return null;

                ClickButton(root, "Equip");
                yield return null;

                ClickButton(root, "Main");
                yield return null;

                Assert.That(activeInventory.State.GetWeaponId(0), Is.EqualTo("weapon_dagger"));
                Assert.That(backpack.State.GetCount("weapon_sword"), Is.EqualTo(1));

                Object.Destroy(weapon);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator InventoryPanel_BackpackContextMenu_VaultMovesOneItem_WhenInsideCastlePreWave()
        {
            var root = new GameObject("InventoryPanelVaultMovePlayRoot", typeof(Canvas));

            try
            {
                var backpack = root.AddComponent<BackpackInventoryStateComponent>();
                root.AddComponent<InventoryStateComponent>();
                var vault = root.AddComponent<CastleInventoryStateComponent>();
                var castleRegion = root.AddComponent<CastleRegionTracker>();
                var phase = new WavePhaseTracker();
                var panel = root.AddComponent<InventoryPanelController>();
                panel.SetBackpackSource(backpack);
                panel.SetCastleInventorySource(vault);
                panel.SetCastleRegionTracker(castleRegion);
                panel.SetPhaseTracker(phase);

                castleRegion.Debug_SetPlayerInsideForTests(true);
                backpack.State.AddItem("potion_health", 2);
                panel.TogglePanel();
                yield return null;

                var trigger = root.GetComponentInChildren<InventoryContextMenuTrigger>(true);
                Assert.NotNull(trigger);
                trigger.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
                yield return null;

                ClickButton(root, "Vault");
                yield return null;

                Assert.That(backpack.State.GetCount("potion_health"), Is.EqualTo(1));
                Assert.That(vault.State.GetCount("potion_health"), Is.EqualTo(1));
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator InventoryPanel_ShopRendersInCastle_AndClosesOnWaveStart()
        {
            var root = new GameObject("InventoryPanelShopPlayRoot", typeof(Canvas));

            try
            {
                root.AddComponent<BackpackInventoryStateComponent>();
                root.AddComponent<CastleInventoryStateComponent>();
                var phase = new WavePhaseTracker();
                var castleRegion = root.AddComponent<CastleRegionTracker>();
                var panel = root.AddComponent<InventoryPanelController>();
                panel.SetPhaseTracker(phase);
                panel.SetCastleRegionTracker(castleRegion);

                castleRegion.Debug_SetPlayerInsideForTests(true);

                panel.TogglePanel();
                panel.ShopTabButton.onClick.Invoke();
                yield return null;

                Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Shop));
                AssertTextExists(root, "Castle Shop");
                AssertTextExists(root, "Sword - 250 gold");
                AssertTextExists(root, "Iron Club - 250 gold");
                AssertTextExists(root, "Health Potion - 50 gold");

                phase.SetPhase(WavePhase.InWave);
                yield return null;

                Assert.IsFalse(panel.IsOpen);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator InventoryPanel_ShopBuy_SpendsGoldAndAddsItem()
        {
            var root = new GameObject("InventoryPanelShopBuyPlayRoot", typeof(Canvas));

            try
            {
                var backpack = root.AddComponent<BackpackInventoryStateComponent>();
                var activeInventory = root.AddComponent<InventoryStateComponent>();
                root.AddComponent<CastleInventoryStateComponent>();
                var phase = new WavePhaseTracker();
                var castleRegion = root.AddComponent<CastleRegionTracker>();
                var panel = root.AddComponent<InventoryPanelController>();
                panel.SetBackpackSource(backpack);
                panel.SetActiveInventorySource(activeInventory);
                panel.SetPhaseTracker(phase);
                panel.SetCastleRegionTracker(castleRegion);

                castleRegion.Debug_SetPlayerInsideForTests(true);
                activeInventory.State.AddGold(300);

                panel.TogglePanel();
                panel.ShopTabButton.onClick.Invoke();
                yield return null;

                ClickButton(root, "Buy");
                yield return null;

                Assert.That(activeInventory.State.Gold, Is.EqualTo(50));
                Assert.That(backpack.State.GetCount("weapon_sword"), Is.EqualTo(1));
                AssertTextExists(root, "Purchase complete");
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        private static void ClickButton(GameObject root, string label)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var text = buttons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null && text.text == label)
                {
                    buttons[i].onClick.Invoke();
                    return;
                }
            }

            Assert.Fail($"Expected button '{label}'.");
        }

        private static void AssertTextExists(GameObject root, string expected)
        {
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var label in labels)
            {
                if (label.text == expected)
                {
                    return;
                }
            }

            Assert.Fail($"Expected text '{expected}'.");
        }

        private sealed class TestWeaponResolver : MonoBehaviour, IWeaponDefinitionResolver
        {
            public WeaponDefinition Weapon { get; set; }

            public WeaponDefinition Resolve(string weaponId)
            {
                return Weapon != null && Weapon.ItemId == weaponId ? Weapon : null;
            }
        }
    }
}

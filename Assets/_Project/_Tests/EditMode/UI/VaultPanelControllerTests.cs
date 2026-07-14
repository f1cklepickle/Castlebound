using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Castlebound.Tests.UI
{
    public class VaultPanelControllerTests
    {
        private GameObject root;
        private VaultPanelController panel;
        private BackpackInventoryStateComponent backpack;
        private CastleInventoryStateComponent vault;
        private InventoryStateComponent activeInventory;
        private WavePhaseTracker phase;
        private WeaponDefinition weapon;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("VaultPanelRoot", typeof(Canvas));
            backpack = root.AddComponent<BackpackInventoryStateComponent>();
            vault = root.AddComponent<CastleInventoryStateComponent>();
            activeInventory = root.AddComponent<InventoryStateComponent>();
            var resolver = root.AddComponent<TestWeaponResolver>();
            weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_sword";
            resolver.Weapon = weapon;
            phase = new WavePhaseTracker();
            panel = root.AddComponent<VaultPanelController>();
            panel.SetInventorySources(backpack, vault, activeInventory);
            panel.SetPhaseTracker(phase);
            panel.SetWeaponDefinitionResolver(resolver);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void OpenFromWorld_RendersVaultEntries_BetweenWaves()
        {
            phase.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("potion_basic", 3);

            Assert.IsTrue(panel.OpenFromWorld());

            Assert.IsTrue(panel.IsOpen);
            AssertTextExists("Vault");
            AssertTextExists("potion_basic x3");
        }

        [Test]
        public void OpenFromWorld_IsDenied_MidWave()
        {
            phase.SetPhase(WavePhase.InWave);
            vault.State.AddItem("potion_basic", 3);

            Assert.IsFalse(panel.OpenFromWorld());

            Assert.IsFalse(panel.IsOpen);
            AssertTextMissing("potion_basic x3");
        }

        [Test]
        public void VaultChanges_RefreshVisibleRows()
        {
            phase.SetPhase(WavePhase.PreWave);
            Assert.IsTrue(panel.OpenFromWorld());

            vault.State.AddItem("weapon_sword", 1);

            AssertTextExists("weapon_sword x1");
        }

        [Test]
        public void CloseButton_ClosesVaultPanel()
        {
            phase.SetPhase(WavePhase.PreWave);
            Assert.IsTrue(panel.OpenFromWorld());

            ClickButton("X");

            Assert.IsFalse(panel.IsOpen);
        }

        [Test]
        public void VaultRow_ButtonClickOpensContextMenu_WithMoveAndEquipActions()
        {
            phase.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("weapon_sword", 1);
            Assert.IsTrue(panel.OpenFromWorld());

            var trigger = root.GetComponentInChildren<InventoryContextMenuTrigger>(true);
            var rowButton = trigger.GetComponent<Button>();

            Assert.NotNull(rowButton);
            rowButton.onClick.Invoke();

            AssertTextExists("Backpack");
            AssertTextExists("Equip");
        }

        [Test]
        public void VaultContextMenu_MoveToBackpack_RefreshesVaultRowsWithoutClosingWhenMoreRemain()
        {
            phase.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("weapon_sword", 2);
            Assert.IsTrue(panel.OpenFromWorld());

            OpenFirstVaultRowContextMenu();
            ClickButton("Backpack");
            var menu = root.GetComponentInChildren<InventoryContextMenuController>(true);

            Assert.IsTrue(menu.IsOpen);
            Assert.That(menu.ActiveSource, Is.EqualTo(InventoryContextSource.Vault));
            Assert.That(vault.State.GetCount("weapon_sword"), Is.EqualTo(1));
            Assert.That(backpack.State.GetCount("weapon_sword"), Is.EqualTo(1));
            AssertTextExists("weapon_sword x1");
        }

        [Test]
        public void VaultContextMenu_EquipWeapon_ReturnsDisplacedWeaponToVault()
        {
            phase.SetPhase(WavePhase.PreWave);
            activeInventory.State.AddWeapon("weapon_dagger");
            vault.State.AddItem("weapon_sword", 1);
            Assert.IsTrue(panel.OpenFromWorld());

            OpenFirstVaultRowContextMenu();

            ClickButton("Equip");
            ClickButton("Main");

            Assert.That(activeInventory.State.GetWeaponId(0), Is.EqualTo("weapon_sword"));
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void VaultContextMenu_CreatesVaultScopedMenu_WhenSharingRootWithInventoryPanel()
        {
            var inventoryPanel = root.AddComponent<InventoryPanelController>();
            inventoryPanel.SetBackpackSource(backpack);
            inventoryPanel.SetActiveInventorySource(activeInventory);
            inventoryPanel.SetCastleInventorySource(vault);
            inventoryPanel.SetPhaseTracker(phase);
            inventoryPanel.SetWeaponDefinitionResolver(root.GetComponent<TestWeaponResolver>());

            phase.SetPhase(WavePhase.PreWave);
            backpack.State.AddItem("weapon_sword", 1);
            vault.State.AddItem("weapon_sword", 1);
            inventoryPanel.TogglePanel();

            Assert.IsTrue(panel.OpenFromWorld());

            OpenFirstVaultRowContextMenu();
            var vaultMenu = FindMenuWithSource(InventoryContextSource.Vault);

            Assert.NotNull(vaultMenu);
            Assert.That(vaultMenu.transform.parent.name, Is.EqualTo("VaultPanel"));

            OpenFirstInventoryRowContextMenu();
            var backpackMenu = root.GetComponent<InventoryContextMenuController>();

            Assert.NotNull(backpackMenu);
            Assert.IsTrue(backpackMenu.IsOpen);
            Assert.That(backpackMenu.ActiveSource, Is.EqualTo(InventoryContextSource.Backpack));
            AssertTextExists("Drop");
            AssertTextExists("Equip");
        }

        private void OpenFirstVaultRowContextMenu()
        {
            var trigger = FindTriggerUnder("VaultRows");
            Assert.NotNull(trigger);
            var eventData = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right };
            trigger.OnPointerClick(eventData);
        }

        private void OpenFirstInventoryRowContextMenu()
        {
            var trigger = FindTriggerUnder("InventoryRows");
            Assert.NotNull(trigger);
            var eventData = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right };
            trigger.OnPointerClick(eventData);
        }

        private void ClickButton(string label)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                if (!button.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null && text.text == label)
                {
                    button.onClick.Invoke();
                    return;
                }
            }

            Assert.Fail($"Expected button '{label}'.");
        }

        private void AssertTextExists(string expected)
        {
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var label in labels)
            {
                if (label.text == expected)
                {
                    return;
                }
            }

            Assert.Fail($"Expected vault panel text '{expected}'.");
        }

        private void AssertTextMissing(string expected)
        {
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var label in labels)
            {
                if (label.text == expected)
                {
                    Assert.Fail($"Did not expect vault panel text '{expected}'.");
                }
            }
        }

        private InventoryContextMenuController FindMenuWithSource(InventoryContextSource source)
        {
            var menus = root.GetComponentsInChildren<InventoryContextMenuController>(true);
            foreach (var menu in menus)
            {
                if (menu.IsOpen && menu.ActiveSource == source)
                {
                    return menu;
                }
            }

            return null;
        }

        private InventoryContextMenuTrigger FindTriggerUnder(string parentName)
        {
            var triggers = root.GetComponentsInChildren<InventoryContextMenuTrigger>(true);
            foreach (var trigger in triggers)
            {
                if (HasParentNamed(trigger.transform, parentName))
                {
                    return trigger;
                }
            }

            return null;
        }

        private static bool HasParentNamed(Transform child, string parentName)
        {
            var current = child;
            while (current != null)
            {
                if (current.name == parentName)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
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

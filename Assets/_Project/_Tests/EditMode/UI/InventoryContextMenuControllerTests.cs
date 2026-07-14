using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Tests.UI
{
    public class InventoryContextMenuControllerTests
    {
        private GameObject root;
        private BackpackInventoryStateComponent backpack;
        private CastleInventoryStateComponent vault;
        private InventoryStateComponent activeInventory;
        private InventoryContextMenuController menu;
        private BackpackWeaponEquipController equipController;
        private BackpackItemDropController dropController;
        private CastleRegionTracker castleRegion;
        private WavePhaseTracker phase;
        private WeaponDefinition weapon;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("MenuRoot", typeof(Canvas), typeof(RectTransform));
            backpack = root.AddComponent<BackpackInventoryStateComponent>();
            vault = root.AddComponent<CastleInventoryStateComponent>();
            activeInventory = root.AddComponent<InventoryStateComponent>();
            var resolver = root.AddComponent<TestWeaponResolver>();
            equipController = root.AddComponent<BackpackWeaponEquipController>();
            dropController = root.AddComponent<BackpackItemDropController>();
            castleRegion = root.AddComponent<CastleRegionTracker>();
            phase = new WavePhaseTracker();
            menu = root.AddComponent<InventoryContextMenuController>();

            weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_dagger";
            resolver.Weapon = weapon;
            castleRegion.Debug_SetPlayerInsideForTests(true);

            equipController.SetActiveInventorySource(activeInventory);
            equipController.SetBackpackSource(backpack);
            dropController.SetBackpackSource(backpack);
            dropController.SetWeaponDefinitionResolver(resolver);
            dropController.SetDropOrigin(root.transform);
            menu.SetParentRoot(root.GetComponent<RectTransform>());
            menu.SetEquipController(equipController);
            menu.SetDropController(dropController);
            menu.SetInventorySources(backpack, vault, activeInventory);
            menu.SetAccessContext(castleRegion, phase);
        }

        [TearDown]
        public void TearDown()
        {
            if (dropController != null && dropController.LastSpawnedPickup != null)
            {
                Object.DestroyImmediate(dropController.LastSpawnedPickup.gameObject);
            }

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void ShowForWeapon_EquipButtonTransformsIntoMainAndSecondaryChoices()
        {
            menu.ShowForItem("weapon_dagger", true, null);

            ClickButton("Equip");

            Assert.IsTrue(menu.IsChoosingEquipSlot);
            AssertTextExists("Main");
            AssertTextExists("Secondary");
        }

        [Test]
        public void ClickingMain_EquipsBackpackWeaponIntoMainSlot()
        {
            backpack.State.AddItem("weapon_dagger", 1);
            activeInventory.State.AddWeapon("weapon_sword");
            menu.ShowForItem("weapon_dagger", true, null);
            ClickButton("Equip");

            ClickButton("Main");

            Assert.IsFalse(menu.IsOpen);
            Assert.That(activeInventory.State.GetWeaponId(0), Is.EqualTo("weapon_dagger"));
            Assert.That(backpack.State.GetCount("weapon_sword"), Is.EqualTo(1));
        }

        [Test]
        public void ClickingMain_EquipsVaultWeaponIntoMainSlot_AndReturnsDisplacedWeaponToVault()
        {
            vault.State.AddItem("weapon_dagger", 1);
            activeInventory.State.AddWeapon("weapon_sword");
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Vault, null);
            ClickButton("Equip");

            ClickButton("Main");

            Assert.IsFalse(menu.IsOpen);
            Assert.That(activeInventory.State.GetWeaponId(0), Is.EqualTo("weapon_dagger"));
            Assert.That(vault.State.GetCount("weapon_sword"), Is.EqualTo(1));
        }

        [Test]
        public void MoveToBackpack_MovesOneVaultItem_AndKeepsMenuOpenWhenMoreRemain()
        {
            vault.State.AddItem("weapon_dagger", 2);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Vault, null);

            ClickButton("Backpack");

            Assert.IsTrue(menu.IsOpen);
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(1));
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void MoveToBackpack_ClosesMenuWhenLastVaultItemMoves()
        {
            vault.State.AddItem("weapon_dagger", 1);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Vault, null);

            ClickButton("Backpack");

            Assert.IsFalse(menu.IsOpen);
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(0));
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void Vault_MovesOneBackpackItem_AndKeepsMenuOpenWhenMoreRemain()
        {
            backpack.State.AddItem("weapon_dagger", 2);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Backpack, null);

            ClickButton("Vault");

            Assert.IsTrue(menu.IsOpen);
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void Vault_ClosesMenuWhenLastBackpackItemMoves()
        {
            backpack.State.AddItem("weapon_dagger", 1);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Backpack, null);

            ClickButton("Vault");

            Assert.IsFalse(menu.IsOpen);
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(0));
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void Vault_DoesNotMoveBackpackItem_WhenPlayerOutsideCastle()
        {
            backpack.State.AddItem("weapon_dagger", 1);
            castleRegion.Debug_SetPlayerInsideForTests(false);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Backpack, null);

            ClickButton("Vault");

            Assert.IsTrue(menu.IsOpen);
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(0));
        }

        [Test]
        public void Vault_DoesNotMoveBackpackItem_DuringWave()
        {
            backpack.State.AddItem("weapon_dagger", 1);
            phase.SetPhase(WavePhase.InWave);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Backpack, null);

            ClickButton("Vault");

            Assert.IsTrue(menu.IsOpen);
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(0));
        }

        [Test]
        public void ShowForItem_ClampsMenuInsideParentRoot()
        {
            var parent = root.GetComponent<RectTransform>();
            parent.sizeDelta = new Vector2(160f, 120f);

            var anchorObject = new GameObject("Anchor", typeof(RectTransform));
            anchorObject.transform.SetParent(root.transform, false);
            var anchor = anchorObject.GetComponent<RectTransform>();
            anchor.anchorMin = new Vector2(0f, 1f);
            anchor.anchorMax = new Vector2(0f, 1f);
            anchor.pivot = new Vector2(0f, 1f);
            anchor.anchoredPosition = new Vector2(150f, -110f);
            anchor.sizeDelta = new Vector2(34f, 34f);

            menu.ShowForItem("weapon_dagger", true, anchor);

            var menuRoot = FindRectTransform("InventoryContextMenu");
            Assert.NotNull(menuRoot);
            Assert.That(menuRoot.anchoredPosition.x, Is.InRange(0f, parent.rect.width - menuRoot.sizeDelta.x));
            Assert.That(menuRoot.anchoredPosition.y, Is.InRange(-parent.rect.height + menuRoot.sizeDelta.y, 0f));
        }

        private void ClickButton(string label)
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

        private void AssertTextExists(string expected)
        {
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].text == expected)
                {
                    return;
                }
            }

            Assert.Fail($"Expected text '{expected}'.");
        }

        private RectTransform FindRectTransform(string objectName)
        {
            var rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].name == objectName)
                {
                    return rects[i];
                }
            }

            return null;
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

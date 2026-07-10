using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
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
            menu = root.AddComponent<InventoryContextMenuController>();

            weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_dagger";
            resolver.Weapon = weapon;

            equipController.SetActiveInventorySource(activeInventory);
            equipController.SetBackpackSource(backpack);
            dropController.SetBackpackSource(backpack);
            dropController.SetWeaponDefinitionResolver(resolver);
            dropController.SetDropOrigin(root.transform);
            menu.SetParentRoot(root.GetComponent<RectTransform>());
            menu.SetEquipController(equipController);
            menu.SetDropController(dropController);
            menu.SetInventorySources(backpack, vault, activeInventory);
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

            ClickButton("Move to Backpack");

            Assert.IsTrue(menu.IsOpen);
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(1));
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
        }

        [Test]
        public void MoveToBackpack_ClosesMenuWhenLastVaultItemMoves()
        {
            vault.State.AddItem("weapon_dagger", 1);
            menu.ShowForItem("weapon_dagger", true, InventoryContextSource.Vault, null);

            ClickButton("Move to Backpack");

            Assert.IsFalse(menu.IsOpen);
            Assert.That(vault.State.GetCount("weapon_dagger"), Is.EqualTo(0));
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(1));
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

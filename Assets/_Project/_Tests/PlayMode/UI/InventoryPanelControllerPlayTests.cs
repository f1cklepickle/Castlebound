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
        public IEnumerator InventoryPanel_OpensBackpack_AndDeniesWorldVaultMidWave()
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

                phase.SetPhase(WavePhase.InWave);
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

using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Castlebound.Gameplay.Input;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;

namespace Castlebound.Tests.UI
{
    public class TouchUIBindingsTests
    {
        private class FakeHealable : MonoBehaviour, IHealable
        {
            public void Heal(int amount) { }
        }

        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Root");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void PotionButton_OnClick_TriggersPotionConsume()
        {
            var definition = ScriptableObject.CreateInstance<PotionDefinition>();
            definition.ItemId = "potion_health";
            definition.CooldownSeconds = 1f;

            var resolver = _root.AddComponent<PotionDefinitionResolverComponent>();
            SetPrivateField(resolver, "definitions", new PotionDefinition[] { definition });

            var healable = _root.AddComponent<FakeHealable>();
            var inventory = _root.AddComponent<InventoryStateComponent>();
            inventory.State.TryAddPotion("potion_health", 1);

            var potionController = _root.AddComponent<PotionUseController>();
            potionController.SetInventorySource(inventory);
            potionController.SetResolverSource(resolver);
            potionController.SetHealTargetSource(healable);

            var button = new GameObject("PotionButton").AddComponent<Button>();
            button.transform.SetParent(_root.transform);

            var bindings = _root.AddComponent<TouchUIBindings>();
            bindings.SetPotionUseController(potionController);
            bindings.SetPotionButton(button);
            bindings.Initialize();

            button.onClick.Invoke();

            Assert.Greater(potionController.CooldownRemaining, 0f,
                "Clicking the potion button should trigger potion consume and start cooldown.");
        }

        [Test]
        public void WeaponButton_OnClick_SwapsWeaponSlot()
        {
            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(_root.transform);
            var inventory = playerGo.AddComponent<InventoryStateComponent>();
            inventory.State.AddWeapon("weapon_a");
            inventory.State.AddWeapon("weapon_b");
            var player = playerGo.AddComponent<PlayerController>();

            var button = new GameObject("WeaponButton").AddComponent<Button>();
            button.transform.SetParent(_root.transform);

            var bindings = _root.AddComponent<TouchUIBindings>();
            bindings.SetPlayerController(player);
            bindings.SetWeaponButton(button);
            bindings.Initialize();

            var slotBefore = inventory.State.ActiveWeaponSlotIndex;
            button.onClick.Invoke();

            Assert.AreNotEqual(slotBefore, inventory.State.ActiveWeaponSlotIndex,
                "Clicking the weapon button should swap the active weapon slot.");
        }

        [Test]
        public void CloseButton_OnClick_ClosesUpgradeMenu()
        {
            var phase = new WavePhaseTracker();
            var menuGo = new GameObject("Menu");
            menuGo.transform.SetParent(_root.transform);
            var upgradeMenu = menuGo.AddComponent<UpgradeMenuController>();
            upgradeMenu.SetPhaseTracker(phase);
            upgradeMenu.SetAutoOpenOnFirstPreWave(true);
            phase.SetPhase(WavePhase.InWave);
            phase.SetPhase(WavePhase.PreWave);
            Assert.IsTrue(upgradeMenu.IsMenuOpen, "Precondition: menu must be open.");

            var button = new GameObject("CloseButton").AddComponent<Button>();
            button.transform.SetParent(_root.transform);

            var bindings = _root.AddComponent<TouchUIBindings>();
            bindings.SetUpgradeMenuController(upgradeMenu);
            bindings.SetCloseButton(button);
            bindings.Initialize();

            button.onClick.Invoke();

            Assert.IsFalse(upgradeMenu.IsMenuOpen,
                "Clicking the close button should close the upgrade menu.");
        }

        [Test]
        public void Initialize_WithoutAllBindingsSet_DoesNotThrow()
        {
            var bindings = _root.AddComponent<TouchUIBindings>();

            Assert.DoesNotThrow(() => bindings.Initialize(),
                "Initialize should not throw when optional bindings are not set.");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
                field.SetValue(target, value);
        }
    }
}

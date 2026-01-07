using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Combat;

namespace Castlebound.Tests.UI
{
    public class WeaponSlotsHudTests
    {
        private class FakeWeaponDefinitionResolver : MonoBehaviour, IWeaponDefinitionResolver
        {
            private readonly Dictionary<string, WeaponDefinition> definitions =
                new Dictionary<string, WeaponDefinition>();

            public void SetDefinition(WeaponDefinition definition)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.ItemId))
                {
                    return;
                }

                definitions[definition.ItemId] = definition;
            }

            public WeaponDefinition Resolve(string weaponId)
            {
                if (string.IsNullOrWhiteSpace(weaponId))
                {
                    return null;
                }

                WeaponDefinition definition;
                return definitions.TryGetValue(weaponId, out definition) ? definition : null;
            }
        }

        [Test]
        public void OnEnable_InitializesSlotIconsAndActiveHighlight_FromInventory()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddWeapon("weapon_a");
            inventory.AddWeapon("weapon_b");
            inventory.SetActiveWeaponSlot(1);

            var resolverObject = new GameObject("Resolver");
            var resolver = resolverObject.AddComponent<FakeWeaponDefinitionResolver>();
            var weaponA = CreateWeaponDefinition("weapon_a");
            var weaponB = CreateWeaponDefinition("weapon_b");
            resolver.SetDefinition(weaponA);
            resolver.SetDefinition(weaponB);

            var iconA = CreateImage("Slot0Icon");
            var iconB = CreateImage("Slot1Icon");
            var highlightA = CreateImage("Slot0Highlight");
            var highlightB = CreateImage("Slot1Highlight");

            var hud = root.AddComponent<WeaponSlotsHud>();
            SetPrivateField(hud, "inventorySource", inventoryComponent);
            SetPrivateField(hud, "resolverSource", resolver);
            SetPrivateField(hud, "slotIcons", new Image[] { iconA, iconB });
            SetPrivateField(hud, "activeSlotHighlights", new Image[] { highlightA, highlightB });

            hud.Initialize();

            Assert.AreEqual(weaponA.Icon, iconA.sprite);
            Assert.AreEqual(weaponB.Icon, iconB.sprite);
            Assert.IsFalse(highlightA.enabled);
            Assert.IsTrue(highlightB.enabled);
        }

        [Test]
        public void InventoryChange_UpdatesActiveHighlight_WhenSlotChanges()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddWeapon("weapon_a");
            inventory.AddWeapon("weapon_b");
            inventory.SetActiveWeaponSlot(0);

            var resolverObject = new GameObject("Resolver");
            var resolver = resolverObject.AddComponent<FakeWeaponDefinitionResolver>();
            resolver.SetDefinition(CreateWeaponDefinition("weapon_a"));
            resolver.SetDefinition(CreateWeaponDefinition("weapon_b"));

            var iconA = CreateImage("Slot0Icon");
            var iconB = CreateImage("Slot1Icon");
            var highlightA = CreateImage("Slot0Highlight");
            var highlightB = CreateImage("Slot1Highlight");

            var hud = root.AddComponent<WeaponSlotsHud>();
            SetPrivateField(hud, "inventorySource", inventoryComponent);
            SetPrivateField(hud, "resolverSource", resolver);
            SetPrivateField(hud, "slotIcons", new Image[] { iconA, iconB });
            SetPrivateField(hud, "activeSlotHighlights", new Image[] { highlightA, highlightB });

            hud.Initialize();

            inventory.SetActiveWeaponSlot(1);

            Assert.IsFalse(highlightA.enabled);
            Assert.IsTrue(highlightB.enabled);
        }

        private static WeaponDefinition CreateWeaponDefinition(string id)
        {
            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.ItemId = id;
            definition.Icon = CreateSprite();
            return definition;
        }

        private static Sprite CreateSprite()
        {
            var texture = new Texture2D(2, 2);
            return Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }

        private static Image CreateImage(string name)
        {
            var go = new GameObject(name);
            return go.AddComponent<Image>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}

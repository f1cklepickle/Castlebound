using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Tests.UI
{
    public class HudInventoryFallbackTests
    {
        [Test]
        public void GoldXpHud_AutoFindsInventory_WhenMissing()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            var inventory = player.AddComponent<InventoryStateComponent>();

            var root = new GameObject("Hud");
            var goldText = CreateText("GoldText", root.transform);
            var xpText = CreateText("XpText", root.transform);
            var hud = root.AddComponent<GoldXpHud>();

            SetPrivateField(hud, "goldText", goldText);
            SetPrivateField(hud, "xpText", xpText);

            inventory.State.AddGold(5);
            hud.Initialize();

            Assert.AreEqual("5", goldText.text);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void WeaponSlotsHud_AutoFindsInventory_WhenMissing()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.AddComponent<InventoryStateComponent>();

            var root = new GameObject("Hud");
            var hud = root.AddComponent<WeaponSlotsHud>();
            SetPrivateField(hud, "slotIcons", new Image[0]);

            hud.Initialize();

            Assert.IsNotNull(GetPrivateField<InventoryState>(hud, "inventory"));

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(player);
        }

        [Test]
        public void PotionHudSlot_AutoFindsInventory_WhenMissing()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            var inventoryComponent = player.AddComponent<InventoryStateComponent>();
            var resolver = player.AddComponent<PotionDefinitionResolverComponent>();
            var useController = player.AddComponent<PotionUseController>();

            var root = new GameObject("Hud");
            var slot = root.AddComponent<PotionHudSlot>();

            var icon = CreateImage("Icon", root.transform);
            var count = CreateText("CountText", root.transform);
            var cooldown = CreateImage("CooldownOverlay", root.transform);

            SetPrivateField(slot, "iconImage", icon);
            SetPrivateField(slot, "countText", count);
            SetPrivateField(slot, "cooldownOverlay", cooldown);

            slot.Initialize();

            Assert.IsNotNull(GetPrivateField<InventoryState>(slot, "inventory"));
            Assert.AreSame(inventoryComponent, GetPrivateField<InventoryStateComponent>(slot, "inventorySource"));
            Assert.AreSame(resolver, GetPrivateField<PotionDefinitionResolverComponent>(slot, "resolverSource"));
            Assert.AreSame(useController, GetPrivateField<PotionUseController>(slot, "potionUseController"));

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(player);
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.AddComponent<TextMeshProUGUI>();
        }

        private static Image CreateImage(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.AddComponent<Image>();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(target);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using TMPro;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.UI
{
    public class GoldXpHudTests
    {
        [Test]
        public void OnEnable_InitializesGoldAndXpText_FromInventory()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;
            inventory.AddGold(7);
            inventory.AddXp(12);

            var goldText = CreateText("GoldText");
            var xpText = CreateText("XpText");

            var hud = root.AddComponent<GoldXpHud>();
            SetPrivateField(hud, "inventorySource", inventoryComponent);
            SetPrivateField(hud, "goldText", goldText);
            SetPrivateField(hud, "xpText", xpText);

            hud.Initialize();

            Assert.AreEqual("7", goldText.text);
            Assert.AreEqual("12", xpText.text);
        }

        [Test]
        public void InventoryChange_UpdatesGoldAndXpText_OnCurrencyChange()
        {
            var root = new GameObject("Root");
            root.SetActive(false);
            var inventoryComponent = root.AddComponent<InventoryStateComponent>();
            var inventory = inventoryComponent.State;

            var goldText = CreateText("GoldText");
            var xpText = CreateText("XpText");

            var hud = root.AddComponent<GoldXpHud>();
            SetPrivateField(hud, "inventorySource", inventoryComponent);
            SetPrivateField(hud, "goldText", goldText);
            SetPrivateField(hud, "xpText", xpText);

            hud.Initialize();

            inventory.AddGold(3);
            inventory.AddXp(5);

            Assert.AreEqual("3", goldText.text);
            Assert.AreEqual("5", xpText.text);
        }

        private static TextMeshProUGUI CreateText(string name)
        {
            var go = new GameObject(name);
            return go.AddComponent<TextMeshProUGUI>();
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

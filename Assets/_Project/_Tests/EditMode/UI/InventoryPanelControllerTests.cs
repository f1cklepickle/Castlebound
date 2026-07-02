using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Tests.UI
{
    public class InventoryPanelControllerTests
    {
        private GameObject root;
        private InventoryPanelController panel;
        private BackpackInventoryStateComponent backpack;
        private CastleInventoryStateComponent vault;
        private WavePhaseTracker phase;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("InventoryPanelRoot", typeof(Canvas));
            backpack = root.AddComponent<BackpackInventoryStateComponent>();
            vault = root.AddComponent<CastleInventoryStateComponent>();
            phase = new WavePhaseTracker();
            panel = root.AddComponent<InventoryPanelController>();
            panel.SetBackpackSource(backpack);
            panel.SetCastleInventorySource(vault);
            panel.SetPhaseTracker(phase);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void TogglePanel_OpensToBackpackTab()
        {
            vault.State.AddItem("weapon_sword", 1);

            panel.SetActiveTab(InventoryPanelTab.Vault);
            panel.TogglePanel();

            Assert.IsTrue(panel.IsOpen);
            Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Backpack));
        }

        [Test]
        public void BackpackTab_RendersBackpackEntries()
        {
            backpack.State.AddItem("weapon_sword", 2);

            panel.TogglePanel();

            AssertTextExists("weapon_sword x2");
        }

        [Test]
        public void VaultTab_RendersVaultEntries_BetweenWaves()
        {
            phase.SetPhase(WavePhase.PreWave);
            vault.State.AddItem("potion_health", 3);

            panel.TogglePanel();
            panel.SetActiveTab(InventoryPanelTab.Vault);

            AssertTextExists("potion_health x3");
            Assert.That(panel.VaultTabButton.interactable, Is.False);
        }

        [Test]
        public void VaultTab_IsDisabled_MidWave()
        {
            phase.SetPhase(WavePhase.InWave);

            panel.TogglePanel();
            panel.SetActiveTab(InventoryPanelTab.Vault);

            Assert.That(panel.ActiveTab, Is.EqualTo(InventoryPanelTab.Backpack));
            Assert.That(panel.VaultTabButton.interactable, Is.False);
            Assert.That(panel.BackpackTabButton.interactable, Is.False);
        }

        [Test]
        public void BackpackChanges_RefreshVisibleRows()
        {
            panel.TogglePanel();

            backpack.State.AddItem("weapon_axe", 1);

            AssertTextExists("weapon_axe x1");
        }

        [Test]
        public void OpenButton_AnchorsTopLeft_ForResponsiveHudPlacement()
        {
            var rect = panel.OpenButton.GetComponent<RectTransform>();

            Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(rect.pivot, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(rect.anchoredPosition.x, Is.GreaterThanOrEqualTo(0f));
            Assert.That(rect.anchoredPosition.y, Is.LessThanOrEqualTo(0f));
        }

        [Test]
        public void RuntimeLayout_KeepsTabsAboveRows()
        {
            panel.TogglePanel();

            var tabRoot = panel.BackpackTabButton.transform.parent as RectTransform;
            var rowRoot = FindRectTransform("InventoryRows");
            var backpackLayout = panel.BackpackTabButton.GetComponent<LayoutElement>();
            var vaultLayout = panel.VaultTabButton.GetComponent<LayoutElement>();

            Assert.NotNull(tabRoot);
            Assert.NotNull(rowRoot);
            Assert.NotNull(backpackLayout);
            Assert.NotNull(vaultLayout);
            Assert.That(tabRoot.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(tabRoot.anchorMax, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(rowRoot.anchorMin, Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(rowRoot.anchorMax, Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(rowRoot.offsetMax.y, Is.LessThan(tabRoot.offsetMin.y));
            Assert.That(backpackLayout.preferredWidth, Is.GreaterThanOrEqualTo(120f));
            Assert.That(vaultLayout.preferredWidth, Is.GreaterThanOrEqualTo(120f));
        }

        [Test]
        public void RuntimePanel_BackgroundAndLabels_DoNotBlockGameplayPointerInput()
        {
            panel.TogglePanel();

            var panelBackground = FindRectTransform("InventoryPanel").GetComponent<Image>();
            var rowLabel = FindRectTransform("InventoryRow").GetComponent<TextMeshProUGUI>();
            var tabLabel = panel.BackpackTabButton.GetComponentInChildren<TextMeshProUGUI>();

            Assert.NotNull(panelBackground);
            Assert.NotNull(rowLabel);
            Assert.NotNull(tabLabel);
            Assert.IsFalse(panelBackground.raycastTarget);
            Assert.IsFalse(rowLabel.raycastTarget);
            Assert.IsFalse(tabLabel.raycastTarget);
            Assert.IsTrue(panel.OpenButton.GetComponent<Image>().raycastTarget);
            Assert.IsTrue(panel.BackpackTabButton.GetComponent<Image>().raycastTarget);
            Assert.IsTrue(panel.VaultTabButton.GetComponent<Image>().raycastTarget);
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

            Assert.Fail($"Expected inventory panel text '{expected}'.");
        }

        private RectTransform FindRectTransform(string objectName)
        {
            var rects = root.GetComponentsInChildren<RectTransform>(true);
            foreach (var rect in rects)
            {
                if (rect.name == objectName)
                {
                    return rect;
                }
            }

            return null;
        }
    }
}

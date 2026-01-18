using System.Collections.Generic;
using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class UpgradeMenuListView : MonoBehaviour
    {
        [SerializeField] private UpgradeMenuController menuController;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private FeedbackEventChannel feedbackChannel;
        [SerializeField] private InventoryStateComponent inventorySource;

        private readonly List<Row> rows = new List<Row>();

        private void OnEnable()
        {
            ResolveReferences();
            if (menuController != null)
            {
                menuController.MenuStateChanged += OnMenuStateChanged;
            }
        }

        private void OnDisable()
        {
            if (menuController != null)
            {
                menuController.MenuStateChanged -= OnMenuStateChanged;
            }
        }

        private void OnMenuStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            EnsureContentRoot();
            ClearRows();

            var controllers = FindObjectsOfType<BarrierUpgradeController>();
            var inventory = inventorySource != null ? inventorySource.State : null;

            foreach (var controller in controllers)
            {
                if (controller == null || !controller.isActiveAndEnabled)
                {
                    continue;
                }

                if (inventory != null)
                {
                    controller.SetInventory(inventory);
                }

                if (feedbackChannel != null)
                {
                    controller.SetFeedbackChannel(feedbackChannel);
                }

                rows.Add(CreateRow(controller));
            }
        }

        private void ResolveReferences()
        {
            if (menuController == null)
            {
                menuController = FindObjectOfType<UpgradeMenuController>();
            }

            if (inventorySource == null)
            {
                inventorySource = FindObjectOfType<InventoryStateComponent>();
            }
        }

        private void EnsureContentRoot()
        {
            if (contentRoot != null)
            {
                return;
            }

            if (menuController == null)
            {
                return;
            }

            var panel = menuController.EnsureMenuRoot();
            var container = new GameObject("UpgradeList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            container.transform.SetParent(panel, false);

            contentRoot = container.GetComponent<RectTransform>();
            contentRoot.anchorMin = new Vector2(0f, 0f);
            contentRoot.anchorMax = new Vector2(1f, 1f);
            contentRoot.offsetMin = new Vector2(16f, 16f);
            contentRoot.offsetMax = new Vector2(-16f, -16f);

            var layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = container.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private Row CreateRow(BarrierUpgradeController controller)
        {
            var rowObject = new GameObject("UpgradeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            rowObject.transform.SetParent(contentRoot, false);

            var layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            var fitter = rowObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var nameText = CreateText("Name", rowObject.transform, 18, TextAlignmentOptions.Left);
            var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.preferredWidth = 140f;
            nameLayout.flexibleWidth = 0f;

            var detailText = CreateText("Details", rowObject.transform, 16, TextAlignmentOptions.Left);
            var detailLayout = detailText.gameObject.AddComponent<LayoutElement>();
            detailLayout.preferredWidth = 260f;
            detailLayout.flexibleWidth = 1f;
            var button = CreateButton("UpgradeButton", rowObject.transform);

            var row = new Row(controller, rowObject, nameText, detailText, button, Refresh);
            row.Refresh();
            return row;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, int fontSize, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableWordWrapping = false;

            return text;
        }

        private Button CreateButton(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            var button = go.GetComponent<Button>();

            var text = CreateText("Label", go.transform, 16, TextAlignmentOptions.Center);
            text.text = "Upgrade";

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90f, 28f);
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = 90f;
            layout.flexibleWidth = 0f;

            return button;
        }

        private void ClearRows()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].Dispose();
            }
            rows.Clear();
        }

        private class Row
        {
            private readonly BarrierUpgradeController controller;
            private readonly GameObject root;
            private readonly TextMeshProUGUI nameText;
            private readonly TextMeshProUGUI detailText;
            private readonly Button upgradeButton;
            private readonly System.Action refreshAll;

            public Row(
                BarrierUpgradeController controller,
                GameObject root,
                TextMeshProUGUI nameText,
                TextMeshProUGUI detailText,
                Button upgradeButton,
                System.Action refreshAll)
            {
                this.controller = controller;
                this.root = root;
                this.nameText = nameText;
                this.detailText = detailText;
                this.upgradeButton = upgradeButton;
                this.refreshAll = refreshAll;

                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            public void Refresh()
            {
                nameText.text = controller.name;
                detailText.text = $"Tier {controller.GetCurrentTier()} | HP {controller.GetCurrentHealth()}/{controller.GetCurrentMaxHealth()} | Cost {controller.GetUpgradeCost()}";
            }

            public void Dispose()
            {
                if (upgradeButton != null)
                {
                    upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
                }

                if (root != null)
                {
                    Object.Destroy(root);
                }
            }

            private void OnUpgradeClicked()
            {
                controller.TryUpgrade();
                refreshAll?.Invoke();
            }
        }
    }
}

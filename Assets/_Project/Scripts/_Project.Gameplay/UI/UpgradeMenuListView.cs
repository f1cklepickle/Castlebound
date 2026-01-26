using System.Collections;
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
        [SerializeField] private Color normalButtonColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        [SerializeField] private Color hoverButtonColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
        [SerializeField] private Color pressedButtonColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        [SerializeField] private Color disabledButtonColor = new Color(0.15f, 0.15f, 0.15f, 0.4f);
        [SerializeField] private Color successFlashColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color deniedFlashColor = new Color(0.85f, 0.2f, 0.2f, 1f);
        [SerializeField] private float flashDurationSeconds = 0.15f;
        [SerializeField] private Sprite buttonSprite;

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

            if (menuController.MenuRoot != null)
            {
                var found = menuController.MenuRoot.Find("UpgradeMenuContent");
                if (found != null)
                {
                    contentRoot = found as RectTransform;
                }
            }

            if (contentRoot == null)
            {
                Debug.LogWarning("UpgradeMenuListView: Content root is not assigned.", this);
                return;
            }

            ConfigureContentRootLayout();
        }

        private void ConfigureContentRootLayout()
        {
            var layout = contentRoot.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = contentRoot.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
            }

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

            var row = new Row(controller, rowObject, nameText, detailText, button, Refresh, FlashButton);
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
            if (buttonSprite == null)
            {
                buttonSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = normalButtonColor;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = normalButtonColor;
            colors.highlightedColor = hoverButtonColor;
            colors.pressedColor = pressedButtonColor;
            colors.selectedColor = hoverButtonColor;
            colors.disabledColor = disabledButtonColor;
            button.colors = colors;

            var text = CreateText("Label", go.transform, 16, TextAlignmentOptions.Center);
            text.text = "Upgrade";

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(90f, 28f);
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = 90f;
            layout.preferredHeight = 28f;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            return button;
        }

        private void FlashButton(Button button, bool success, System.Action onComplete)
        {
            if (button == null)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(FlashRoutine(button, success ? successFlashColor : deniedFlashColor, onComplete));
        }

        private IEnumerator FlashRoutine(Button button, Color flashColor, System.Action onComplete)
        {
            if (button == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            var originalTransition = button.transition;
            var originalColors = button.colors;
            var image = button.GetComponent<Image>();
            var originalImageColor = image != null ? image.color : Color.white;

            button.transition = Selectable.Transition.None;
            if (image != null)
            {
                image.color = flashColor;
            }

            yield return new WaitForSeconds(flashDurationSeconds);

            if (button != null)
            {
                button.transition = originalTransition;
                button.colors = originalColors;
                if (image != null)
                {
                    image.color = originalImageColor;
                }
            }

            onComplete?.Invoke();
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
            private readonly System.Action<Button, bool, System.Action> flashButton;

            public Row(
                BarrierUpgradeController controller,
                GameObject root,
                TextMeshProUGUI nameText,
                TextMeshProUGUI detailText,
                Button upgradeButton,
                System.Action refreshAll,
                System.Action<Button, bool, System.Action> flashButton)
            {
                this.controller = controller;
                this.root = root;
                this.nameText = nameText;
                this.detailText = detailText;
                this.upgradeButton = upgradeButton;
                this.refreshAll = refreshAll;
                this.flashButton = flashButton;

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
                bool upgraded = controller.TryUpgrade();
                if (flashButton != null)
                {
                    flashButton.Invoke(upgradeButton, upgraded, refreshAll);
                }
                else
                {
                    refreshAll?.Invoke();
                }
            }
        }

    }
}

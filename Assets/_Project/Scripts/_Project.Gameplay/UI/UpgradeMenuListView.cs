using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Tower;
using Castlebound.Gameplay.World.Placement;
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
        [SerializeField] private TowerBuildController towerBuildController;
        [SerializeField] private WorldPlaceablePlacementController bearTrapPlacementController;
        [SerializeField] private PlaceableObjectDefinition bearTrapDefinition;
        [SerializeField] private UpgradeMenuTab activeTab = UpgradeMenuTab.Castle;

        private readonly List<ActionRow> rows = new List<ActionRow>();
        private RectTransform rowRoot;
        private UpgradeMenuTabStrip tabStrip;

        private static readonly TowerUpgradeTrack[] TowerUpgradeTracks =
        {
            TowerUpgradeTrack.Damage,
            TowerUpgradeTrack.FireRate,
            TowerUpgradeTrack.Health,
            TowerUpgradeTrack.Range
        };

        public UpgradeMenuTab ActiveTab => activeTab;

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
            EnsureTabControls();
            EnsureRowRoot();
            ClearRows();

            var controllers = FindObjectsOfType<BarrierUpgradeController>();
            var inventory = inventorySource != null ? inventorySource.State : null;

            if (activeTab == UpgradeMenuTab.Defense)
            {
                CreateBearTrapPlacementRow();
            }

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

                if (activeTab == UpgradeMenuTab.Castle)
                {
                    rows.Add(CreateBarrierUpgradeRow(controller));
                }
                else
                {
                    CreateTowerPlotRows(controller);
                }
            }

            RefreshTabButtons();
        }

        public void SetContentRoot(RectTransform root)
        {
            contentRoot = root;
        }

        public void SetTowerBuildController(TowerBuildController controller)
        {
            towerBuildController = controller;
        }

        public void SetBearTrapPlacementController(WorldPlaceablePlacementController controller)
        {
            bearTrapPlacementController = controller;
        }

        public void SetBearTrapDefinition(PlaceableObjectDefinition definition)
        {
            bearTrapDefinition = definition;
        }

        public void SetActiveTab(UpgradeMenuTab tab)
        {
            if (activeTab == tab)
            {
                return;
            }

            activeTab = tab;
            Refresh();
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
                if (inventorySource == null)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    inventorySource = player != null ? player.GetComponent<InventoryStateComponent>() : null;
                }
            }

            if (towerBuildController == null)
            {
                towerBuildController = FindObjectOfType<TowerBuildController>();
            }

            if (bearTrapPlacementController == null)
            {
                bearTrapPlacementController = FindObjectOfType<WorldPlaceablePlacementController>();
            }

            if (bearTrapDefinition == null && bearTrapPlacementController != null)
            {
                bearTrapDefinition = bearTrapPlacementController.DefaultPlaceable;
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

            layout.spacing = 6f;
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

        private void EnsureTabControls()
        {
            if (contentRoot == null)
            {
                return;
            }

            if (tabStrip == null)
            {
                tabStrip = new UpgradeMenuTabStrip(CreateButton, SetActiveTab);
            }

            tabStrip.Ensure(contentRoot);
            RefreshTabButtons();
        }

        private void RefreshTabButtons()
        {
            tabStrip?.Refresh(activeTab, normalButtonColor, hoverButtonColor);
        }

        private void EnsureRowRoot()
        {
            if (contentRoot == null || rowRoot != null)
            {
                return;
            }

            var rowObject = new GameObject("UpgradeMenuRows", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            rowObject.transform.SetParent(contentRoot, false);
            rowRoot = rowObject.GetComponent<RectTransform>();

            var layout = rowObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var fitter = rowObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private ActionRow CreateBarrierUpgradeRow(BarrierUpgradeController controller)
        {
            var rowObject = new GameObject("UpgradeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            rowObject.transform.SetParent(rowRoot, false);
            ConfigureRowLayout(rowObject, 0f, 35f);

            var nameText = CreateText("Name", rowObject.transform, 22, TextAlignmentOptions.Left);
            ConfigureLayoutElement(nameText.gameObject, 188f, 0f);

            var detailText = CreateText("Details", rowObject.transform, 20, TextAlignmentOptions.Left);
            ConfigureLayoutElement(detailText.gameObject, 425f, 0f);
            var button = CreateButton("UpgradeButton", rowObject.transform, "Upgrade", 120f, 18);

            var row = new ActionRow(
                rowObject,
                nameText,
                detailText,
                button,
                () => controller.name,
                () => $"Tier {controller.GetCurrentTier()} | HP {controller.GetCurrentHealth()}/{controller.GetCurrentMaxHealth()} | {controller.GetUpgradeCost()} Gold",
                () => "Upgrade",
                () => true,
                () => controller.TryUpgrade(),
                Refresh,
                FlashButton);
            row.Refresh();
            return row;
        }

        private void CreateTowerPlotRows(BarrierUpgradeController controller)
        {
            if (towerBuildController == null)
            {
                return;
            }

            var plots = controller.GetComponent<BarrierTowerPlotCollection>();
            if (plots == null || plots.PlotCount == 0)
            {
                return;
            }

            if (inventorySource != null)
            {
                towerBuildController.SetInventory(inventorySource.State);
            }

            if (feedbackChannel != null)
            {
                towerBuildController.SetFeedbackChannel(feedbackChannel);
            }

            for (int i = 0; i < plots.PlotCount; i++)
            {
                var plot = plots.Plots[i];
                if (plot == null)
                {
                    continue;
                }

                rows.Add(plot.IsOccupied
                    ? CreateTowerUpgradeRow(plot, i, i == plots.PlotCount - 1)
                    : CreateTowerPlotRow(plot, i, i == plots.PlotCount - 1));
            }
        }

        private void CreateBearTrapPlacementRow()
        {
            if (bearTrapDefinition == null)
            {
                return;
            }

            var rowObject = new GameObject("BearTrapPlacementRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            rowObject.transform.SetParent(rowRoot, false);
            ConfigureRowLayout(rowObject, 0f, 35f);

            var nameText = CreateText("Name", rowObject.transform, 16, TextAlignmentOptions.Left);
            ConfigureLayoutElement(nameText.gameObject, 150f, 0f);

            var detailText = CreateText("Details", rowObject.transform, 16, TextAlignmentOptions.Left);
            ConfigureLayoutElement(detailText.gameObject, 425f, 0f);
            var button = CreateButton("BearTrapButton", rowObject.transform, "Place", 100f, 18);

            var row = new ActionRow(
                rowObject,
                nameText,
                detailText,
                button,
                () => bearTrapDefinition.DisplayName,
                () => "Free | Outside ground | 1x1",
                () => "Place",
                () => bearTrapPlacementController != null && bearTrapDefinition.IsValid,
                BeginBearTrapPlacement,
                Refresh,
                FlashButton);
            row.Refresh();
            rows.Add(row);
        }

        private bool BeginBearTrapPlacement()
        {
            if (bearTrapPlacementController == null || bearTrapDefinition == null)
            {
                return false;
            }

            if (!bearTrapPlacementController.BeginPlacement(bearTrapDefinition, ReopenDefenseTabAfterPlacement))
            {
                return false;
            }

            menuController?.HideMenuForPlacement();
            return true;
        }

        private void ReopenDefenseTabAfterPlacement()
        {
            activeTab = UpgradeMenuTab.Defense;
            menuController?.ReopenMenuAfterPlacement();
            Refresh();
        }

        private ActionRow CreateTowerPlotRow(TowerPlot plot, int plotIndex, bool endsBarrierGroup)
        {
            var rowObject = new GameObject("TowerPlotRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            rowObject.transform.SetParent(rowRoot, false);
            if (endsBarrierGroup)
            {
                var groupSpacing = rowObject.AddComponent<LayoutElement>();
                groupSpacing.minHeight = 60f;
                groupSpacing.preferredHeight = 60f;
            }

            ConfigureRowLayout(rowObject, 0f, 35f);

            var nameText = CreateText("Name", rowObject.transform, 16, TextAlignmentOptions.Left);
            ConfigureLayoutElement(nameText.gameObject, 150f, 0f);

            var detailText = CreateText("Details", rowObject.transform, 16, TextAlignmentOptions.Left);
            ConfigureLayoutElement(detailText.gameObject, 425f, 0f);
            var button = CreateButton("BuildButton", rowObject.transform, "Build", 100f, 18);

            var row = new ActionRow(
                rowObject,
                nameText,
                detailText,
                button,
                () => $"- {GetPlotLabel(plotIndex)}",
                () => GetTowerPlotDetails(plot),
                () => plot.IsOccupied ? "Occupied" : "Build",
                () => !plot.IsOccupied,
                () => towerBuildController.TryBuild(plot) == TowerBuildResult.Success,
                Refresh,
                FlashButton);
            row.Refresh();
            return row;
        }

        private ActionRow CreateTowerUpgradeRow(TowerPlot plot, int plotIndex, bool endsBarrierGroup)
        {
            var rowObject = new GameObject("TowerUpgradeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            rowObject.transform.SetParent(rowRoot, false);
            if (endsBarrierGroup)
            {
                var groupSpacing = rowObject.AddComponent<LayoutElement>();
                groupSpacing.minHeight = 60f;
                groupSpacing.preferredHeight = 60f;
            }

            ConfigureRowLayout(rowObject, 0f, 12f);

            var nameText = CreateText("Name", rowObject.transform, 16, TextAlignmentOptions.Left);
            ConfigureLayoutElement(nameText.gameObject, 150f, 0f);

            var detailText = CreateText("Details", rowObject.transform, 16, TextAlignmentOptions.Left);
            ConfigureLayoutElement(detailText.gameObject, 325f, 0f);

            var upgradeController = GetTowerUpgradeController(plot);
            var bindings = new List<TowerUpgradeButtonBinding>();
            if (upgradeController != null)
            {
                if (inventorySource != null)
                {
                    upgradeController.SetInventory(inventorySource.State);
                }

                if (feedbackChannel != null)
                {
                    upgradeController.SetFeedbackChannel(feedbackChannel);
                }

                if (towerBuildController != null && towerBuildController.PhaseTracker != null)
                {
                    upgradeController.SetPhaseTracker(towerBuildController.PhaseTracker);
                }
            }

            foreach (var track in TowerUpgradeTracks)
            {
                if (upgradeController == null || upgradeController.Config == null || !upgradeController.Config.IsTrackEnabled(track))
                {
                    continue;
                }

                var trackButton = CreateButton($"{track}Button", rowObject.transform, GetTrackLabel(track), 85f, 16);
                var capturedTrack = track;
                trackButton.onClick.AddListener(() =>
                {
                    bool succeeded = upgradeController.TryUpgrade(capturedTrack);
                    FlashButton(trackButton, succeeded, Refresh);
                });
                bindings.Add(new TowerUpgradeButtonBinding(trackButton, track));
            }

            var row = new ActionRow(
                rowObject,
                nameText,
                detailText,
                null,
                () => $"- {GetPlotLabel(plotIndex)}",
                () => GetTowerPlotDetails(plot),
                null,
                null,
                null,
                Refresh,
                FlashButton,
                bindings,
                () => RefreshTowerUpgradeButtons(bindings, upgradeController));
            row.Refresh();
            return row;
        }

        private static void ConfigureRowLayout(GameObject rowObject, float leftPadding, float spacing)
        {
            var layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding.left = Mathf.RoundToInt(leftPadding);
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            var fitter = rowObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void ConfigureLayoutElement(GameObject target, float preferredWidth, float flexibleWidth)
        {
            var layout = target.AddComponent<LayoutElement>();
            layout.preferredWidth = preferredWidth;
            layout.minWidth = preferredWidth;
            layout.flexibleWidth = flexibleWidth;
        }

        private string GetTowerPlotDetails(TowerPlot plot)
        {
            var config = towerBuildController != null ? towerBuildController.Config : null;
            string towerName = config != null && config.TowerPrefab != null ? config.TowerPrefab.name : "Tower";
            int maxHealth = config != null ? config.BaseMaxHealth : 0;
            int damage = config != null ? config.BaseDamage : 0;
            int buildCost = config != null ? config.BuildCost : 0;
            int upgradeCost = config != null ? config.BaseUpgradeCost : 0;

            if (plot != null && plot.IsOccupied)
            {
                var runtime = plot.OccupantInstance != null ? plot.OccupantInstance.GetComponent<TowerRuntime>() : null;
                var attack = plot.OccupantInstance != null ? plot.OccupantInstance.GetComponent<TowerAttackController>() : null;
                var targeting = plot.OccupantInstance != null ? plot.OccupantInstance.GetComponent<TowerTargetingController>() : null;
                int currentHealth = runtime != null ? runtime.CurrentHealth : maxHealth;
                int occupiedMaxHealth = runtime != null ? runtime.MaxHealth : maxHealth;
                int currentDamage = attack != null ? attack.Damage : damage;
                float cooldown = attack != null ? attack.CooldownSeconds : 0f;
                float range = targeting != null ? targeting.MaxRange : 0f;
                return $"{towerName} | HP {currentHealth}/{occupiedMaxHealth} | DMG {currentDamage} | RATE {cooldown:0.##} | RNG {range:0.#}";
            }

            return $"{towerName} | HP {maxHealth} | DMG {damage} | Build {buildCost} Gold";
        }

        private static string GetPlotLabel(int plotIndex)
        {
            if (plotIndex == 0)
            {
                return "Left Plot";
            }

            if (plotIndex == 1)
            {
                return "Right Plot";
            }

            return $"Plot {plotIndex + 1}";
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

        private Button CreateButton(string name, Transform parent, string label, float width, int fontSize = 16)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = buttonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
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

            var text = CreateText("Label", go.transform, fontSize, TextAlignmentOptions.Center);
            text.text = label;
            text.raycastTarget = false;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 35f);
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredWidth = width;
            layout.preferredHeight = 35f;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            return button;
        }

        private static string GetTrackLabel(TowerUpgradeTrack track)
        {
            return track switch
            {
                TowerUpgradeTrack.Damage => "DMG",
                TowerUpgradeTrack.FireRate => "RATE",
                TowerUpgradeTrack.Health => "HP",
                TowerUpgradeTrack.Range => "RNG",
                _ => string.Empty
            };
        }

        private static TowerUpgradeController GetTowerUpgradeController(TowerPlot plot)
        {
            return plot != null && plot.OccupantInstance != null
                ? plot.OccupantInstance.GetComponent<TowerUpgradeController>()
                : null;
        }

        private static void RefreshTowerUpgradeButtons(IReadOnlyList<TowerUpgradeButtonBinding> bindings, TowerUpgradeController upgradeController)
        {
            if (bindings == null || upgradeController == null)
            {
                return;
            }

            for (int i = 0; i < bindings.Count; i++)
            {
                var button = bindings[i].Button;
                var track = bindings[i].Track;
                if (button == null)
                {
                    continue;
                }

                button.interactable = upgradeController.CanUpgrade(track);
                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = GetTrackButtonText(upgradeController, track);
                }
            }
        }

        private static string GetTrackButtonText(TowerUpgradeController upgradeController, TowerUpgradeTrack track)
        {
            if (upgradeController == null)
            {
                return GetTrackLabel(track);
            }

            if (!upgradeController.CanUpgrade(track))
            {
                return $"{GetTrackLabel(track)} MAX";
            }

            return $"{GetTrackLabel(track)} {upgradeController.GetUpgradeCost(track)}";
        }

        private void FlashButton(Button button, bool success, System.Action onComplete)
        {
            if (button == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (!Application.isPlaying)
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

        private readonly struct TowerUpgradeButtonBinding
        {
            public TowerUpgradeButtonBinding(Button button, TowerUpgradeTrack track)
            {
                Button = button;
                Track = track;
            }

            public Button Button { get; }
            public TowerUpgradeTrack Track { get; }
        }

        private class ActionRow
        {
            private readonly GameObject root;
            private readonly TextMeshProUGUI nameText;
            private readonly TextMeshProUGUI detailText;
            private readonly Button actionButton;
            private readonly System.Func<string> getName;
            private readonly System.Func<string> getDetails;
            private readonly System.Func<string> getButtonLabel;
            private readonly System.Func<bool> canInvoke;
            private readonly System.Func<bool> invoke;
            private readonly System.Action refreshAll;
            private readonly System.Action<Button, bool, System.Action> flashButton;
            private readonly List<TowerUpgradeButtonBinding> extraButtons;
            private readonly System.Action refreshExtraButtons;

            public ActionRow(
                GameObject root,
                TextMeshProUGUI nameText,
                TextMeshProUGUI detailText,
                Button actionButton,
                System.Func<string> getName,
                System.Func<string> getDetails,
                System.Func<string> getButtonLabel,
                System.Func<bool> canInvoke,
                System.Func<bool> invoke,
                System.Action refreshAll,
                System.Action<Button, bool, System.Action> flashButton,
                List<TowerUpgradeButtonBinding> extraButtons = null,
                System.Action refreshExtraButtons = null)
            {
                this.root = root;
                this.nameText = nameText;
                this.detailText = detailText;
                this.actionButton = actionButton;
                this.getName = getName;
                this.getDetails = getDetails;
                this.getButtonLabel = getButtonLabel;
                this.canInvoke = canInvoke;
                this.invoke = invoke;
                this.refreshAll = refreshAll;
                this.flashButton = flashButton;
                this.extraButtons = extraButtons;
                this.refreshExtraButtons = refreshExtraButtons;

                if (actionButton != null)
                {
                    actionButton.onClick.AddListener(OnClicked);
                }
            }

            public void Refresh()
            {
                nameText.text = getName != null ? getName.Invoke() : string.Empty;
                detailText.text = getDetails != null ? getDetails.Invoke() : string.Empty;
                if (actionButton != null)
                {
                    actionButton.interactable = canInvoke == null || canInvoke.Invoke();

                    var label = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = getButtonLabel != null ? getButtonLabel.Invoke() : string.Empty;
                    }
                }

                refreshExtraButtons?.Invoke();
            }

            public void Dispose()
            {
                if (actionButton != null)
                {
                    actionButton.onClick.RemoveListener(OnClicked);
                }

                if (extraButtons != null)
                {
                    foreach (var binding in extraButtons)
                    {
                        binding.Button?.onClick.RemoveAllListeners();
                    }
                }

                if (root != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(root);
                    }
                    else
                    {
                        Object.DestroyImmediate(root);
                    }
                }
            }

            private void OnClicked()
            {
                bool succeeded = invoke != null && invoke.Invoke();
                if (flashButton != null)
                {
                    flashButton.Invoke(actionButton, succeeded, refreshAll);
                }
                else
                {
                    refreshAll?.Invoke();
                }
            }
        }

    }
}

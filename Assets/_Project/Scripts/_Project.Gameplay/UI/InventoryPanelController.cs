using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class InventoryPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button backpackTabButton;
        [SerializeField] private Button vaultTabButton;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private BackpackInventoryStateComponent backpackSource;
        [SerializeField] private CastleInventoryStateComponent castleInventorySource;
        [SerializeField] private EnemySpawnerRunner waveRunner;

        private BackpackInventoryState backpack;
        private CastleInventoryState vault;
        private WavePhaseTracker phaseTracker;
        private RectTransform tabRoot;
        private InventoryPanelTab activeTab = InventoryPanelTab.Backpack;

        public bool IsOpen => panelRoot != null && panelRoot.gameObject.activeSelf;
        public InventoryPanelTab ActiveTab => activeTab;
        public Button OpenButton => openButton;
        public Button BackpackTabButton => backpackTabButton;
        public Button VaultTabButton => vaultTabButton;

        private void OnEnable()
        {
            ResolveReferences();
            EnsureRuntimeUi();
            HookSources();
            ApplyPanelState(false);
            Refresh();
        }

        private void OnDisable()
        {
            UnhookSources();
        }

        public void SetBackpackSource(BackpackInventoryStateComponent source)
        {
            if (backpackSource == source)
            {
                return;
            }

            UnhookBackpack();
            backpackSource = source;
            backpack = backpackSource != null ? backpackSource.State : null;
            HookBackpack();
            Refresh();
        }

        public void SetCastleInventorySource(CastleInventoryStateComponent source)
        {
            if (castleInventorySource == source)
            {
                return;
            }

            UnhookVault();
            castleInventorySource = source;
            vault = castleInventorySource != null ? castleInventorySource.State : null;
            HookVault();
            Refresh();
        }

        public void SetPhaseTracker(WavePhaseTracker tracker)
        {
            if (phaseTracker == tracker)
            {
                return;
            }

            UnhookPhaseTracker();
            phaseTracker = tracker;
            HookPhaseTracker();
            Refresh();
        }

        public void SetWaveRunner(EnemySpawnerRunner runner)
        {
            waveRunner = runner;
            SetPhaseTracker(waveRunner != null ? waveRunner.PhaseTracker : null);
        }

        public void TogglePanel()
        {
            ApplyPanelState(!IsOpen);
            if (IsOpen)
            {
                SetActiveTab(InventoryPanelTab.Backpack);
            }
        }

        public void ClosePanel()
        {
            ApplyPanelState(false);
        }

        public void SetActiveTab(InventoryPanelTab tab)
        {
            if (tab == InventoryPanelTab.Vault && !IsVaultAccessible())
            {
                activeTab = InventoryPanelTab.Backpack;
            }
            else
            {
                activeTab = tab;
            }

            Refresh();
        }

        public void Refresh()
        {
            EnsureRuntimeUi();
            RefreshRows();
            RefreshTabButtons();
        }

        private void ResolveReferences()
        {
            if (backpackSource == null)
            {
                backpackSource = FindObjectOfType<BackpackInventoryStateComponent>();
            }

            if (castleInventorySource == null)
            {
                castleInventorySource = FindObjectOfType<CastleInventoryStateComponent>();
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            backpack = backpackSource != null ? backpackSource.State : null;
            vault = castleInventorySource != null ? castleInventorySource.State : null;
            phaseTracker = waveRunner != null ? waveRunner.PhaseTracker : phaseTracker;
        }

        private void HookSources()
        {
            HookBackpack();
            HookVault();
            HookPhaseTracker();
        }

        private void UnhookSources()
        {
            UnhookBackpack();
            UnhookVault();
            UnhookPhaseTracker();
        }

        private void HookBackpack()
        {
            if (backpack != null)
            {
                backpack.OnInventoryChanged += Refresh;
            }
        }

        private void UnhookBackpack()
        {
            if (backpack != null)
            {
                backpack.OnInventoryChanged -= Refresh;
            }
        }

        private void HookVault()
        {
            if (vault != null)
            {
                vault.OnInventoryChanged += Refresh;
            }
        }

        private void UnhookVault()
        {
            if (vault != null)
            {
                vault.OnInventoryChanged -= Refresh;
            }
        }

        private void HookPhaseTracker()
        {
            if (phaseTracker != null)
            {
                phaseTracker.PhaseChanged += OnPhaseChanged;
            }
        }

        private void UnhookPhaseTracker()
        {
            if (phaseTracker != null)
            {
                phaseTracker.PhaseChanged -= OnPhaseChanged;
            }
        }

        private void OnPhaseChanged(WavePhase phase)
        {
            if (phase == WavePhase.InWave && activeTab == InventoryPanelTab.Vault)
            {
                activeTab = InventoryPanelTab.Backpack;
            }

            Refresh();
        }

        private bool IsVaultAccessible()
        {
            return phaseTracker == null || phaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private void ApplyPanelState(bool open)
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(open);
            }
        }

        private void EnsureRuntimeUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            var parent = canvas != null ? canvas.transform : transform;

            if (openButton == null)
            {
                openButton = CreateButton("InventoryOpenButton", parent, "Bag", new Vector2(56f, 48f), 17);
                var rect = openButton.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(20f, -20f);
                openButton.onClick.AddListener(TogglePanel);
            }

            if (panelRoot == null)
            {
                panelRoot = CreatePanel(parent);
            }

            if (tabRoot == null)
            {
                tabRoot = CreateTabRoot(panelRoot);
            }

            if (backpackTabButton == null)
            {
                backpackTabButton = CreateButton("BackpackTab", tabRoot, "Backpack", new Vector2(120f, 38f), 16);
                backpackTabButton.onClick.AddListener(() => SetActiveTab(InventoryPanelTab.Backpack));
            }

            if (vaultTabButton == null)
            {
                vaultTabButton = CreateButton("VaultTab", tabRoot, "Vault", new Vector2(120f, 38f), 16);
                vaultTabButton.onClick.AddListener(() => SetActiveTab(InventoryPanelTab.Vault));
            }

            if (contentRoot == null)
            {
                var rows = new GameObject("InventoryRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
                rows.transform.SetParent(panelRoot, false);
                contentRoot = rows.GetComponent<RectTransform>();
                contentRoot.anchorMin = new Vector2(0f, 0f);
                contentRoot.anchorMax = new Vector2(1f, 1f);
                contentRoot.offsetMin = new Vector2(14f, 14f);
                contentRoot.offsetMax = new Vector2(-14f, -64f);

                var layout = rows.GetComponent<VerticalLayoutGroup>();
                layout.spacing = 6f;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
            }
        }

        private RectTransform CreatePanel(Transform parent)
        {
            var panel = new GameObject("InventoryPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.SetActive(false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -78f);
            rect.sizeDelta = new Vector2(360f, 310f);

            var image = panel.GetComponent<Image>();
            image.color = new Color(0.08f, 0.09f, 0.1f, 0.92f);
            image.raycastTarget = false;

            return rect;
        }

        private static RectTransform CreateTabRoot(Transform parent)
        {
            var tabObject = new GameObject("InventoryTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabObject.transform.SetParent(parent, false);

            var rect = tabObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(14f, -52f);
            rect.offsetMax = new Vector2(-14f, -14f);

            var layout = tabObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = false;
            layout.childControlWidth = false;

            return rect;
        }

        private void RefreshTabButtons()
        {
            if (backpackTabButton != null)
            {
                backpackTabButton.interactable = activeTab != InventoryPanelTab.Backpack;
            }

            if (vaultTabButton != null)
            {
                vaultTabButton.interactable = activeTab != InventoryPanelTab.Vault && IsVaultAccessible();
            }
        }

        private void RefreshRows()
        {
            if (contentRoot == null)
            {
                return;
            }

            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                DestroyChild(contentRoot.GetChild(i).gameObject);
            }

            if (activeTab == InventoryPanelTab.Vault && !IsVaultAccessible())
            {
                activeTab = InventoryPanelTab.Backpack;
            }

            if (activeTab == InventoryPanelTab.Backpack)
            {
                CreateBackpackRows();
            }
            else
            {
                CreateVaultRows();
            }
        }

        private void CreateBackpackRows()
        {
            if (backpack == null || backpack.EntryCount == 0)
            {
                CreateTextRow("Backpack is empty");
                return;
            }

            foreach (var entry in backpack.Entries)
            {
                CreateTextRow($"{entry.ItemId} x{entry.Count}");
            }
        }

        private void CreateVaultRows()
        {
            if (!IsVaultAccessible())
            {
                CreateTextRow("Vault opens between waves");
                return;
            }

            if (vault == null || vault.EntryCount == 0)
            {
                CreateTextRow("Vault is empty");
                return;
            }

            foreach (var entry in vault.Entries)
            {
                CreateTextRow($"{entry.ItemId} x{entry.Count}");
            }
        }

        private void CreateTextRow(string value)
        {
            var label = new GameObject("InventoryRow", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(contentRoot, false);

            var rect = label.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 28f);

            var text = label.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 size, int fontSize)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.minWidth = size.x;
            layout.minHeight = size.y;
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.17f, 0.2f, 0.95f);

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.15f, 0.17f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.24f, 0.27f, 0.31f, 1f);
            colors.pressedColor = new Color(0.1f, 0.12f, 0.15f, 1f);
            colors.disabledColor = new Color(0.12f, 0.13f, 0.15f, 0.45f);
            button.colors = colors;

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            text.enableAutoSizing = true;
            text.fontSizeMin = 11;
            text.fontSizeMax = fontSize;

            return button;
        }

        private static void DestroyChild(GameObject child)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(child);
            }
            else
            {
                Object.DestroyImmediate(child);
            }
        }
    }
}

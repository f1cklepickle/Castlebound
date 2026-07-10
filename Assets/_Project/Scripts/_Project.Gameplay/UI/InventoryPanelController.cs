using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.Combat;
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
        [SerializeField] private InventoryStateComponent activeInventorySource;
        [SerializeField] private CastleInventoryStateComponent castleInventorySource;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private CastleRegionTracker castleRegionTracker;
        [SerializeField] private InventoryContextMenuController contextMenu;
        [SerializeField] private BackpackWeaponEquipController equipController;
        [SerializeField] private BackpackItemDropController dropController;
        [SerializeField] private MonoBehaviour weaponDefinitionResolverSource;

        private BackpackInventoryState backpack;
        private CastleInventoryState vault;
        private WavePhaseTracker phaseTracker;
        private IWeaponDefinitionResolver weaponDefinitionResolver;
        private RectTransform tabRoot;
        private InventoryPanelTab activeTab = InventoryPanelTab.Backpack;
        private bool contextMenuHooked;
        private bool castleRegionHooked;
        private bool vaultOpenedFromWorld;

        public bool IsOpen => panelRoot != null && panelRoot.gameObject.activeSelf;
        public InventoryPanelTab ActiveTab => activeTab;
        public Button OpenButton => openButton;
        public Button BackpackTabButton => backpackTabButton;
        public Button VaultTabButton => vaultTabButton;
        public Button ShopTabButton => vaultTabButton;

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
            UnhookContextMenu();
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

        public void SetActiveInventorySource(InventoryStateComponent source)
        {
            activeInventorySource = source;
            if (equipController != null)
            {
                equipController.SetActiveInventorySource(activeInventorySource);
            }
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

        public void SetCastleRegionTracker(CastleRegionTracker tracker)
        {
            if (castleRegionTracker == tracker)
            {
                return;
            }

            UnhookCastleRegionTracker();
            castleRegionTracker = tracker;
            HookCastleRegionTracker();
            HandleShopAccessChanged();
        }

        public void SetWeaponDefinitionResolver(IWeaponDefinitionResolver resolver)
        {
            weaponDefinitionResolver = resolver;
            weaponDefinitionResolverSource = resolver as MonoBehaviour;
            if (dropController != null)
            {
                dropController.SetWeaponDefinitionResolver(resolver);
            }
        }

        public void TogglePanel()
        {
            ApplyPanelState(!IsOpen);
            vaultOpenedFromWorld = false;
            if (IsOpen)
            {
                if (contextMenu != null)
                {
                    contextMenu.Close();
                }

                SetActiveTab(InventoryPanelTab.Backpack);
            }
        }

        public bool OpenVaultFromWorld()
        {
            if (!IsVaultAccessible())
            {
                return false;
            }

            EnsureRuntimeUi();
            vaultOpenedFromWorld = true;
            if (contextMenu != null && contextMenu.ActiveSource != InventoryContextSource.Vault)
            {
                contextMenu.Close();
            }

            ApplyPanelState(true);
            Refresh();
            return true;
        }

        public void ClosePanel()
        {
            if (contextMenu != null)
            {
                contextMenu.Close();
            }

            vaultOpenedFromWorld = false;
            ApplyPanelState(false);
        }

        public void SetActiveTab(InventoryPanelTab tab)
        {
            vaultOpenedFromWorld = false;
            if (contextMenu != null)
            {
                contextMenu.Close();
            }

            activeTab = tab;
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

            if (activeInventorySource == null)
            {
                activeInventorySource = FindObjectOfType<InventoryStateComponent>();
            }

            if (castleInventorySource == null)
            {
                castleInventorySource = FindObjectOfType<CastleInventoryStateComponent>();
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            if (castleRegionTracker == null)
            {
                castleRegionTracker = CastleRegionTracker.Instance;
            }

            backpack = backpackSource != null ? backpackSource.State : null;
            vault = castleInventorySource != null ? castleInventorySource.State : null;
            phaseTracker = waveRunner != null ? waveRunner.PhaseTracker : phaseTracker;
            weaponDefinitionResolver = weaponDefinitionResolverSource as IWeaponDefinitionResolver ?? weaponDefinitionResolver ?? FindWeaponDefinitionResolver();
        }

        private void HookSources()
        {
            HookBackpack();
            HookVault();
            HookPhaseTracker();
            HookCastleRegionTracker();
            CastleRegionTracker.InstanceReady -= OnCastleRegionInstanceReady;
            CastleRegionTracker.InstanceReady += OnCastleRegionInstanceReady;
        }

        private void UnhookSources()
        {
            UnhookBackpack();
            UnhookVault();
            UnhookPhaseTracker();
            UnhookCastleRegionTracker();
            CastleRegionTracker.InstanceReady -= OnCastleRegionInstanceReady;
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
            if (phase == WavePhase.InWave && vaultOpenedFromWorld)
            {
                vaultOpenedFromWorld = false;
                activeTab = InventoryPanelTab.Backpack;
            }

            HandleShopAccessChanged();
            Refresh();
        }

        private void OnCastleRegionInstanceReady()
        {
            if (castleRegionTracker != null)
            {
                return;
            }

            SetCastleRegionTracker(CastleRegionTracker.Instance);
        }

        private void HookCastleRegionTracker()
        {
            if (castleRegionHooked || castleRegionTracker == null)
            {
                return;
            }

            castleRegionTracker.OnPlayerEntered += OnCastlePlayerRegionChanged;
            castleRegionTracker.OnPlayerExited += OnCastlePlayerRegionChanged;
            castleRegionHooked = true;
        }

        private void UnhookCastleRegionTracker()
        {
            if (!castleRegionHooked || castleRegionTracker == null)
            {
                castleRegionHooked = false;
                return;
            }

            castleRegionTracker.OnPlayerEntered -= OnCastlePlayerRegionChanged;
            castleRegionTracker.OnPlayerExited -= OnCastlePlayerRegionChanged;
            castleRegionHooked = false;
        }

        private void OnCastlePlayerRegionChanged()
        {
            HandleShopAccessChanged();
            Refresh();
        }

        private void HandleShopAccessChanged()
        {
            if (activeTab == InventoryPanelTab.Shop && IsOpen && !IsShopAccessible())
            {
                ClosePanel();
            }
        }

        private bool IsVaultAccessible()
        {
            return phaseTracker == null || phaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private bool IsShopAccessible()
        {
            return CastleShopAccessPolicy.CanOpen(
                castleRegionTracker != null,
                castleRegionTracker != null && castleRegionTracker.PlayerInside,
                phaseTracker != null ? phaseTracker.CurrentPhase : WavePhase.PreWave);
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
                vaultTabButton = CreateButton("ShopTab", tabRoot, "Shop", new Vector2(120f, 38f), 16);
                vaultTabButton.onClick.AddListener(() => SetActiveTab(InventoryPanelTab.Shop));
            }

            ConfigureShopTabButton();

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

            EnsureContextActionComponents();
        }

        private void EnsureContextActionComponents()
        {
            if (equipController == null)
            {
                equipController = GetComponent<BackpackWeaponEquipController>();
                if (equipController == null)
                {
                    equipController = gameObject.AddComponent<BackpackWeaponEquipController>();
                }
            }

            if (dropController == null)
            {
                dropController = GetComponent<BackpackItemDropController>();
                if (dropController == null)
                {
                    dropController = gameObject.AddComponent<BackpackItemDropController>();
                }
            }

            if (contextMenu == null)
            {
                contextMenu = GetComponent<InventoryContextMenuController>();
                if (contextMenu == null)
                {
                    contextMenu = gameObject.AddComponent<InventoryContextMenuController>();
                }
            }

            HookContextMenu();

            equipController.SetActiveInventorySource(activeInventorySource);
            equipController.SetBackpackSource(backpackSource);
            dropController.SetBackpackSource(backpackSource);
            dropController.SetDropOrigin(activeInventorySource != null ? activeInventorySource.transform : transform);
            dropController.SetWeaponDefinitionResolver(weaponDefinitionResolverSource as IWeaponDefinitionResolver ?? weaponDefinitionResolver ?? FindWeaponDefinitionResolver());
            contextMenu.SetParentRoot(panelRoot);
            contextMenu.SetEquipController(equipController);
            contextMenu.SetDropController(dropController);
            contextMenu.SetInventorySources(backpackSource, castleInventorySource, activeInventorySource);
        }

        private void HookContextMenu()
        {
            if (contextMenu == null || contextMenuHooked)
            {
                return;
            }

            contextMenu.ActionCompleted += Refresh;
            contextMenuHooked = true;
        }

        private void UnhookContextMenu()
        {
            if (contextMenu == null || !contextMenuHooked)
            {
                return;
            }

            contextMenu.ActionCompleted -= Refresh;
            contextMenuHooked = false;
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
                backpackTabButton.interactable = vaultOpenedFromWorld || activeTab != InventoryPanelTab.Backpack;
            }

            if (vaultTabButton != null)
            {
                vaultTabButton.interactable = true;
            }
        }

        private void ConfigureShopTabButton()
        {
            if (vaultTabButton == null)
            {
                return;
            }

            vaultTabButton.gameObject.name = "ShopTab";
            var label = vaultTabButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = "Shop";
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

            if (vaultOpenedFromWorld && !IsVaultAccessible())
            {
                vaultOpenedFromWorld = false;
                activeTab = InventoryPanelTab.Backpack;
            }

            if (vaultOpenedFromWorld)
            {
                CreateVaultRows();
            }
            else if (activeTab == InventoryPanelTab.Backpack)
            {
                CreateBackpackRows();
            }
            else
            {
                CreateShopRows();
            }

            if (contextMenu != null)
            {
                contextMenu.CloseIfActiveItemMissing();
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
                CreateBackpackRow(entry);
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
                CreateInventoryRow(entry.ItemId, entry.Count, InventoryContextSource.Vault);
            }
        }

        private void CreateShopRows()
        {
            if (!IsShopAccessible())
            {
                CreateTextRow("Shop opens inside castle between waves");
                return;
            }

            CreateTextRow("Castle Shop");
            foreach (var offer in CastleShopCatalog.DefaultOffers)
            {
                CreateTextRow(offer.DisplayName);
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

        private void CreateBackpackRow(BackpackInventoryEntry entry)
        {
            CreateInventoryRow(entry.ItemId, entry.Count, InventoryContextSource.Backpack);
        }

        private void CreateInventoryRow(string itemId, int count, InventoryContextSource source)
        {
            var rowObject = new GameObject("InventoryRow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            rowObject.transform.SetParent(contentRoot, false);

            var rect = rowObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 34f);

            var layout = rowObject.GetComponent<LayoutElement>();
            layout.minHeight = 34f;
            layout.preferredHeight = 34f;
            layout.flexibleWidth = 1f;

            var image = rowObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.04f);
            image.raycastTarget = true;

            var button = rowObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => contextMenu.ShowForItem(itemId, IsWeaponItem(itemId), source, rect));

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(rowObject.transform, false);

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);

            var text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = $"{itemId} x{count}";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;

            var trigger = rowObject.AddComponent<InventoryContextMenuTrigger>();
            trigger.Configure(contextMenu, itemId, IsWeaponItem(itemId), source);
        }

        private bool IsWeaponItem(string itemId)
        {
            IWeaponDefinitionResolver resolver = weaponDefinitionResolverSource as IWeaponDefinitionResolver ?? weaponDefinitionResolver ?? FindWeaponDefinitionResolver();
            return resolver != null && resolver.Resolve(itemId) != null;
        }

        private IWeaponDefinitionResolver FindWeaponDefinitionResolver()
        {
            var behaviours = FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IWeaponDefinitionResolver resolver)
                {
                    return resolver;
                }
            }

            return null;
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
                child.SetActive(false);
                Object.Destroy(child);
            }
            else
            {
                Object.DestroyImmediate(child);
            }
        }
    }
}

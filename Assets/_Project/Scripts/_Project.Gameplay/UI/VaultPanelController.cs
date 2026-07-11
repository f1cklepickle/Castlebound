using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class VaultPanelController : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private CastleInventoryStateComponent castleInventorySource;
        [SerializeField] private BackpackInventoryStateComponent backpackSource;
        [SerializeField] private InventoryStateComponent activeInventorySource;
        [SerializeField] private InventoryContextMenuController contextMenu;
        [SerializeField] private MonoBehaviour weaponDefinitionResolverSource;

        private CastleInventoryState vault;
        private WavePhaseTracker phaseTracker;
        private IWeaponDefinitionResolver weaponDefinitionResolver;
        private bool contextMenuHooked;

        public bool IsOpen => panelRoot != null && panelRoot.gameObject.activeSelf;

        private void OnEnable()
        {
            ResolveReferences();
            EnsureRuntimeUi();
            HookVault();
            HookPhaseTracker();
            ApplyPanelState(false);
            Refresh();
        }

        private void OnDisable()
        {
            UnhookVault();
            UnhookPhaseTracker();
            UnhookContextMenu();
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

        public void SetInventorySources(
            BackpackInventoryStateComponent backpack,
            CastleInventoryStateComponent castleInventory,
            InventoryStateComponent activeInventory)
        {
            backpackSource = backpack;
            activeInventorySource = activeInventory;
            SetCastleInventorySource(castleInventory);
            ConfigureContextMenu();
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

        public void SetWeaponDefinitionResolver(IWeaponDefinitionResolver resolver)
        {
            weaponDefinitionResolver = resolver;
            weaponDefinitionResolverSource = resolver as MonoBehaviour;
        }

        public bool OpenFromWorld()
        {
            if (!IsVaultAccessible())
            {
                return false;
            }

            EnsureRuntimeUi();
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

            ApplyPanelState(false);
        }

        public void Refresh()
        {
            EnsureRuntimeUi();
            RefreshRows();
        }

        private void ResolveReferences()
        {
            if (castleInventorySource == null)
            {
                castleInventorySource = FindObjectOfType<CastleInventoryStateComponent>();
            }

            if (backpackSource == null)
            {
                backpackSource = FindObjectOfType<BackpackInventoryStateComponent>();
            }

            if (activeInventorySource == null)
            {
                activeInventorySource = FindObjectOfType<InventoryStateComponent>();
            }

            vault = castleInventorySource != null ? castleInventorySource.State : null;
            weaponDefinitionResolver = weaponDefinitionResolverSource as IWeaponDefinitionResolver ?? weaponDefinitionResolver ?? FindWeaponDefinitionResolver();
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
            if (phase == WavePhase.InWave)
            {
                ClosePanel();
            }

            Refresh();
        }

        private bool IsVaultAccessible()
        {
            return phaseTracker == null || phaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private void EnsureRuntimeUi()
        {
            var canvas = GetComponentInParent<Canvas>();
            var parent = canvas != null ? canvas.transform : transform;

            if (panelRoot == null)
            {
                panelRoot = CreatePanel(parent);
            }

            if (contentRoot == null)
            {
                var rows = new GameObject("VaultRows", typeof(RectTransform), typeof(VerticalLayoutGroup));
                rows.transform.SetParent(panelRoot, false);
                contentRoot = rows.GetComponent<RectTransform>();
                contentRoot.anchorMin = Vector2.zero;
                contentRoot.anchorMax = Vector2.one;
                contentRoot.offsetMin = new Vector2(14f, 14f);
                contentRoot.offsetMax = new Vector2(-14f, -44f);

                var layout = rows.GetComponent<VerticalLayoutGroup>();
                layout.spacing = 6f;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
            }

            if (closeButton == null)
            {
                closeButton = CreateCloseButton(panelRoot);
                closeButton.onClick.AddListener(ClosePanel);
            }

            ConfigureContextMenu();
        }

        private void ConfigureContextMenu()
        {
            if (contextMenu == null)
            {
                contextMenu = GetComponent<InventoryContextMenuController>();
                if (contextMenu == null)
                {
                    contextMenu = gameObject.AddComponent<InventoryContextMenuController>();
                }
            }

            HookContextMenu();
            contextMenu.SetParentRoot(panelRoot);
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
            var panel = new GameObject("VaultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.SetActive(false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(392f, -78f);
            rect.sizeDelta = new Vector2(300f, 310f);

            var image = panel.GetComponent<Image>();
            image.color = new Color(0.08f, 0.09f, 0.1f, 0.92f);
            image.raycastTarget = false;

            CreateHeader(rect);
            return rect;
        }

        private static Button CreateCloseButton(Transform parent)
        {
            var buttonObject = new GameObject("VaultCloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-10f, -10f);
            rect.sizeDelta = new Vector2(30f, 30f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.17f, 0.2f, 0.95f);

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.15f, 0.17f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.24f, 0.27f, 0.31f, 1f);
            colors.pressedColor = new Color(0.1f, 0.12f, 0.15f, 1f);
            button.colors = colors;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = "X";
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            return button;
        }

        private static void CreateHeader(Transform parent)
        {
            var header = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            header.transform.SetParent(parent, false);

            var rect = header.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(14f, -38f);
            rect.offsetMax = new Vector2(-14f, -10f);

            var text = header.GetComponent<TextMeshProUGUI>();
            text.text = "Vault";
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
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

            if (!IsVaultAccessible())
            {
                CreateTextRow("Vault opens between waves");
                CloseContextMenuIfActiveItemMissing();
                return;
            }

            if (vault == null || vault.EntryCount == 0)
            {
                CreateTextRow("Vault is empty");
                CloseContextMenuIfActiveItemMissing();
                return;
            }

            foreach (var entry in vault.Entries)
            {
                CreateVaultRow(entry);
            }

            CloseContextMenuIfActiveItemMissing();
        }

        private void CloseContextMenuIfActiveItemMissing()
        {
            if (contextMenu != null)
            {
                contextMenu.CloseIfActiveItemMissing();
            }
        }

        private void CreateVaultRow(CastleInventoryEntry entry)
        {
            var rowObject = new GameObject("VaultRow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
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
            button.onClick.AddListener(() => contextMenu.ShowForItem(entry.ItemId, IsWeaponItem(entry.ItemId), InventoryContextSource.Vault, rect));

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(rowObject.transform, false);

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);

            var text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = $"{entry.ItemId} x{entry.Count}";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;

            var trigger = rowObject.AddComponent<InventoryContextMenuTrigger>();
            trigger.Configure(contextMenu, entry.ItemId, IsWeaponItem(entry.ItemId), InventoryContextSource.Vault);
        }

        private void CreateTextRow(string value)
        {
            var label = new GameObject("VaultRow", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
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

        private bool IsWeaponItem(string itemId)
        {
            var resolver = weaponDefinitionResolverSource as IWeaponDefinitionResolver ?? weaponDefinitionResolver ?? FindWeaponDefinitionResolver();
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

        private void ApplyPanelState(bool open)
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(open);
            }
        }

        private static void DestroyChild(GameObject child)
        {
            if (Application.isPlaying)
            {
                child.SetActive(false);
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }
}

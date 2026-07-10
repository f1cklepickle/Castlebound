using System;
using Castlebound.Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public enum InventoryContextSource
    {
        Backpack = 0,
        Vault = 1
    }

    public class InventoryContextMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform menuRoot;
        [SerializeField] private BackpackWeaponEquipController equipController;
        [SerializeField] private BackpackItemDropController dropController;
        [SerializeField] private BackpackInventoryStateComponent backpackSource;
        [SerializeField] private CastleInventoryStateComponent vaultSource;
        [SerializeField] private InventoryStateComponent activeInventorySource;

        private RectTransform parentRoot;
        private string activeItemId;
        private bool activeItemIsWeapon;
        private InventoryContextSource activeSource;
        private bool isChoosingEquipSlot;

        public event Action ActionCompleted;

        public bool IsOpen => menuRoot != null && menuRoot.gameObject.activeSelf;
        public string ActiveItemId => activeItemId;
        public InventoryContextSource ActiveSource => activeSource;
        public bool IsChoosingEquipSlot => isChoosingEquipSlot;

        public void SetParentRoot(RectTransform root)
        {
            parentRoot = root;
        }

        public void SetEquipController(BackpackWeaponEquipController controller)
        {
            equipController = controller;
        }

        public void SetDropController(BackpackItemDropController controller)
        {
            dropController = controller;
        }

        public void SetInventorySources(
            BackpackInventoryStateComponent backpack,
            CastleInventoryStateComponent vault,
            InventoryStateComponent activeInventory)
        {
            backpackSource = backpack;
            vaultSource = vault;
            activeInventorySource = activeInventory;
        }

        public void ShowForItem(string itemId, bool isWeapon, RectTransform anchor)
        {
            ShowForItem(itemId, isWeapon, InventoryContextSource.Backpack, anchor);
        }

        public void ShowForItem(string itemId, bool isWeapon, InventoryContextSource source, RectTransform anchor)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                Close();
                return;
            }

            activeItemId = itemId;
            activeItemIsWeapon = isWeapon;
            activeSource = source;
            isChoosingEquipSlot = false;
            EnsureMenuRoot();
            PositionNear(anchor);
            RebuildRootActions();
            menuRoot.gameObject.SetActive(true);
        }

        public void Close()
        {
            activeItemId = null;
            activeItemIsWeapon = false;
            activeSource = InventoryContextSource.Backpack;
            isChoosingEquipSlot = false;
            if (menuRoot != null)
            {
                menuRoot.gameObject.SetActive(false);
            }
        }

        public void CloseIfActiveItemMissing()
        {
            if (!IsOpen || GetActiveItemCount() > 0)
            {
                return;
            }

            Close();
        }

        private void RebuildRootActions()
        {
            ClearMenu();
            if (activeSource == InventoryContextSource.Backpack)
            {
                CreateMenuButton("Drop", TryDropActiveItem);
            }
            else
            {
                CreateMenuButton("Move to Backpack", TryMoveVaultItemToBackpack);
            }

            if (activeItemIsWeapon)
            {
                CreateMenuButton("Equip", ShowEquipSlots);
            }

            RefreshMenuSize();
        }

        private void ShowEquipSlots()
        {
            if (!activeItemIsWeapon)
            {
                return;
            }

            isChoosingEquipSlot = true;
            ClearMenu();
            if (activeSource == InventoryContextSource.Backpack)
            {
                CreateMenuButton("Drop", TryDropActiveItem);
            }
            else
            {
                CreateMenuButton("Move to Backpack", TryMoveVaultItemToBackpack);
            }

            CreateMenuButton("Main", () => TryEquipActiveItem(0));
            CreateMenuButton("Secondary", () => TryEquipActiveItem(1));
            RefreshMenuSize();
        }

        private void TryDropActiveItem()
        {
            if (dropController != null && dropController.TryDrop(activeItemId))
            {
                CompleteAction();
            }
        }

        private void TryEquipActiveItem(int slotIndex)
        {
            bool equipped = activeSource == InventoryContextSource.Backpack
                ? equipController != null && equipController.TryEquip(activeItemId, slotIndex)
                : TryEquipVaultItem(slotIndex);

            if (equipped)
            {
                CompleteAction();
            }
        }

        private void TryMoveVaultItemToBackpack()
        {
            string itemId = activeItemId;
            var vault = vaultSource != null ? vaultSource.State : null;
            var backpack = backpackSource != null ? backpackSource.State : null;
            if (vault == null || backpack == null || string.IsNullOrWhiteSpace(itemId) || vault.GetCount(itemId) <= 0 || !backpack.CanAddItem(itemId, 1))
            {
                return;
            }

            if (!vault.TryRemoveItem(itemId, 1))
            {
                return;
            }

            if (!backpack.AddItem(itemId, 1))
            {
                vault.AddItem(itemId, 1);
                return;
            }

            CompleteAction();
        }

        private bool TryEquipVaultItem(int slotIndex)
        {
            string itemId = activeItemId;
            var activeInventory = activeInventorySource != null ? activeInventorySource.State : null;
            var vault = vaultSource != null ? vaultSource.State : null;
            if (activeInventory == null || vault == null || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (slotIndex < 0 || slotIndex >= InventoryState.WeaponSlotCount || vault.GetCount(itemId) <= 0)
            {
                return false;
            }

            if (!vault.TryRemoveItem(itemId, 1))
            {
                return false;
            }

            if (!activeInventory.TrySetWeaponAtSlot(slotIndex, itemId, out string displacedWeaponId))
            {
                vault.AddItem(itemId, 1);
                return false;
            }

            if (string.IsNullOrEmpty(displacedWeaponId))
            {
                return true;
            }

            vault.AddItem(displacedWeaponId, 1);
            return true;
        }

        private void CompleteAction()
        {
            if (GetActiveItemCount() <= 0)
            {
                Close();
            }
            else if (isChoosingEquipSlot)
            {
                RebuildRootActions();
            }

            ActionCompleted?.Invoke();
        }

        private int GetActiveItemCount()
        {
            if (string.IsNullOrWhiteSpace(activeItemId))
            {
                return 0;
            }

            return activeSource == InventoryContextSource.Backpack
                ? backpackSource != null ? backpackSource.State.GetCount(activeItemId) : 0
                : vaultSource != null ? vaultSource.State.GetCount(activeItemId) : 0;
        }

        private void EnsureMenuRoot()
        {
            if (menuRoot != null)
            {
                return;
            }

            Transform parent = parentRoot != null ? parentRoot : transform;
            var menuObject = new GameObject("InventoryContextMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            menuObject.transform.SetParent(parent, false);
            menuRoot = menuObject.GetComponent<RectTransform>();
            menuRoot.anchorMin = new Vector2(0f, 1f);
            menuRoot.anchorMax = new Vector2(0f, 1f);
            menuRoot.pivot = new Vector2(0f, 1f);
            menuRoot.sizeDelta = new Vector2(116f, 0f);

            var image = menuObject.GetComponent<Image>();
            image.color = new Color(0.06f, 0.07f, 0.08f, 0.98f);

            var layout = menuObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            menuObject.SetActive(false);
        }

        private void PositionNear(RectTransform anchor)
        {
            if (menuRoot == null)
            {
                return;
            }

            if (anchor == null || parentRoot == null)
            {
                menuRoot.anchoredPosition = new Vector2(212f, -62f);
                return;
            }

            Vector3 worldPosition = anchor.TransformPoint(new Vector3(anchor.rect.xMax, anchor.rect.yMax, 0f));
            Vector3 localPosition = parentRoot.InverseTransformPoint(worldPosition);
            menuRoot.anchoredPosition = new Vector2(localPosition.x + 8f, localPosition.y);
        }

        private void ClearMenu()
        {
            if (menuRoot == null)
            {
                return;
            }

            for (int i = menuRoot.childCount - 1; i >= 0; i--)
            {
                DestroyChild(menuRoot.GetChild(i).gameObject);
            }
        }

        private void CreateMenuButton(string label, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(menuRoot, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(108f, 30f);

            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.minHeight = 30f;
            layout.preferredHeight = 30f;

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.16f, 0.18f, 0.21f, 1f);

            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            text.enableAutoSizing = true;
            text.fontSizeMin = 10;
            text.fontSizeMax = 14;
        }

        private void RefreshMenuSize()
        {
            if (menuRoot == null)
            {
                return;
            }

            int count = menuRoot.childCount;
            float height = 8f + count * 30f + Mathf.Max(0, count - 1) * 4f;
            menuRoot.sizeDelta = new Vector2(116f, height);
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

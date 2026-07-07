using System;
using Castlebound.Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class InventoryContextMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform menuRoot;
        [SerializeField] private BackpackWeaponEquipController equipController;
        [SerializeField] private BackpackItemDropController dropController;

        private RectTransform parentRoot;
        private string activeItemId;
        private bool activeItemIsWeapon;
        private bool isChoosingEquipSlot;

        public event Action ActionCompleted;

        public bool IsOpen => menuRoot != null && menuRoot.gameObject.activeSelf;
        public string ActiveItemId => activeItemId;
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

        public void ShowForItem(string itemId, bool isWeapon, RectTransform anchor)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                Close();
                return;
            }

            activeItemId = itemId;
            activeItemIsWeapon = isWeapon;
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
            isChoosingEquipSlot = false;
            if (menuRoot != null)
            {
                menuRoot.gameObject.SetActive(false);
            }
        }

        private void RebuildRootActions()
        {
            ClearMenu();
            CreateMenuButton("Drop", TryDropActiveItem);
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
            CreateMenuButton("Drop", TryDropActiveItem);
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
            if (equipController != null && equipController.TryEquip(activeItemId, slotIndex))
            {
                CompleteAction();
            }
        }

        private void CompleteAction()
        {
            Close();
            ActionCompleted?.Invoke();
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

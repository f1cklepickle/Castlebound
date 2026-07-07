using UnityEngine;
using UnityEngine.EventSystems;

namespace Castlebound.Gameplay.UI
{
    public class InventoryContextMenuTrigger : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float longPressSeconds = 0.45f;

        private InventoryContextMenuController menu;
        private string itemId;
        private bool isWeapon;
        private bool isPressing;
        private bool hasOpenedFromPress;
        private float pressStartTime;

        public float LongPressSeconds
        {
            get => longPressSeconds;
            set => longPressSeconds = Mathf.Max(0f, value);
        }

        public void Configure(InventoryContextMenuController targetMenu, string targetItemId, bool targetIsWeapon)
        {
            menu = targetMenu;
            itemId = targetItemId;
            isWeapon = targetIsWeapon;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
            {
                OpenMenu();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressing = true;
            hasOpenedFromPress = false;
            pressStartTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressing = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPressing = false;
        }

        private void Update()
        {
            if (!isPressing || hasOpenedFromPress || Time.unscaledTime - pressStartTime < longPressSeconds)
            {
                return;
            }

            hasOpenedFromPress = true;
            OpenMenu();
        }

        private void OpenMenu()
        {
            RectTransform anchor = transform as RectTransform;
            if (menu != null)
            {
                menu.ShowForItem(itemId, isWeapon, anchor);
            }
        }
    }
}

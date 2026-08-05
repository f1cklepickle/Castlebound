using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Castlebound.Gameplay.Input
{
    public class TouchDefenseAimButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        [Tooltip("Distance in screen pixels the pointer can move from the button center before the button begins following.")]
        [SerializeField, Min(0f)] private float softAnchorRadius = 160f;

        [Tooltip("Maximum distance in screen pixels the button can drift from its authored position.")]
        [SerializeField, Min(0f)] private float maxAnchorDrift = 170f;

        private enum CaptureValidationMode
        {
            None,
            Touchscreen,
            GenericPointer
        }

        private int activePointerId = NoPointer;
        private RectTransform buttonRect;
        private Vector2 visualHomeAnchoredPosition;
        private Vector2 gestureHomeScreenPosition;
        private Vector2 currentAnchorScreenPosition;
        private Camera pointerCamera;
        private bool hasVisualHome;
        private CaptureValidationMode validationMode;

        public bool IsDefending => activePointerId != NoPointer;
        public Vector2 FacingDirection { get; private set; }
        public Vector2 CurrentAnchorScreenPosition => currentAnchorScreenPosition;

        public float SoftAnchorRadius
        {
            get => softAnchorRadius;
            set => softAnchorRadius = Mathf.Max(0f, value);
        }

        public float MaxAnchorDrift
        {
            get => maxAnchorDrift;
            set => maxAnchorDrift = Mathf.Max(0f, value);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (TryGetPointerIdentity(eventData, out int pointerId, out CaptureValidationMode mode))
                TryBeginDefense(pointerId, eventData.position, mode, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (TryGetPointerIdentity(eventData, out int pointerId, out _))
                SimulateDrag(pointerId, eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (TryGetPointerIdentity(eventData, out int pointerId, out _))
                SimulatePointerUp(pointerId);
        }

        private void LateUpdate()
        {
            if (IsDefending && validationMode != CaptureValidationMode.None && !IsCapturedPointerActive())
                ResetCapture();
        }

        private void OnDisable()
        {
            ResetCapture();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                ResetCapture();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                ResetCapture();
        }

        public bool SimulatePointerDown(int pointerId, Vector2 screenPosition)
        {
            return TryBeginDefense(pointerId, screenPosition, CaptureValidationMode.None, null);
        }

        private bool TryBeginDefense(
            int pointerId,
            Vector2 screenPosition,
            CaptureValidationMode mode,
            Camera eventCamera)
        {
            if (IsDefending)
                return false;

            CacheAnchorHomes(screenPosition);
            activePointerId = pointerId;
            pointerCamera = eventCamera;
            validationMode = mode;
            FacingDirection = Vector2.zero;
            return true;
        }

        public bool SimulateDrag(int pointerId, Vector2 screenPosition)
        {
            if (pointerId != activePointerId)
                return false;

            Vector2 anchorDelta = screenPosition - currentAnchorScreenPosition;
            float distanceFromAnchor = anchorDelta.magnitude;
            if (distanceFromAnchor > softAnchorRadius)
            {
                Vector2 direction = anchorDelta / distanceFromAnchor;
                Vector2 proposedAnchor = currentAnchorScreenPosition +
                                         direction * (distanceFromAnchor - softAnchorRadius);
                Vector2 driftFromHome = Vector2.ClampMagnitude(
                    proposedAnchor - gestureHomeScreenPosition,
                    maxAnchorDrift);
                currentAnchorScreenPosition = gestureHomeScreenPosition + driftFromHome;
            }

            ApplyAnchorVisual();

            Vector2 delta = screenPosition - currentAnchorScreenPosition;
            if (delta.sqrMagnitude > 0f)
                FacingDirection = delta.normalized;
            return true;
        }

        public bool SimulatePointerUp(int pointerId)
        {
            if (pointerId != activePointerId)
                return false;

            ResetCapture();
            return true;
        }

        private void ResetCapture()
        {
            activePointerId = NoPointer;
            validationMode = CaptureValidationMode.None;
            FacingDirection = Vector2.zero;
            pointerCamera = null;

            if (hasVisualHome && buttonRect != null)
                buttonRect.anchoredPosition = visualHomeAnchoredPosition;

            currentAnchorScreenPosition = hasVisualHome ? gestureHomeScreenPosition : Vector2.zero;
        }

        private void CacheAnchorHomes(Vector2 screenPosition)
        {
            buttonRect = transform as RectTransform;
            if (buttonRect == null)
            {
                gestureHomeScreenPosition = screenPosition;
                currentAnchorScreenPosition = screenPosition;
                hasVisualHome = false;
                return;
            }

            visualHomeAnchoredPosition = buttonRect.anchoredPosition;
            gestureHomeScreenPosition = screenPosition;
            currentAnchorScreenPosition = gestureHomeScreenPosition;
            hasVisualHome = true;
        }

        private void ApplyAnchorVisual()
        {
            if (!hasVisualHome || buttonRect == null)
                return;

            if (buttonRect.parent is RectTransform parentRect &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    gestureHomeScreenPosition,
                    pointerCamera,
                    out Vector2 homeLocal) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    currentAnchorScreenPosition,
                    pointerCamera,
                    out Vector2 currentLocal))
            {
                buttonRect.anchoredPosition = visualHomeAnchoredPosition + currentLocal - homeLocal;
                return;
            }

            buttonRect.anchoredPosition = visualHomeAnchoredPosition +
                                          currentAnchorScreenPosition -
                                          gestureHomeScreenPosition;
        }

        private static bool TryGetPointerIdentity(
            PointerEventData eventData,
            out int pointerId,
            out CaptureValidationMode mode)
        {
            pointerId = NoPointer;
            mode = CaptureValidationMode.None;
            if (eventData == null)
                return false;

            if (eventData is ExtendedPointerEventData extended)
            {
                if (extended.pointerType != UIPointerType.Touch || extended.touchId == 0)
                    return false;

                pointerId = extended.touchId;
                mode = CaptureValidationMode.Touchscreen;
                return true;
            }

            if (eventData.pointerId < 0)
                return false;

            pointerId = eventData.pointerId;
            mode = CaptureValidationMode.GenericPointer;
            return true;
        }

        private bool IsCapturedPointerActive()
        {
            if (validationMode == CaptureValidationMode.Touchscreen)
                return IsTouchActive(activePointerId);

            return IsAnyTouchActive() ||
                   (Mouse.current != null && Mouse.current.leftButton.isPressed);
        }

        private static bool IsTouchActive(int touchId)
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            foreach (var touch in touchscreen.touches)
            {
                if (touch.touchId.ReadValue() == touchId && touch.press.isPressed)
                    return true;
            }

            return false;
        }

        private static bool IsAnyTouchActive()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            foreach (var touch in touchscreen.touches)
            {
                if (touch.press.isPressed)
                    return true;
            }

            return false;
        }
    }
}

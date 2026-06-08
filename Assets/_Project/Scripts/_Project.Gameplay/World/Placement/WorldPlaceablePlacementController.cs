using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Input;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Castlebound.Gameplay.World.Placement
{
    public class WorldPlaceablePlacementController : MonoBehaviour
    {
        [SerializeField] private PlaceableObjectDefinition defaultPlaceable;
        [SerializeField] private Grid worldGrid;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform placedParent;
        [SerializeField] private Transform uiParent;
        [SerializeField] private TouchAimAttackZone touchAimAttackZone;
        [SerializeField] private PlayerFireInputController playerFireInputController;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SpriteRenderer previewRenderer;
        [SerializeField] private Sprite placeholderPreviewSprite;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private PlaceablePlacementSurface currentSurface = PlaceablePlacementSurface.OutsideGround;
        [SerializeField] private float gridCellSize = 1f;
        [SerializeField] private Color validPreviewColor = new Color(0.35f, 1f, 0.35f, 0.55f);
        [SerializeField] private Color invalidPreviewColor = new Color(1f, 0.25f, 0.2f, 0.55f);

        private readonly CastleOccupancyMap occupancy = new CastleOccupancyMap();
        private PlaceableObjectDefinition selectedPlaceable;
        private Vector2 currentPreviewPosition;
        private Vector2 lockedPlacementPosition;
        private bool hasPreviewTarget;
        private bool hasLockedTarget;
        private Action placementCanceled;

        public bool HasSelection => selectedPlaceable != null;

        public bool IsPlacementActive => selectedPlaceable != null;

        public bool HasLockedTarget => hasLockedTarget;

        public Vector2 LockedPlacementPosition => lockedPlacementPosition;

        public PlaceableObjectDefinition SelectedPlaceable => selectedPlaceable;

        public PlaceableObjectDefinition DefaultPlaceable => defaultPlaceable;

        private void Awake()
        {
            ResolveReferences();
            EnsurePreviewRenderer();
            EnsurePlacementControls();
            RefreshPlacementControls();
        }

        private void OnEnable()
        {
            HookPlacementControls();
        }

        private void OnDisable()
        {
            UnhookPlacementControls();
        }

        private void Update()
        {
            if (selectedPlaceable == null)
            {
                SetPreviewActive(false);
                return;
            }

            if (worldCamera != null
                && TryGetPointerScreenPosition(out var screenPosition)
                && !IsPointerOverPlacementControls(screenPosition))
            {
                var worldPosition = ScreenToWorldPosition(screenPosition);
                var snapped = WorldPlaceablePlacementRules.SnapToGrid(worldPosition, gridCellSize);
                currentPreviewPosition = snapped;
                hasPreviewTarget = true;

                if (!hasLockedTarget)
                {
                    UpdatePreview(currentPreviewPosition, CanPlaceSelectedAt(currentPreviewPosition));
                }

                if (WasWorldLockRequested())
                {
                    LockPlacementTarget(currentPreviewPosition);
                }
            }

            if (hasLockedTarget)
            {
                UpdatePreview(lockedPlacementPosition, CanPlaceSelectedAt(lockedPlacementPosition));
            }
            else if (!hasPreviewTarget)
            {
                SetPreviewActive(false);
            }

            RefreshPlacementControls();
        }

        public void SelectDefaultPlaceable()
        {
            BeginPlacement(defaultPlaceable);
        }

        public bool BeginPlacement(PlaceableObjectDefinition definition)
        {
            return BeginPlacement(definition, null);
        }

        public bool BeginPlacement(PlaceableObjectDefinition definition, Action onCanceled)
        {
            if (definition == null || !definition.IsValid)
            {
                return false;
            }

            selectedPlaceable = definition;
            placementCanceled = onCanceled;
            ClearTarget();
            SetPlacementControlsActive(true);
            return true;
        }

        public void LockPlacementTarget(Vector2 snappedWorldPosition)
        {
            lockedPlacementPosition = snappedWorldPosition;
            hasLockedTarget = true;
            UpdatePreview(lockedPlacementPosition, CanPlaceSelectedAt(lockedPlacementPosition));
            RefreshPlacementControls();
        }

        public bool ConfirmPlacement()
        {
            if (!hasLockedTarget)
            {
                return false;
            }

            if (!TryPlaceSelectedAt(lockedPlacementPosition))
            {
                RefreshPlacementControls();
                return false;
            }

            ClearTarget();
            RefreshPlacementControls();
            return true;
        }

        public void CancelPlacement()
        {
            selectedPlaceable = null;
            ClearTarget();
            SetPreviewActive(false);
            SetPlacementControlsActive(false);
            ReleaseTouchAimAttackState();
            ReleasePlayerFireInputState();
            ReleasePlayerAttackInputState();

            var callback = placementCanceled;
            placementCanceled = null;
            callback?.Invoke();
        }

        public bool CanPlaceSelectedAt(Vector2 snappedWorldPosition)
        {
            return WorldPlaceablePlacementRules.CanPlaceAt(
                selectedPlaceable,
                snappedWorldPosition,
                currentSurface,
                occupancy);
        }

        public bool TryPlaceSelectedAt(Vector2 snappedWorldPosition)
        {
            if (!CanPlaceSelectedAt(snappedWorldPosition))
            {
                return false;
            }

            var parent = placedParent != null ? placedParent : transform;
            var instance = Instantiate(selectedPlaceable.Prefab, snappedWorldPosition, Quaternion.identity, parent);
            instance.name = selectedPlaceable.DisplayName;
            occupancy.Occupy(snappedWorldPosition, selectedPlaceable.Footprint);
            return true;
        }

        private void ResolveReferences()
        {
            if (worldGrid == null)
            {
                worldGrid = FindObjectOfType<Grid>();
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (placedParent == null)
            {
                placedParent = transform;
            }

            if (uiParent == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    uiParent = canvas.transform;
                }
            }

            if (touchAimAttackZone == null)
            {
                touchAimAttackZone = FindObjectOfType<TouchAimAttackZone>();
            }

            if (playerFireInputController == null)
            {
                playerFireInputController = FindObjectOfType<PlayerFireInputController>();
            }

            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            if (worldGrid != null)
            {
                gridCellSize = Mathf.Max(0.01f, worldGrid.cellSize.x);
            }
        }

        private void EnsurePreviewRenderer()
        {
            if (previewRenderer != null)
            {
                return;
            }

            var previewObject = new GameObject("BearTrapPlacementPreview", typeof(SpriteRenderer));
            previewObject.transform.SetParent(transform, false);
            previewRenderer = previewObject.GetComponent<SpriteRenderer>();
            previewRenderer.sprite = placeholderPreviewSprite;
            previewRenderer.sortingOrder = 25;
            SetPreviewActive(false);
        }

        private void EnsurePlacementControls()
        {
            if (uiParent == null)
            {
                return;
            }

            if (confirmButton == null)
            {
                confirmButton = CreatePlacementButton("BearTrapConfirmButton", "Confirm", new Vector2(-82f, 96f));
            }

            if (cancelButton == null)
            {
                cancelButton = CreatePlacementButton("BearTrapCancelButton", "Cancel", new Vector2(82f, 96f));
            }

            SetPlacementControlsActive(false);
        }

        private Button CreatePlacementButton(string name, string label, Vector2 anchoredPosition)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(uiParent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(148f, 44f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            return buttonObject.GetComponent<Button>();
        }

        private void HookPlacementControls()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelPlacement);
            }
        }

        private void UnhookPlacementControls()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(OnConfirmClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(CancelPlacement);
            }
        }

        private void OnConfirmClicked()
        {
            ConfirmPlacement();
        }

        private void ReleaseTouchAimAttackState()
        {
            if (touchAimAttackZone == null)
            {
                touchAimAttackZone = FindObjectOfType<TouchAimAttackZone>();
            }

            touchAimAttackZone?.SimulatePointerUp();
        }

        private void ReleasePlayerFireInputState()
        {
            if (playerFireInputController == null)
            {
                playerFireInputController = FindObjectOfType<PlayerFireInputController>();
            }

            playerFireInputController?.ClearHeldFire();
        }

        private void ReleasePlayerAttackInputState()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            playerController?.ClearAttackInputState();
        }

        private bool TryGetPointerScreenPosition(out Vector2 screenPosition)
        {
            screenPosition = default;

            if (Touchscreen.current != null
                && (Touchscreen.current.primaryTouch.press.isPressed
                    || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                screenPosition = Mouse.current.position.ReadValue();
            }
            else
            {
                return false;
            }

            return true;
        }

        private Vector2 ScreenToWorldPosition(Vector2 screenPosition)
        {
            var world = worldCamera.ScreenToWorldPoint(screenPosition);
            return new Vector2(world.x, world.y);
        }

        private static bool WasWorldLockRequested()
        {
            return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame);
        }

        private bool IsPointerOverPlacementControls(Vector2 screenPosition)
        {
            return IsScreenPositionInsideButton(confirmButton, screenPosition)
                || IsScreenPositionInsideButton(cancelButton, screenPosition);
        }

        private bool IsScreenPositionInsideButton(Button button, Vector2 screenPosition)
        {
            if (button == null || !button.gameObject.activeInHierarchy)
            {
                return false;
            }

            return button.transform is RectTransform rectTransform
                && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
        }

        private void UpdatePreview(Vector2 snappedPosition, bool canPlace)
        {
            if (previewRenderer == null)
            {
                return;
            }

            previewRenderer.transform.position = snappedPosition;
            previewRenderer.color = canPlace ? validPreviewColor : invalidPreviewColor;
            previewRenderer.sprite = ResolvePreviewSprite();
            SetPreviewActive(true);
        }

        private Sprite ResolvePreviewSprite()
        {
            if (placeholderPreviewSprite != null)
            {
                return placeholderPreviewSprite;
            }

            if (selectedPlaceable != null && selectedPlaceable.Prefab != null)
            {
                var renderer = selectedPlaceable.Prefab.GetComponentInChildren<SpriteRenderer>(true);
                if (renderer != null)
                {
                    return renderer.sprite;
                }
            }

            return null;
        }

        private void SetPreviewActive(bool active)
        {
            if (previewRenderer != null)
            {
                previewRenderer.gameObject.SetActive(active);
            }
        }

        private void ClearTarget()
        {
            hasPreviewTarget = false;
            hasLockedTarget = false;
            currentPreviewPosition = default;
            lockedPlacementPosition = default;
        }

        private void RefreshPlacementControls()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = IsPlacementActive
                    && hasLockedTarget
                    && CanPlaceSelectedAt(lockedPlacementPosition);
            }
        }

        private void SetPlacementControlsActive(bool active)
        {
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(active);
            }

            if (cancelButton != null)
            {
                cancelButton.gameObject.SetActive(active);
            }
        }
    }
}

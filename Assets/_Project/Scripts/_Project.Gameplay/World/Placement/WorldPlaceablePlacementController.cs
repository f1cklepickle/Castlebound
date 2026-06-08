using Castlebound.Gameplay.Castle;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [SerializeField] private Button selectButton;
        [SerializeField] private SpriteRenderer previewRenderer;
        [SerializeField] private Sprite placeholderPreviewSprite;
        [SerializeField] private PlaceablePlacementSurface currentSurface = PlaceablePlacementSurface.OutsideGround;
        [SerializeField] private float gridCellSize = 1f;
        [SerializeField] private Color validPreviewColor = new Color(0.35f, 1f, 0.35f, 0.55f);
        [SerializeField] private Color invalidPreviewColor = new Color(1f, 0.25f, 0.2f, 0.55f);

        private readonly CastleOccupancyMap occupancy = new CastleOccupancyMap();
        private PlaceableObjectDefinition selectedPlaceable;

        public bool HasSelection => selectedPlaceable != null;

        public PlaceableObjectDefinition SelectedPlaceable => selectedPlaceable;

        public PlaceableObjectDefinition DefaultPlaceable => defaultPlaceable;

        private void Awake()
        {
            ResolveReferences();
            EnsureSelectButton();
            EnsurePreviewRenderer();
        }

        private void OnEnable()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(SelectDefaultPlaceable);
            }
        }

        private void OnDisable()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(SelectDefaultPlaceable);
            }
        }

        private void Update()
        {
            if (selectedPlaceable == null)
            {
                SetPreviewActive(false);
                return;
            }

            if (!TryGetPointerWorldPosition(out var worldPosition))
            {
                SetPreviewActive(false);
                return;
            }

            var snapped = WorldPlaceablePlacementRules.SnapToGrid(worldPosition, gridCellSize);
            var canPlace = CanPlaceSelectedAt(snapped);
            UpdatePreview(snapped, canPlace);

            if (WasPrimaryPointerPressedThisFrame() && !IsPointerOverUi())
            {
                TryPlaceSelectedAt(snapped);
            }
        }

        public void SelectDefaultPlaceable()
        {
            if (defaultPlaceable != null && defaultPlaceable.IsValid)
            {
                selectedPlaceable = defaultPlaceable;
            }
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

            var instance = Instantiate(selectedPlaceable.Prefab, snappedWorldPosition, Quaternion.identity, placedParent);
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

            if (worldGrid != null)
            {
                gridCellSize = Mathf.Max(0.01f, worldGrid.cellSize.x);
            }
        }

        private void EnsureSelectButton()
        {
            if (selectButton != null || uiParent == null)
            {
                return;
            }

            var buttonObject = new GameObject("BearTrapPlaceButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(uiParent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(24f, -96f);
            rect.sizeDelta = new Vector2(148f, 42f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

            selectButton = buttonObject.GetComponent<Button>();

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "Bear Trap";
            label.fontSize = 16;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
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

        private bool TryGetPointerWorldPosition(out Vector2 worldPosition)
        {
            worldPosition = default;

            if (worldCamera == null)
            {
                return false;
            }

            Vector2 screenPosition;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
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

            var world = worldCamera.ScreenToWorldPoint(screenPosition);
            worldPosition = new Vector2(world.x, world.y);
            return true;
        }

        private static bool WasPrimaryPointerPressedThisFrame()
        {
            return (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);
        }

        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
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
    }
}

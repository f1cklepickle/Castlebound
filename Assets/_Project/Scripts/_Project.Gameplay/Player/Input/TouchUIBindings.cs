using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.UI;

namespace Castlebound.Gameplay.Input
{
    public class TouchUIBindings : MonoBehaviour
    {
        // All refs found automatically at runtime via FindObjectOfType / GetComponent.
        // [SerializeField] allows Inspector override and keeps the test Set* API intact.
        // Potion and weapon use TouchOnlyButton so mouse clicks on PC are ignored.
        [SerializeField] private TouchOnlyButton _potionButton;
        [SerializeField] private TouchOnlyButton _weaponButton;
        [SerializeField] private Button _closeButton;   // mouse-clickable on PC is fine

        private PotionUseController _potionUseController;
        private PlayerController _playerController;
        private UpgradeMenuController _upgradeMenuController;

        private void Start()
        {
            _potionUseController   = FindObjectOfType<PotionUseController>();
            _playerController      = FindObjectOfType<PlayerController>();
            _upgradeMenuController = FindObjectOfType<UpgradeMenuController>();

            // If not pre-wired in Inspector, get-or-add Button on the HUD containers at runtime.
            if (_potionButton == null)
            {
                var potionHud = FindObjectOfType<PotionHudSlot>();
                if (potionHud != null)
                    _potionButton = potionHud.GetComponent<TouchOnlyButton>()
                                   ?? potionHud.gameObject.AddComponent<TouchOnlyButton>();
            }

            if (_weaponButton == null)
            {
                var weaponHud = FindObjectOfType<WeaponSlotsHud>();
                if (weaponHud != null)
                    _weaponButton = weaponHud.GetComponent<TouchOnlyButton>()
                                    ?? weaponHud.gameObject.AddComponent<TouchOnlyButton>();
            }

            if (_closeButton == null && _upgradeMenuController != null)
                _closeButton = CreateCloseButton(_upgradeMenuController.MenuRoot);

            Initialize();
        }

        // ── Close button factory ──────────────────────────────────────────────

        private static Button CreateCloseButton(RectTransform parent)
        {
            if (parent == null) return null;

            var go = new GameObject("CloseButton",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            // Top-right corner, 44×44 px, 8 px inset
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-8f, -8f);
            rt.sizeDelta = new Vector2(44f, 44f);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.7f, 0.15f, 0.15f, 0.9f);

            var label = new GameObject("Label",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            label.transform.SetParent(go.transform, false);
            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var tmp = label.GetComponent<TextMeshProUGUI>();
            tmp.text      = "×";
            tmp.fontSize  = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor      = new Color(0.7f, 0.15f, 0.15f, 0.9f);
            colors.highlightedColor = new Color(0.9f, 0.2f,  0.2f,  1.0f);
            colors.pressedColor     = new Color(0.5f, 0.1f,  0.1f,  1.0f);
            button.colors = colors;

            return button;
        }

        // ── Injection (tests) ─────────────────────────────────────────────────

        public void SetPotionUseController(PotionUseController controller) => _potionUseController = controller;
        public void SetPlayerController(PlayerController controller)       => _playerController = controller;
        public void SetUpgradeMenuController(UpgradeMenuController controller) => _upgradeMenuController = controller;
        public void SetPotionButton(TouchOnlyButton button) => _potionButton = button;
        public void SetWeaponButton(TouchOnlyButton button) => _weaponButton = button;
        public void SetCloseButton(Button button)           => _closeButton  = button;

        public void Initialize()
        {
            if (_potionButton != null && _potionUseController != null)
                _potionButton.onClick.AddListener(() => _potionUseController.TryConsume());

            if (_weaponButton != null && _playerController != null)
                _weaponButton.onClick.AddListener(() => _playerController.TrySwapWeaponSlotWithoutCooldown());

            if (_closeButton != null && _upgradeMenuController != null)
                _closeButton.onClick.AddListener(() => _upgradeMenuController.CloseMenu());
        }
    }
}

using UnityEngine.UI;
using UnityEngine;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.UI;

namespace Castlebound.Gameplay.Input
{
    public class TouchUIBindings : MonoBehaviour
    {
        // All refs found automatically at runtime via FindObjectOfType / GetComponent.
        // [SerializeField] allows Inspector override and keeps the test Set* API intact.
        [SerializeField] private Button _potionButton;
        [SerializeField] private Button _weaponButton;
        [SerializeField] private Button _closeButton;

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
                    _potionButton = potionHud.GetComponent<Button>() ?? potionHud.gameObject.AddComponent<Button>();
            }

            if (_weaponButton == null)
            {
                var weaponHud = FindObjectOfType<WeaponSlotsHud>();
                if (weaponHud != null)
                    _weaponButton = weaponHud.GetComponent<Button>() ?? weaponHud.gameObject.AddComponent<Button>();
            }

            Initialize();
        }

        // ── Injection (tests) ─────────────────────────────────────────────────

        public void SetPotionUseController(PotionUseController controller) => _potionUseController = controller;
        public void SetPlayerController(PlayerController controller)       => _playerController = controller;
        public void SetUpgradeMenuController(UpgradeMenuController controller) => _upgradeMenuController = controller;
        public void SetPotionButton(Button button)  => _potionButton  = button;
        public void SetWeaponButton(Button button)  => _weaponButton  = button;
        public void SetCloseButton(Button button)   => _closeButton   = button;

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

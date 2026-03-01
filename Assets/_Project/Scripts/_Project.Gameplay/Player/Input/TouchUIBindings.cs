using UnityEngine.UI;
using UnityEngine;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.UI;

namespace Castlebound.Gameplay.Input
{
    public class TouchUIBindings : MonoBehaviour
    {
        private PotionUseController _potionUseController;
        private PlayerController _playerController;
        private UpgradeMenuController _upgradeMenuController;
        private Button _potionButton;
        private Button _weaponButton;
        private Button _closeButton;

        public void SetPotionUseController(PotionUseController controller) => _potionUseController = controller;
        public void SetPlayerController(PlayerController controller) => _playerController = controller;
        public void SetUpgradeMenuController(UpgradeMenuController controller) => _upgradeMenuController = controller;
        public void SetPotionButton(Button button) => _potionButton = button;
        public void SetWeaponButton(Button button) => _weaponButton = button;
        public void SetCloseButton(Button button) => _closeButton = button;

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

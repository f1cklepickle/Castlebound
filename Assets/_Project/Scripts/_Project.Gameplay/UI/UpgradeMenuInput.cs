using UnityEngine;
using UnityEngine.InputSystem;

namespace Castlebound.Gameplay.UI
{
    public class UpgradeMenuInput : MonoBehaviour
    {
        [SerializeField] private UpgradeMenuController menuController;

        private void Awake()
        {
            if (menuController == null)
            {
                menuController = FindObjectOfType<UpgradeMenuController>();
            }
        }

        public void OnToggleUpgradeMenu(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            HandleToggle();
        }

        private void HandleToggle()
        {
            if (menuController != null)
            {
                menuController.ToggleMenu();
            }
        }
    }
}

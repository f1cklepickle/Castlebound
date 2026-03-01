using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Castlebound.Gameplay.Input
{
    /// <summary>
    /// Registers a virtual Gamepad device on Enable and drives it each frame from
    /// the touch zone components. PlayerController responds via its existing Input
    /// System action callbacks — no direct coupling to touch input.
    /// </summary>
    public class MobileInputDriver : MonoBehaviour
    {
        [SerializeField] private TouchMovementZone movementZone;
        [SerializeField] private TouchAimAttackZone aimAttackZone;
        [SerializeField] private TouchRepairButton repairButton;

        private Gamepad _virtualGamepad;
        private bool _pendingRepairPress;

        private void OnEnable()
        {
            _virtualGamepad = InputSystem.AddDevice<Gamepad>("MobileGamepad");

            if (repairButton != null)
                repairButton.OnRepairRequested += HandleRepairRequested;
        }

        private void OnDisable()
        {
            if (repairButton != null)
                repairButton.OnRepairRequested -= HandleRepairRequested;

            if (_virtualGamepad != null)
            {
                InputSystem.RemoveDevice(_virtualGamepad);
                _virtualGamepad = null;
            }
        }

        private void Update()
        {
            if (_virtualGamepad == null)
                return;

            var state = new GamepadState();

            // Left stick drives movement (and facing when aim zone is inactive).
            if (movementZone != null)
                state.leftStick = movementZone.MoveVector;

            // Right stick drives facing direction; right trigger fires when above deadzone.
            if (aimAttackZone != null)
            {
                state.rightStick = aimAttackZone.FacingDirection;
                state.rightTrigger = aimAttackZone.IsFiring ? 1f : 0f;
            }

            // One-shot repair press: button is set this frame, absent next frame = released.
            if (_pendingRepairPress)
            {
                state.buttons |= (ushort)(1 << (int)GamepadButton.South);
                _pendingRepairPress = false;
            }

            InputSystem.QueueStateEvent(_virtualGamepad, state);
        }

        private void HandleRepairRequested()
        {
            _pendingRepairPress = true;
        }
    }
}

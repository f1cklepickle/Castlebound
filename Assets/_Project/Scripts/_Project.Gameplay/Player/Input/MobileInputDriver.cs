using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace Castlebound.Gameplay.Input
{
    /// <summary>
    /// Registers a virtual Gamepad device on Enable and drives it each frame from
    /// the touch zone components. PlayerController responds via its existing Input
    /// System action callbacks — no direct coupling to touch input.
    /// </summary>
    /// <remarks>
    /// Assign the scene's <see cref="PlayerInput"/> component in the inspector.
    /// On Android there are no physical keyboard/mouse devices, so PlayerInput's
    /// InputUser never auto-pairs with the virtual gamepad we create here.
    /// <see cref="InputUser.PerformPairingWithDevice"/> makes the pairing explicit
    /// so that all gamepad-bound actions (Move, Look, Fire, Repair…) are received.
    /// </remarks>
    public class MobileInputDriver : MonoBehaviour
    {
        [SerializeField] private TouchMovementZone movementZone;
        [SerializeField] private TouchAimAttackZone aimAttackZone;
        [SerializeField] private TouchRepairButton repairButton;

        [Tooltip("The PlayerInput component that drives PlayerController. " +
                 "Must be assigned so the virtual gamepad is paired to its InputUser.")]
        [SerializeField] private PlayerInput playerInput;

        private Gamepad _virtualGamepad;
        private bool _pendingRepairPress;

        private void OnEnable()
        {
            _virtualGamepad = InputSystem.AddDevice<Gamepad>("MobileGamepad");

            // Prefer the explicitly-assigned reference; fall back to scene search
            // so the inspector wire-up is optional.
            if (playerInput == null)
                playerInput = FindObjectOfType<PlayerInput>();

            // PlayerInput's InputUser only pairs with devices that exist at
            // initialisation time. On Android with no physical keyboard/mouse the
            // user has nothing paired, so state events from the virtual gamepad are
            // silently dropped. PerformPairingWithDevice fixes this at runtime.
            if (playerInput != null)
                InputUser.PerformPairingWithDevice(_virtualGamepad, user: playerInput.user);
            else
                Debug.LogWarning(
                    "[MobileInputDriver] No PlayerInput found in scene — virtual gamepad " +
                    "will not be paired and touch movement will not work. " +
                    "Assign the PlayerInput reference in the inspector to suppress this search.",
                    this);

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

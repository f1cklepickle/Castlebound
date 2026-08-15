using System.Collections.Generic;
using Castlebound.Gameplay.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PcControlModeController : MonoBehaviour
{
    [SerializeField] private PlayerRelativeMouseFacingController relativeMouseFacing;
    [SerializeField] private PlayerController playerController;

    private readonly HashSet<Object> pointerModeOwners = new HashSet<Object>();
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
    private bool inputLocked;
    private bool suppressCombatUntilMouseReleased;
    private bool pcMouseControlActive = true;
    private EventSystem cachedEventSystem;
    private PointerEventData pointerEventData;

    public CursorLockMode RequestedLockMode { get; private set; } = CursorLockMode.None;
    public bool RequestedCursorVisible { get; private set; } = true;
    public bool IsPointerModeActive => inputLocked || pointerModeOwners.Count > 0;
    public bool IsPcMouseControlActive => !Application.isMobilePlatform && Mouse.current != null && pcMouseControlActive;
    public bool IsPointerOverUi => IsPointerModeActive && IsPointerOverDesktopUi();
    public bool ShouldSnapFacingPresentation => IsPcMouseControlActive && !IsPointerModeActive;

    private void Awake()
    {
        if (relativeMouseFacing == null)
            relativeMouseFacing = GetComponent<PlayerRelativeMouseFacingController>();
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        RefreshCursorState();
    }

    private void Update()
    {
        if (Application.isMobilePlatform)
            return;

        if (Mouse.current != null &&
            (Mouse.current.delta.ReadValue().sqrMagnitude > 0.0001f ||
             Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame))
        {
            SetActiveInputDevice(true);
        }
        else if (Keyboard.current != null &&
            (Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed ||
             Keyboard.current.sKey.isPressed || Keyboard.current.dKey.isPressed))
        {
            SetActiveInputDevice(true);
        }
        else if (IsAnyGamepadActive())
        {
            SetActiveInputDevice(false);
        }

        RefreshTransitionSuppression();
    }

    private void OnDisable()
    {
        pointerModeOwners.Clear();
        RequestedLockMode = CursorLockMode.None;
        RequestedCursorVisible = true;
        Cursor.lockState = RequestedLockMode;
        Cursor.visible = RequestedCursorVisible;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            RefreshCursorState();
    }

    public void SetInputLocked(bool locked)
    {
        if (inputLocked == locked)
            return;

        inputLocked = locked;
        BeginControlModeTransition();
    }

    public void RefreshCursorState()
    {
        ApplyCursorState(IsPcMouseControlActive && IsPointerModeActive);
    }

    public void RequestPointerMode(Object owner)
    {
        if (owner != null && pointerModeOwners.Add(owner)) BeginControlModeTransition();
    }

    public void ReleasePointerMode(Object owner)
    {
        if (owner != null && pointerModeOwners.Remove(owner)) BeginControlModeTransition();
    }

    public bool ShouldSuppressCombatInput()
    {
        if (!IsPcMouseControlActive)
            return false;

        RefreshTransitionSuppression();
        return suppressCombatUntilMouseReleased || IsPointerOverUi;
    }

    public void NotifyMouseCombatInput()
    {
        if (!Application.isMobilePlatform && Mouse.current != null)
            SetActiveInputDevice(true);
    }

    public void NotifyLookInput(Vector2 lookInput)
    {
        if (Application.isMobilePlatform)
        {
            SetActiveInputDevice(false);
            return;
        }

        if (lookInput.sqrMagnitude > 1.21f)
            SetActiveInputDevice(true);
        else if (Gamepad.current != null || Touchscreen.current != null)
            SetActiveInputDevice(false);
    }

    public bool TryResolveFacing(Vector2 currentFacing, Vector2 movementInput, Vector2 absolutePointerAim,
        bool attackIntent, bool defenseIntent, out Vector2 resolvedFacing)
    {
        resolvedFacing = currentFacing;
        if (!IsPcMouseControlActive) return false;
        if (!IsPointerModeActive)
        {
            resolvedFacing = relativeMouseFacing != null ? relativeMouseFacing.ConsumeFacing(currentFacing) : currentFacing;
            return true;
        }
        resolvedFacing = ResolvePointerModeFacing(
            currentFacing,
            movementInput,
            absolutePointerAim,
            attackIntent || defenseIntent,
            IsPointerOverUi);
        relativeMouseFacing?.SynchronizeFacing(resolvedFacing);
        return true;
    }

    public static Vector2 ResolvePointerModeFacing(
        Vector2 currentFacing,
        Vector2 movementInput,
        Vector2 absolutePointerAim,
        bool combatAimIntent,
        bool pointerOverUi)
    {
        if (!pointerOverUi && combatAimIntent && absolutePointerAim.sqrMagnitude > 0.0001f)
            return absolutePointerAim.normalized;
        if (movementInput.sqrMagnitude > 0.0001f)
            return movementInput.normalized;
        return currentFacing;
    }

    public static CursorLockMode ResolveLockMode(bool pcMouseActive, bool pointerModeActive)
    {
        return pcMouseActive && !pointerModeActive ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public static bool ResolveCursorVisible(bool pcMouseActive, bool pointerModeActive)
    {
        return !pcMouseActive || pointerModeActive;
    }

    private void SetActiveInputDevice(bool mouseAndKeyboard)
    {
        if (pcMouseControlActive == mouseAndKeyboard)
            return;

        pcMouseControlActive = mouseAndKeyboard;
        SynchronizeFacingAndCursor();
    }

    private static bool IsAnyGamepadActive()
    {
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f ||
                gamepad.rightStick.ReadValue().sqrMagnitude > 0.04f ||
                gamepad.leftTrigger.ReadValue() > 0.2f ||
                gamepad.rightTrigger.ReadValue() > 0.2f ||
                gamepad.buttonSouth.isPressed || gamepad.buttonEast.isPressed ||
                gamepad.buttonWest.isPressed || gamepad.buttonNorth.isPressed)
            {
                return true;
            }
        }

        return false;
    }

    private void BeginControlModeTransition()
    {
        suppressCombatUntilMouseReleased = true;
        ResolveReferences();
        SynchronizeFacingAndCursor();
        playerController?.ClearCombatInputState();
    }

    private void SynchronizeFacingAndCursor()
    {
        ResolveReferences();
        relativeMouseFacing?.SynchronizeFacing(playerController != null ? playerController.CurrentFacingDirection : Vector2.up);
        RefreshCursorState();
    }

    private void RefreshTransitionSuppression()
    {
        if (!suppressCombatUntilMouseReleased)
            return;

        bool mouseReleased = Mouse.current == null ||
            (!Mouse.current.leftButton.isPressed && !Mouse.current.rightButton.isPressed);
        if (mouseReleased)
            suppressCombatUntilMouseReleased = false;
    }

    private void ResolveReferences()
    {
        if (relativeMouseFacing == null)
            relativeMouseFacing = GetComponent<PlayerRelativeMouseFacingController>();
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private bool IsPointerOverDesktopUi()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        if (cachedEventSystem != eventSystem || pointerEventData == null)
        {
            cachedEventSystem = eventSystem;
            pointerEventData = new PointerEventData(eventSystem);
        }

        pointerEventData.position = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        uiRaycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, uiRaycastResults);
        if (uiRaycastResults.Count == 0)
            return eventSystem.IsPointerOverGameObject();

        foreach (RaycastResult result in uiRaycastResults)
        {
            if (!IsDesktopTouchSurface(result.gameObject))
                return true;
        }

        return false;
    }

    public static bool IsDesktopTouchSurface(GameObject target)
    {
        if (target == null)
            return false;

        return target.GetComponentInParent<TouchMovementZone>() != null ||
            target.GetComponentInParent<TouchAimAttackZone>() != null ||
            target.GetComponentInParent<TouchDefenseAimButton>() != null ||
            target.GetComponentInParent<TouchRepairButton>() != null;
    }

    private void ApplyCursorState(bool pointerModeActive)
    {
        RequestedLockMode = ResolveLockMode(IsPcMouseControlActive, pointerModeActive);
        RequestedCursorVisible = ResolveCursorVisible(IsPcMouseControlActive, pointerModeActive);
        Cursor.lockState = RequestedLockMode;
        Cursor.visible = RequestedCursorVisible;
    }
}

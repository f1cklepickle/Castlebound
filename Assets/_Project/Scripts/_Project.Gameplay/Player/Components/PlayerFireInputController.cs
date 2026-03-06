using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFireInputController : MonoBehaviour
{
    [SerializeField] private bool useReleaseFallbackPolling = true;

    private Func<bool> tryAttack;
    private Func<bool> isFireStillPressedEvaluator;
    private bool isFireHeld;

    public bool IsFireHeld => isFireHeld;

    public void Configure(Func<bool> tryAttackCallback, Func<bool> pressedEvaluator = null)
    {
        tryAttack = tryAttackCallback;
        isFireStillPressedEvaluator = pressedEvaluator;
    }

    public void OnFirePressedStateChanged(bool isPressed)
    {
        isFireHeld = isPressed;
    }

    public void ClearHeldFire()
    {
        isFireHeld = false;
    }

    public void Tick()
    {
        if (!isFireHeld)
            return;

        if (useReleaseFallbackPolling && !IsFireStillPressed())
        {
            isFireHeld = false;
            return;
        }

        tryAttack?.Invoke();
    }

    private bool IsFireStillPressed()
    {
        if (isFireStillPressedEvaluator != null)
            return isFireStillPressedEvaluator();

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            return true;

        if (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.5f)
            return true;

        return false;
    }
}

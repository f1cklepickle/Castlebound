using System;
using UnityEngine;

[Serializable]
public class PlayerMovementOrchestrator
{
    [SerializeField] private float deadZone = 0.1f;

    public void Tick(
        PlayerCollisionMove2D mover,
        Vector2 movementInput,
        float speedMultiplier = 1f)
    {
        if (mover != null)
        {
            var mag = movementInput.magnitude;
            Vector2 resolvedMovement = mag < deadZone
                ? Vector2.zero
                : movementInput * Mathf.Max(0f, speedMultiplier);
            mover.SetMoveInput(resolvedMovement);
        }
    }
}

using System;
using UnityEngine;

[Serializable]
public class PlayerMovementOrchestrator
{
    [SerializeField] private float deadZone = 0.1f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float facingDirectionOffset = -90f;
    [SerializeField] private bool flipDirection = false;

    public Vector2 LastMoveDirection { get; private set; } = new Vector2(0f, 1f);
    public Vector2 LastFacingDirection { get; private set; } = new Vector2(0f, 1f);

    public void Tick(
        PlayerCollisionMove2D mover,
        Transform playerTransform,
        Vector2 movementInput,
        Vector2 aimInput,
        float deltaTime,
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

        if (movementInput.magnitude > 0f)
            LastMoveDirection = movementInput;

        Vector2 facingSource = aimInput.magnitude > deadZone ? aimInput : LastMoveDirection;
        if (facingSource.sqrMagnitude > 0f)
            LastFacingDirection = facingSource.normalized;
        Vector2 angleDirection = flipDirection ? -facingSource : facingSource;
        float targetAngle = Mathf.Atan2(angleDirection.y, angleDirection.x) * Mathf.Rad2Deg + facingDirectionOffset;
        var targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, targetRotation, rotationSpeed * deltaTime);
    }
}

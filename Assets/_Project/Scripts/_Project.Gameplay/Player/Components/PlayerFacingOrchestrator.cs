using System;
using UnityEngine;

[Serializable]
public class PlayerFacingOrchestrator
{
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float facingDirectionOffset = -90f;
    [SerializeField] private bool flipDirection;

    public Vector2 LastFacingDirection { get; private set; } = Vector2.up;

    public void Tick(
        Transform playerTransform,
        Vector2 facingDirection,
        float deltaTime,
        bool snapRotation = false)
    {
        if (playerTransform == null || facingDirection.sqrMagnitude <= 0.0001f)
            return;

        LastFacingDirection = facingDirection.normalized;
        Vector2 angleDirection = flipDirection ? -LastFacingDirection : LastFacingDirection;
        float targetAngle = Mathf.Atan2(angleDirection.y, angleDirection.x) * Mathf.Rad2Deg
            + facingDirectionOffset;
        var targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        playerTransform.rotation = snapRotation
            ? targetRotation
            : Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * Mathf.Max(0f, deltaTime));
    }
}

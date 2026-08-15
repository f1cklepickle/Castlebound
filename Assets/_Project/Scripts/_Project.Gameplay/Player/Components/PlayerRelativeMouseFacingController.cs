using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRelativeMouseFacingController : MonoBehaviour
{
    public const float MinimumSensitivity = 0.01f;
    public const float MaximumSensitivity = 2f;

    [SerializeField, Range(MinimumSensitivity, MaximumSensitivity)]
    private float mouseTurnSensitivity = 0.35f;

    private Vector2 accumulatedFacing;
    private float pendingHorizontalDelta;
    private bool hasAccumulatedFacing;
    public float MouseTurnSensitivity => mouseTurnSensitivity;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        QueueMouseDelta(Mouse.current.delta.ReadValue());
    }

    public Vector2 ConsumeFacing(Vector2 currentFacing)
    {
        Vector2 facingDirection = ResolveFacing(
            currentFacing,
            new Vector2(pendingHorizontalDelta, 0f));
        pendingHorizontalDelta = 0f;
        return facingDirection;
    }

    public Vector2 ResolveFacing(Vector2 currentFacing, Vector2 mouseDelta)
    {
        if (!hasAccumulatedFacing)
        {
            accumulatedFacing = NormalizeOrFallback(currentFacing, Vector2.up);
            hasAccumulatedFacing = true;
        }

        float turnDegrees = -mouseDelta.x * mouseTurnSensitivity;
        if (!Mathf.Approximately(turnDegrees, 0f))
        {
            accumulatedFacing = (Quaternion.Euler(0f, 0f, turnDegrees) * accumulatedFacing).normalized;
        }

        return accumulatedFacing;
    }

    public void QueueMouseDelta(Vector2 mouseDelta)
    {
        pendingHorizontalDelta += mouseDelta.x;
    }

    public void SynchronizeFacing(Vector2 currentFacing)
    {
        accumulatedFacing = NormalizeOrFallback(currentFacing, Vector2.up);
        hasAccumulatedFacing = true;
        pendingHorizontalDelta = 0f;
    }

    public void Configure(float sensitivity)
    {
        mouseTurnSensitivity = Mathf.Clamp(
            sensitivity,
            MinimumSensitivity,
            MaximumSensitivity);
        accumulatedFacing = Vector2.zero;
        pendingHorizontalDelta = 0f;
        hasAccumulatedFacing = false;
    }

    private void OnValidate()
    {
        mouseTurnSensitivity = Mathf.Clamp(
            mouseTurnSensitivity,
            MinimumSensitivity,
            MaximumSensitivity);
    }

    private static Vector2 NormalizeOrFallback(Vector2 value, Vector2 fallback)
    {
        return value.sqrMagnitude > 0.0001f ? value.normalized : fallback;
    }
}

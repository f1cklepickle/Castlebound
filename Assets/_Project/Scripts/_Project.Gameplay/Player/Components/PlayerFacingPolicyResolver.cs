using UnityEngine;

public class PlayerFacingPolicyResolver : MonoBehaviour
{
    [SerializeField] private float aimEnterThreshold = 0.25f;
    [SerializeField] private float aimExitThreshold = 0.20f;
    [SerializeField] private float movementMeaningfulThreshold = 0.10f;
    [SerializeField] private float returnToMovementFacingSpeed = 720f;
    [SerializeField] private float stickMagnitudeMax = 1.1f;

    private bool rightStickAimActive;

    public Vector2 ResolveFacing(
        Vector2 currentFacing,
        Vector2 movementInput,
        Vector2 resolvedAimInput,
        Vector2 rawLookInput,
        bool aimIntentActive,
        float deltaTime)
    {
        bool stickAimIntentActive = UpdateRightStickAimIntent(rawLookInput);
        bool shouldUseAim = aimIntentActive || stickAimIntentActive;

        if (shouldUseAim && resolvedAimInput.sqrMagnitude > 0.0001f)
            return resolvedAimInput.normalized;

        float movementThresholdSquared = movementMeaningfulThreshold * movementMeaningfulThreshold;
        if (movementInput.sqrMagnitude >= movementThresholdSquared)
            return movementInput.normalized;

        return currentFacing;
    }

    private bool UpdateRightStickAimIntent(Vector2 rawLookInput)
    {
        float lookMagnitude = rawLookInput.magnitude;
        if (lookMagnitude > stickMagnitudeMax)
        {
            rightStickAimActive = false;
            return false;
        }

        if (rightStickAimActive)
        {
            if (lookMagnitude <= aimExitThreshold)
                rightStickAimActive = false;
        }
        else if (lookMagnitude >= aimEnterThreshold)
        {
            rightStickAimActive = true;
        }

        return rightStickAimActive;
    }
}

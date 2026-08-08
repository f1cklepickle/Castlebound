using UnityEngine;

public class PlayerFacingPolicyResolver : MonoBehaviour
{
    [SerializeField] private float aimEnterThreshold = 0.25f;
    [SerializeField] private float aimExitThreshold = 0.20f;
    [SerializeField] private float movementMeaningfulThreshold = 0.10f;
    [SerializeField, Min(0f)] private float postAimFacingGraceDuration = 0.6f;
    [SerializeField] private float stickMagnitudeMax = 1.1f;

    private bool rightStickAimActive;
    private bool attackAimWasActive;
    private bool defenseWasActive;
    private float postAimFacingGraceRemaining;
    private Vector2 lastAttackAimDirection;
    private Vector2 retainedPostAimDirection;
    private Vector2 activeDefenseDirection;

    public Vector2 ResolveFacing(
        Vector2 currentFacing,
        Vector2 movementInput,
        Vector2 resolvedAimInput,
        Vector2 rawLookInput,
        bool attackAimIntentActive,
        bool defenseActive,
        float deltaTime)
    {
        bool stickAimIntentActive = UpdateRightStickAimIntent(rawLookInput);
        bool attackAimActive = attackAimIntentActive || stickAimIntentActive;
        bool hasResolvedAim = resolvedAimInput.sqrMagnitude > 0.0001f;

        if (attackAimActive && hasResolvedAim)
            lastAttackAimDirection = resolvedAimInput.normalized;

        if (!attackAimActive && attackAimWasActive && lastAttackAimDirection.sqrMagnitude > 0.0001f)
        {
            retainedPostAimDirection = lastAttackAimDirection;
            postAimFacingGraceRemaining = postAimFacingGraceDuration;
        }

        postAimFacingGraceRemaining = Mathf.Max(
            0f,
            postAimFacingGraceRemaining - Mathf.Max(0f, deltaTime));

        if (!defenseActive && defenseWasActive)
            postAimFacingGraceRemaining = 0f;

        if (defenseActive && hasResolvedAim)
        {
            activeDefenseDirection = resolvedAimInput.normalized;
        }
        else if (defenseActive && !defenseWasActive)
            activeDefenseDirection = ResolveDefenseStartDirection(currentFacing);

        attackAimWasActive = attackAimActive;
        defenseWasActive = defenseActive;

        if (defenseActive && activeDefenseDirection.sqrMagnitude > 0.0001f)
            return activeDefenseDirection;

        if (attackAimActive && hasResolvedAim)
            return resolvedAimInput.normalized;

        if (postAimFacingGraceRemaining > 0f && retainedPostAimDirection.sqrMagnitude > 0.0001f)
            return retainedPostAimDirection;

        float movementThresholdSquared = movementMeaningfulThreshold * movementMeaningfulThreshold;
        if (movementInput.sqrMagnitude >= movementThresholdSquared)
            return movementInput.normalized;

        return currentFacing;
    }

    private void OnValidate()
    {
        postAimFacingGraceDuration = Mathf.Max(0f, postAimFacingGraceDuration);
    }

    private Vector2 ResolveDefenseStartDirection(Vector2 currentFacing)
    {
        if (postAimFacingGraceRemaining > 0f && retainedPostAimDirection.sqrMagnitude > 0.0001f)
            return retainedPostAimDirection;

        return currentFacing.sqrMagnitude > 0.0001f
            ? currentFacing.normalized
            : Vector2.up;
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

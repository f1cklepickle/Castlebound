using Castlebound.Gameplay.Combat;
using UnityEngine;

public class PlayerDashController : MonoBehaviour, IDamageImmunitySource
{
    private const float DirectionEpsilonSquared = 0.01f;
    private const float TimerTolerance = 0.000001f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float dashSpeed = 24f;
    [SerializeField, Min(0f)] private float fullDashDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float rearDurationMultiplier = 0.6f;

    [Header("Availability")]
    [SerializeField, Min(0f)] private float cooldown = 0.75f;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.25f;

    [Header("Mobile")]
    [SerializeField, Range(0f, 1f)] private float mobileOuterReleaseThreshold = 0.9f;

    private Vector2 direction;
    private PlayerDashDirection directionClass;
    private float activeDuration;
    private float activeRemaining;
    private float cooldownRemaining;
    private float invulnerabilityRemaining;

    public float DashSpeed
    {
        get => dashSpeed;
        set => dashSpeed = Mathf.Max(0f, value);
    }

    public float FullDashDuration => fullDashDuration;
    public float RearDurationMultiplier => rearDurationMultiplier;
    public float RearDashDuration => fullDashDuration * rearDurationMultiplier;
    public float FullDashDistance => dashSpeed * fullDashDuration;
    public float RearDashDistance => dashSpeed * RearDashDuration;
    public float MobileOuterReleaseThreshold => mobileOuterReleaseThreshold;
    public Vector2 Direction => direction;
    public PlayerDashDirection DirectionClass => directionClass;
    public float ActiveDuration => activeDuration;
    public float ActiveRemaining => activeRemaining;
    public float CooldownRemaining => cooldownRemaining;
    public float InvulnerabilityRemaining => invulnerabilityRemaining;
    public bool IsDashing => activeRemaining > 0f;
    public bool IsInvulnerable => invulnerabilityRemaining > 0f;
    public bool IsDamageImmune => IsInvulnerable;
    public bool IsReady => !IsDashing && cooldownRemaining <= 0f;
    public Vector2 CurrentVelocity => IsDashing ? direction * dashSpeed : Vector2.zero;

    public bool TryStart(Vector2 directionalInput, Vector2 currentFacing, bool actionStateAllowsDash)
    {
        if (!actionStateAllowsDash || !IsReady)
            return false;

        direction = ResolveDirection(directionalInput, currentFacing);
        directionClass = ClassifyDirection(direction, currentFacing);
        activeDuration = directionClass == PlayerDashDirection.Rear
            ? RearDashDuration
            : fullDashDuration;
        activeRemaining = activeDuration;
        cooldownRemaining = cooldown;
        invulnerabilityRemaining = invulnerabilityDuration;
        return true;
    }

    public bool TryResolveMobileRelease(Vector2 releaseSample, out Vector2 releaseDirection)
    {
        if (releaseSample.magnitude < mobileOuterReleaseThreshold ||
            releaseSample.sqrMagnitude <= 0.0001f)
        {
            releaseDirection = Vector2.zero;
            return false;
        }

        releaseDirection = releaseSample.normalized;
        return true;
    }

    public void Tick(float deltaTime)
    {
        float safeDelta = NormalizeDelta(deltaTime);
        activeRemaining = AdvanceTimer(activeRemaining, safeDelta);
        cooldownRemaining = AdvanceTimer(cooldownRemaining, safeDelta);
        invulnerabilityRemaining = AdvanceTimer(invulnerabilityRemaining, safeDelta);

        if (!IsDashing)
            direction = Vector2.zero;
    }

    public void CancelActiveState()
    {
        activeRemaining = 0f;
        invulnerabilityRemaining = 0f;
        direction = Vector2.zero;
    }

    public void ResetState()
    {
        direction = Vector2.zero;
        directionClass = PlayerDashDirection.Full;
        activeDuration = 0f;
        activeRemaining = 0f;
        cooldownRemaining = 0f;
        invulnerabilityRemaining = 0f;
    }

    public void Configure(
        float speed,
        float fullDuration,
        float rearMultiplier,
        float cooldownSeconds,
        float invulnerabilitySeconds,
        float mobileReleaseThreshold)
    {
        dashSpeed = Mathf.Max(0f, speed);
        fullDashDuration = Mathf.Max(0f, fullDuration);
        rearDurationMultiplier = Mathf.Clamp01(rearMultiplier);
        cooldown = Mathf.Max(0f, cooldownSeconds);
        invulnerabilityDuration = Mathf.Max(0f, invulnerabilitySeconds);
        mobileOuterReleaseThreshold = Mathf.Clamp01(mobileReleaseThreshold);
    }

    public static PlayerDashDirection ClassifyDirection(Vector2 dashDirection, Vector2 currentFacing)
    {
        Vector2 normalizedDirection = NormalizeOrFallback(dashDirection, Vector2.down);
        Vector2 normalizedFacing = NormalizeOrFallback(currentFacing, Vector2.up);
        return Vector2.Dot(normalizedDirection, normalizedFacing) < 0f
            ? PlayerDashDirection.Rear
            : PlayerDashDirection.Full;
    }

    private static Vector2 ResolveDirection(Vector2 directionalInput, Vector2 currentFacing)
    {
        if (directionalInput.sqrMagnitude >= DirectionEpsilonSquared)
            return directionalInput.normalized;

        return -NormalizeOrFallback(currentFacing, Vector2.up);
    }

    private static Vector2 NormalizeOrFallback(Vector2 value, Vector2 fallback)
    {
        return value.sqrMagnitude > 0.0001f ? value.normalized : fallback;
    }

    private static float NormalizeDelta(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsNegativeInfinity(deltaTime) || deltaTime <= 0f)
            return 0f;
        return float.IsPositiveInfinity(deltaTime) ? float.MaxValue : deltaTime;
    }

    private static float AdvanceTimer(float remaining, float deltaTime)
    {
        return remaining <= deltaTime + TimerTolerance
            ? 0f
            : remaining - deltaTime;
    }

    private void OnValidate()
    {
        dashSpeed = Mathf.Max(0f, dashSpeed);
        fullDashDuration = Mathf.Max(0f, fullDashDuration);
        rearDurationMultiplier = Mathf.Clamp01(rearDurationMultiplier);
        cooldown = Mathf.Max(0f, cooldown);
        invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
        mobileOuterReleaseThreshold = Mathf.Clamp01(mobileOuterReleaseThreshold);
    }
}

using System;
using UnityEngine;

public class PlayerAttackLoop : MonoBehaviour
{
    [SerializeField] private float windupDuration = 0.05f;
    [SerializeField] private float activeDuration = 0.06f;
    [SerializeField] private float recoveryDuration = 0.09f;
    [SerializeField] private float minSwingDuration = 0.03f;

    private enum AttackPhase
    {
        Idle,
        AwaitingChain,
        Windup,
        Active,
        Recovery
    }

    private AttackPhase phase = AttackPhase.Idle;
    private float phaseRemaining;
    private float swingElapsed;
    private float currentWindupDuration;
    private float currentActiveDuration;
    private float currentRecoveryDuration;
    private int completedSwingCount;

    public bool IsSwingActive => phase != AttackPhase.Idle;
    public bool IsPresentationActive => phase == AttackPhase.Windup || phase == AttackPhase.Active;
    public bool IsHitWindowOpen => phase == AttackPhase.Active;
    public int CompletedSwingCount => completedSwingCount;
    public float CurrentWindupDuration => currentWindupDuration;
    public float CurrentActiveDuration => currentActiveDuration;
    public float CurrentRecoveryDuration => currentRecoveryDuration;
    public float CurrentSwingDuration => currentWindupDuration + currentActiveDuration + currentRecoveryDuration;
    public float MinSwingDuration => minSwingDuration;
    public float NormalizedSwingProgress =>
        CurrentSwingDuration > 0f ? Mathf.Clamp01(swingElapsed / CurrentSwingDuration) : 0f;

    public void Tick(float deltaTime, float effectiveAttackRate, bool isHeld)
    {
        Tick(deltaTime, effectiveAttackRate, isHeld, null, null);
    }

    public void Tick(
        float deltaTime,
        float effectiveAttackRate,
        bool isHeld,
        Func<bool> tryStartSwing,
        Action<bool> setHitWindowActive)
    {
        if (phase == AttackPhase.Idle && isHeld)
            TryStartSwing(effectiveAttackRate, tryStartSwing);

        if (deltaTime <= 0f || phase == AttackPhase.Idle)
            return;

        float remainingDelta = deltaTime;
        while (remainingDelta > 0f && phase != AttackPhase.Idle)
        {
            if (phase == AttackPhase.AwaitingChain)
            {
                if (!isHeld)
                {
                    phase = AttackPhase.Idle;
                    phaseRemaining = 0f;
                    break;
                }

                if (!TryStartSwing(effectiveAttackRate, tryStartSwing))
                    break;

                continue;
            }

            float step = Mathf.Min(phaseRemaining, remainingDelta);
            phaseRemaining -= step;
            remainingDelta -= step;
            swingElapsed += step;

            if (phaseRemaining > 0f)
                break;

            AdvancePhase(effectiveAttackRate, isHeld, tryStartSwing, setHitWindowActive);
        }
    }

    public void ResetLoopState()
    {
        phase = AttackPhase.Idle;
        phaseRemaining = 0f;
        swingElapsed = 0f;
        currentWindupDuration = 0f;
        currentActiveDuration = 0f;
        currentRecoveryDuration = 0f;
        completedSwingCount = 0;
    }

    private bool TryStartSwing(float effectiveAttackRate, Func<bool> tryStartSwing)
    {
        if (tryStartSwing != null && !tryStartSwing.Invoke())
            return false;

        ComputeScaledDurations(effectiveAttackRate);
        phase = AttackPhase.Windup;
        phaseRemaining = currentWindupDuration;
        swingElapsed = 0f;
        return true;
    }

    private void AdvancePhase(
        float effectiveAttackRate,
        bool isHeld,
        Func<bool> tryStartSwing,
        Action<bool> setHitWindowActive)
    {
        switch (phase)
        {
            case AttackPhase.Windup:
                phase = AttackPhase.Active;
                phaseRemaining = currentActiveDuration;
                setHitWindowActive?.Invoke(true);
                break;
            case AttackPhase.Active:
                phase = AttackPhase.Recovery;
                phaseRemaining = currentRecoveryDuration;
                setHitWindowActive?.Invoke(false);
                break;
            case AttackPhase.Recovery:
                completedSwingCount++;
                if (isHeld)
                {
                    if (!TryStartSwing(effectiveAttackRate, tryStartSwing))
                    {
                        phase = AttackPhase.AwaitingChain;
                        phaseRemaining = 0f;
                        swingElapsed = CurrentSwingDuration;
                    }
                }
                else
                {
                    phase = AttackPhase.Idle;
                    phaseRemaining = 0f;
                }
                break;
        }
    }

    private void ComputeScaledDurations(float effectiveAttackRate)
    {
        float safeRate = Mathf.Max(effectiveAttackRate, 0.0001f);
        float baseTotal = Mathf.Max(0.001f, windupDuration + activeDuration + recoveryDuration);
        float targetSwingDuration = Mathf.Max(minSwingDuration, 1f / safeRate);
        float durationScale = targetSwingDuration / baseTotal;

        currentWindupDuration = Mathf.Max(0.0001f, windupDuration * durationScale);
        currentActiveDuration = Mathf.Max(0.0001f, activeDuration * durationScale);
        currentRecoveryDuration = Mathf.Max(0.0001f, recoveryDuration * durationScale);
    }
}

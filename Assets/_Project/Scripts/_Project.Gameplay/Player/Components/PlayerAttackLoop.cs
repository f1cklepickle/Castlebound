using Castlebound.Gameplay.Combat;
using UnityEngine;

public class PlayerAttackLoop : MonoBehaviour
{
    [SerializeField] private float windupDuration = 0.05f;
    [SerializeField] private float activeDuration = 0.06f;
    [SerializeField] private float recoveryDuration = 0.09f;

    private readonly AttackClock attackClock = new AttackClock();
    private int completedSwingCount;
    private int startedSwingCount;
    private bool hitWindowActiveThisStep;

    public bool IsSwingActive => attackClock.IsRunning;
    public bool IsPresentationActive => attackClock.Phase == AttackClockPhase.Windup
        || attackClock.Phase == AttackClockPhase.Active;
    public bool IsHitWindowOpen => attackClock.Phase == AttackClockPhase.Active;
    public bool ShouldKeepHitboxActiveThisStep => hitWindowActiveThisStep;
    public int CompletedSwingCount => completedSwingCount;
    public int StartedSwingCount => startedSwingCount;
    public float CurrentAttackRate => attackClock.CurrentSwing.AttackRate;
    public float CurrentWindupDuration => attackClock.CurrentSwing.WindupDuration;
    public float CurrentActiveDuration => attackClock.CurrentSwing.ActiveDuration;
    public float CurrentRecoveryDuration => attackClock.CurrentSwing.RecoveryDuration;
    public float CurrentSwingDuration => attackClock.CurrentSwing.Duration;
    public float MinSwingDuration => 1f / AttackRatePolicy.MaximumAttackRate;
    public float NormalizedSwingProgress => attackClock.NormalizedProgress;

    public void Tick(float deltaTime, float effectiveAttackRate, bool isHeld)
    {
        hitWindowActiveThisStep = IsHitWindowOpen;

        if (!attackClock.IsRunning && isHeld)
            StartSwing(effectiveAttackRate);

        float remainingDelta = NormalizeDelta(deltaTime);
        while (remainingDelta > 0f && attackClock.IsRunning)
        {
            AttackClockStep step = attackClock.Advance(remainingDelta);
            hitWindowActiveThisStep |= step.ActiveWindowOccurred;

            if (!step.SwingCompleted)
                break;

            completedSwingCount++;
            remainingDelta = step.UnusedDeltaTime;
            if (!isHeld)
                break;

            StartSwing(effectiveAttackRate);
        }
    }

    public void ResetLoopState()
    {
        attackClock.Cancel();
        completedSwingCount = 0;
        startedSwingCount = 0;
        hitWindowActiveThisStep = false;
    }

    private void StartSwing(float effectiveAttackRate)
    {
        attackClock.Start(
            effectiveAttackRate,
            new AttackPhaseProfile(windupDuration, activeDuration, recoveryDuration));
        startedSwingCount++;
    }

    private static float NormalizeDelta(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f)
            return 0f;
        return deltaTime;
    }
}

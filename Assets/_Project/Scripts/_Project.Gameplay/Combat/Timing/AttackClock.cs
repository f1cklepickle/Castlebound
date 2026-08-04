using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public sealed class AttackClock
    {
        private const float BoundaryTolerance = 0.000001f;

        private float elapsed;
        private bool impactEmitted;

        public AttackClockPhase Phase { get; private set; } = AttackClockPhase.Idle;
        public AttackSwingTiming CurrentSwing { get; private set; }
        public bool IsRunning => Phase == AttackClockPhase.Windup
            || Phase == AttackClockPhase.Active
            || Phase == AttackClockPhase.Recovery;
        public float NormalizedProgress => CurrentSwing.Duration > 0f
            ? Mathf.Clamp01(elapsed / CurrentSwing.Duration)
            : 0f;

        public void Start(float attackRate, AttackPhaseProfile phaseProfile)
        {
            CurrentSwing = new AttackSwingTiming(attackRate, phaseProfile);
            elapsed = 0f;
            impactEmitted = false;
            Phase = AttackClockPhase.Windup;
        }

        public AttackClockStep Advance(float deltaTime)
        {
            float safeDelta = NormalizeDelta(deltaTime);
            if (!IsRunning)
                return new AttackClockStep(false, false, false, safeDelta);

            float previousElapsed = elapsed;
            float timeToCompletion = Mathf.Max(0f, CurrentSwing.Duration - elapsed);
            bool reachesCompletion = safeDelta + BoundaryTolerance >= timeToCompletion;
            float consumed = reachesCompletion ? timeToCompletion : safeDelta;
            elapsed += consumed;

            float impactTime = CurrentSwing.WindupDuration;
            float activeEndTime = impactTime + CurrentSwing.ActiveDuration;
            bool impactOccurred = !impactEmitted && elapsed + BoundaryTolerance >= impactTime;
            if (impactOccurred)
                impactEmitted = true;

            bool activeWindowOccurred = consumed > 0f
                && CurrentSwing.ActiveDuration > 0f
                && previousElapsed < activeEndTime
                && elapsed + BoundaryTolerance >= impactTime;

            if (reachesCompletion)
            {
                elapsed = CurrentSwing.Duration;
                Phase = AttackClockPhase.Completed;
                return new AttackClockStep(
                    impactOccurred,
                    activeWindowOccurred,
                    true,
                    Mathf.Max(0f, safeDelta - consumed));
            }

            Phase = ResolvePhase(elapsed, impactTime, activeEndTime);
            return new AttackClockStep(impactOccurred, activeWindowOccurred, false, 0f);
        }

        public void Cancel()
        {
            elapsed = 0f;
            impactEmitted = false;
            CurrentSwing = default;
            Phase = AttackClockPhase.Idle;
        }

        private static AttackClockPhase ResolvePhase(float currentTime, float impactTime, float activeEndTime)
        {
            if (currentTime + BoundaryTolerance < impactTime)
                return AttackClockPhase.Windup;
            if (currentTime + BoundaryTolerance < activeEndTime)
                return AttackClockPhase.Active;
            return AttackClockPhase.Recovery;
        }

        private static float NormalizeDelta(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsNegativeInfinity(deltaTime) || deltaTime <= 0f)
                return 0f;
            if (float.IsPositiveInfinity(deltaTime))
                return float.MaxValue;
            return deltaTime;
        }
    }
}

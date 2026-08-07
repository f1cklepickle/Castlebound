using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public sealed class PlayerDefenseStateMachine
    {
        private const float BoundaryTolerance = 0.000001f;

        private float activeParryWindowDuration;
        private float activeRecoveryDuration;
        private float elapsed;

        public PlayerDefenseState State { get; private set; } = PlayerDefenseState.Idle;
        public int RemainingParryCapacity { get; private set; }
        public bool IsGuarding => State == PlayerDefenseState.ParryWindow
            || State == PlayerDefenseState.Blocking;
        public bool CanAttack => State == PlayerDefenseState.Idle;

        public bool BeginDefense(float parryWindowDuration, int parryCapacity)
        {
            if (State != PlayerDefenseState.Idle)
                return false;

            elapsed = 0f;
            activeParryWindowDuration = NormalizeDuration(parryWindowDuration);
            RemainingParryCapacity = Mathf.Max(0, parryCapacity);
            State = RemainingParryCapacity > 0
                ? PlayerDefenseState.ParryWindow
                : PlayerDefenseState.Blocking;
            return true;
        }

        public bool ReleaseDefense(float recoveryDuration)
        {
            if (!IsGuarding)
                return false;

            elapsed = 0f;
            activeRecoveryDuration = NormalizeDuration(recoveryDuration);
            RemainingParryCapacity = 0;
            State = PlayerDefenseState.Recovery;
            return true;
        }

        public bool TryConsumeParry()
        {
            if (State != PlayerDefenseState.ParryWindow || RemainingParryCapacity <= 0)
                return false;

            RemainingParryCapacity--;
            if (RemainingParryCapacity == 0)
                State = PlayerDefenseState.Blocking;
            return true;
        }

        public void Advance(float deltaTime)
        {
            float safeDelta = NormalizeDelta(deltaTime);

            if (State == PlayerDefenseState.ParryWindow)
            {
                elapsed += safeDelta;
                if (elapsed > activeParryWindowDuration + BoundaryTolerance)
                    State = PlayerDefenseState.Blocking;
                return;
            }

            if (State != PlayerDefenseState.Recovery)
                return;

            elapsed += safeDelta;
            if (elapsed + BoundaryTolerance < activeRecoveryDuration)
                return;

            elapsed = 0f;
            State = PlayerDefenseState.Idle;
        }

        private static float NormalizeDuration(float duration)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration <= 0f)
                return 0f;
            return duration;
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

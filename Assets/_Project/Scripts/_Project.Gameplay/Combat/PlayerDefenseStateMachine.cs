using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public sealed class PlayerDefenseStateMachine
    {
        private const float BoundaryTolerance = 0.000001f;

        private readonly float parryWindowDuration;
        private readonly float recoveryDuration;
        private float elapsed;

        public PlayerDefenseStateMachine(float parryWindowDuration, float recoveryDuration)
        {
            this.parryWindowDuration = NormalizeDuration(parryWindowDuration);
            this.recoveryDuration = NormalizeDuration(recoveryDuration);
        }

        public PlayerDefenseState State { get; private set; } = PlayerDefenseState.Idle;
        public bool IsGuarding => State == PlayerDefenseState.ParryWindow
            || State == PlayerDefenseState.Blocking;
        public bool CanAttack => State == PlayerDefenseState.Idle;

        public bool BeginDefense()
        {
            if (State != PlayerDefenseState.Idle)
                return false;

            elapsed = 0f;
            State = PlayerDefenseState.ParryWindow;
            return true;
        }

        public bool ReleaseDefense()
        {
            if (!IsGuarding)
                return false;

            elapsed = 0f;
            State = PlayerDefenseState.Recovery;
            return true;
        }

        public void Advance(float deltaTime)
        {
            float safeDelta = NormalizeDelta(deltaTime);

            if (State == PlayerDefenseState.ParryWindow)
            {
                elapsed += safeDelta;
                if (elapsed > parryWindowDuration + BoundaryTolerance)
                    State = PlayerDefenseState.Blocking;
                return;
            }

            if (State != PlayerDefenseState.Recovery)
                return;

            elapsed += safeDelta;
            if (elapsed + BoundaryTolerance < recoveryDuration)
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

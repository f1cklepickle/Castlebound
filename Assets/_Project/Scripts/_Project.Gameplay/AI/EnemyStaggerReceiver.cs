using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyStaggerReceiver : MonoBehaviour, IEnemyStaggerReceiver
    {
        [SerializeField] private bool staggerEligible = true;
        [SerializeField, Min(0f)] private float staggerDurationSeconds = 1f;
        [SerializeField] private EnemyAttack enemyAttack;

        public EnemyStaggerState State { get; private set; }
        public float RemainingSeconds { get; private set; }
        public bool IsActionLocked => State != EnemyStaggerState.Inactive;
        public bool StaggerEligible => staggerEligible;
        public float StaggerDurationSeconds => NormalizeDuration(staggerDurationSeconds);

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            ClearState();
        }

        private void OnValidate()
        {
            staggerDurationSeconds = NormalizeDuration(staggerDurationSeconds);
        }

        public bool TryStagger()
        {
            float duration = StaggerDurationSeconds;
            if (!staggerEligible || duration <= 0f || enemyAttack == null || IsActionLocked)
                return false;

            RemainingSeconds = duration;
            State = EnemyStaggerState.Staggered;
            enemyAttack.CancelForStagger();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (State != EnemyStaggerState.Staggered)
                return;

            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - NormalizeDelta(deltaTime));
            if (RemainingSeconds <= 0f)
                State = EnemyStaggerState.AwaitingTargetRefresh;
        }

        public bool AcknowledgeTargetRefresh()
        {
            if (State != EnemyStaggerState.AwaitingTargetRefresh)
                return false;

            ClearState();
            return true;
        }

        public void Configure(bool eligible, float durationSeconds, EnemyAttack attack)
        {
            staggerEligible = eligible;
            staggerDurationSeconds = NormalizeDuration(durationSeconds);
            enemyAttack = attack;
        }

        private void ClearState()
        {
            State = EnemyStaggerState.Inactive;
            RemainingSeconds = 0f;
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

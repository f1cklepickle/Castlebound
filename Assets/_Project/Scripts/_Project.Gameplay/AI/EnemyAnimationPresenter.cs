using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimationPresenter : MonoBehaviour
    {
        public enum PresentationState { Idle, Walk, Attack, Hold }

        [SerializeField] private Animator animator;
        [SerializeField] private string idleStateName = "Idle";
        [SerializeField] private string walkStateName = "Walk";
        [SerializeField] private string attackStateName = "Attack";
        [SerializeField, Min(0.01f)] private float authoredImpactTimeSeconds = 0.33333334f;
        [SerializeField, Min(0f)] private float idleDelaySeconds = 2f;

        private PresentationState state;
        private float inactiveSeconds;
        private bool movementRequested;

        public float AuthoredImpactTimeSeconds => authoredImpactTimeSeconds;
        public float IdleDelaySeconds => idleDelaySeconds;
        public PresentationState CurrentState => state;

        private void Awake() => InitializePresentation();
        private void Update() => Advance(Time.deltaTime);

        public void InitializePresentation()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            HoldNeutral();
        }

        public void SetMovementRequested(bool isMoving)
        {
            bool wasMoving = movementRequested;
            movementRequested = isMoving;
            if (!isMoving)
            {
                if (wasMoving && state != PresentationState.Attack)
                    HoldNeutral();
                return;
            }
            inactiveSeconds = 0f;
            if (state != PresentationState.Attack)
                PlayState(PresentationState.Walk, walkStateName, 1f);
        }

        public void PlayAttack(float authoritativeWindupSeconds)
        {
            inactiveSeconds = 0f;
            PlayState(PresentationState.Attack, attackStateName,
                CalculateAttackSpeed(authoredImpactTimeSeconds, authoritativeWindupSeconds));
        }

        public void CancelAttack() => HoldNeutral();

        public void CompleteAttack()
        {
            if (movementRequested)
                PlayState(PresentationState.Walk, walkStateName, 1f);
            else
                HoldNeutral();
        }

        public void Advance(float deltaTime)
        {
            if (state == PresentationState.Attack || movementRequested)
                return;
            inactiveSeconds += Mathf.Max(0f, deltaTime);
            if (inactiveSeconds >= idleDelaySeconds && state != PresentationState.Idle)
                PlayState(PresentationState.Idle, idleStateName, 1f);
        }

        public static float CalculateAttackSpeed(float authoredImpactTimeSeconds, float authoritativeWindupSeconds)
        {
            if (authoredImpactTimeSeconds <= 0f || authoritativeWindupSeconds <= 0f)
                return 1f;
            return authoredImpactTimeSeconds / authoritativeWindupSeconds;
        }

        private void HoldNeutral()
        {
            state = PresentationState.Hold;
            inactiveSeconds = 0f;
            if (animator == null || string.IsNullOrWhiteSpace(idleStateName))
                return;
            animator.speed = 0f;
            animator.Play(idleStateName, 0, 0f);
            animator.Update(0f);
        }

        private void PlayState(PresentationState nextState, string stateName, float speed)
        {
            state = nextState;
            if (animator == null || string.IsNullOrWhiteSpace(stateName))
                return;
            animator.speed = Mathf.Max(0f, speed);
            animator.Play(stateName, 0, 0f);
        }
    }
}

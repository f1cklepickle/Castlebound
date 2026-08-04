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
        [SerializeField] private string normalizedProgressParameter = "AttackProgress";
        [SerializeField, Min(0.01f)] private float authoredImpactTimeSeconds = 0.33333334f;
        [SerializeField, Min(0.01f)] private float authoredAttackDurationSeconds = 0.35f;
        [SerializeField, Min(0f)] private float idleDelaySeconds = 2f;

        private PresentationState state;
        private float inactiveSeconds;
        private bool movementRequested;
        private float authoritativeImpactProgress = 0.5f;

        public float AuthoredImpactTimeSeconds => authoredImpactTimeSeconds;
        public float AuthoredAttackDurationSeconds => authoredAttackDurationSeconds;
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

        public void PlayAttack(float authoritativeWindupSeconds, float authoritativeDurationSeconds)
        {
            inactiveSeconds = 0f;
            authoritativeImpactProgress = authoritativeDurationSeconds > 0f
                ? Mathf.Clamp01(authoritativeWindupSeconds / authoritativeDurationSeconds)
                : 0f;
            if (animator != null && HasFloatParameter(animator, normalizedProgressParameter))
                animator.SetFloat(normalizedProgressParameter, 0f);
            PlayState(PresentationState.Attack, attackStateName, 1f);
            ApplyAttackProgress(0f);
        }

        public void ApplyAttackProgress(float normalizedAttackProgress)
        {
            if (state != PresentationState.Attack || animator == null || string.IsNullOrWhiteSpace(attackStateName))
                return;

            float authoredImpactProgress = Mathf.Clamp01(
                authoredImpactTimeSeconds / Mathf.Max(0.01f, authoredAttackDurationSeconds));
            float presentationProgress = MapAttackProgress(
                normalizedAttackProgress,
                authoritativeImpactProgress,
                authoredImpactProgress);

            if (HasFloatParameter(animator, normalizedProgressParameter))
            {
                animator.SetFloat(normalizedProgressParameter, presentationProgress);
            }
            else
            {
                animator.Play(attackStateName, 0, presentationProgress);
            }
            animator.Update(0f);
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

        public static float MapAttackProgress(
            float attackProgress,
            float authoritativeImpactProgress,
            float authoredImpactProgress)
        {
            float progress = Mathf.Clamp01(attackProgress);
            float sourceImpact = Mathf.Clamp01(authoritativeImpactProgress);
            float targetImpact = Mathf.Clamp01(authoredImpactProgress);

            if (progress <= sourceImpact && sourceImpact > 0f)
                return Mathf.Lerp(0f, targetImpact, progress / sourceImpact);
            if (sourceImpact >= 1f)
                return targetImpact;

            return Mathf.Lerp(
                targetImpact,
                1f,
                (progress - sourceImpact) / Mathf.Max(0.0001f, 1f - sourceImpact));
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

        private static bool HasFloatParameter(Animator targetAnimator, string parameterName)
        {
            if (targetAnimator == null || string.IsNullOrWhiteSpace(parameterName))
                return false;

            var parameters = targetAnimator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName &&
                    parameters[i].type == AnimatorControllerParameterType.Float)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

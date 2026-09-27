using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Narrow movement-pipeline adapter. Navigation itself owns no combat/component state.
    [DisallowMultipleComponent]
    public sealed class EnemyNavigationChase : MonoBehaviour
    {
        private EnemyStaticNavigation navigation;
        private StaticNavigationRuntime runtime;
        private EnemyController2D controller;
        private EnemyLocomotion movement;
        private EnemyEngagement engagement;
        private CircleCollider2D bodyCollider;
        private EnemyRootReceiver root;
        private EnemyStaggerReceiver stagger;
        private EnemyKnockbackReceiver knockback;
        private Health health;
        private EnemySurroundEligibility eligibility;
        private Transform cachedTarget;
        private Collider2D[] targetColliders;
        private readonly EnemyPredictiveChase avoidance = new EnemyPredictiveChase();
        private readonly Dictionary<Vector2, bool> feasible = new Dictionary<Vector2, bool>();
        private Vector2 solveCenter;
        private float horizon;
        private readonly EnemyNavigationRecovery recovery = new EnemyNavigationRecovery();
        public bool IsRecovering => recovery.IsActive;
        public EnemyStaticNavigation Navigation => navigation;
        public bool GuardMovement { get; private set; }
#if UNITY_EDITOR
        public Vector2 DebugPreferredVelocity { get; private set; }
        public Vector2 DebugAvoidanceVelocity { get; private set; }
        public Vector2 DebugRequestedDisplacement { get; private set; }
        public Vector2 DebugAllowedDisplacement { get; private set; }
        public Vector2 DebugGuardCenter { get; private set; }
        public StaticNavigationSampleState DebugGuardState { get; private set; }
        public float DebugMovementTime { get; private set; }
#endif

        private void Awake()
        {
            controller = GetComponent<EnemyController2D>(); movement = GetComponent<EnemyLocomotion>();
            engagement = GetComponent<EnemyEngagement>(); bodyCollider = GetComponent<CircleCollider2D>();
            root = GetComponent<EnemyRootReceiver>(); stagger = GetComponent<EnemyStaggerReceiver>();
            knockback = GetComponent<EnemyKnockbackReceiver>(); health = GetComponent<Health>();
            eligibility = GetComponent<EnemySurroundEligibility>();
        }
        public bool Apply(float speed, float dt, ref Vector2 radial, ref Vector2 tangent)
        {
            GuardMovement = false;
            if (!isActiveAndEnabled || movement == null || movement.HoldMovementPolicySource != null ||
                eligibility == null || !eligibility.isActiveAndEnabled || eligibility.AvoidanceGroup == PredictiveAvoidanceGroup.None)
            { Suspend(); return false; }
            if (controller == null || !controller.isActiveAndEnabled || health == null || health.Current <= 0 ||
                controller.Target == null || !controller.Target.gameObject.activeInHierarchy ||
                controller.CurrentTargetType == EnemyTargetType.None)
            { Suspend(); radial = tangent = Vector2.zero; return true; }
            if (root != null && root.IsRooted || stagger != null && stagger.IsActionLocked)
            { Suspend(); radial = tangent = Vector2.zero; return true; }
            if (movement.CurrentState != EnemyController2D.State.CHASE)
            { Suspend(); return true; }
            if (runtime != StaticNavigationRuntime.Instance || navigation == null)
            {
                recovery.Reset();
                navigation?.Dispose(); runtime = StaticNavigationRuntime.Instance;
                navigation = runtime != null && runtime.Cache != null ? new EnemyStaticNavigation(runtime) : null;
            }
            if (runtime == null || !runtime.isActiveAndEnabled || runtime.Cache == null || runtime.Scheduler == null ||
                navigation == null || bodyCollider == null || !bodyCollider.enabled || bodyCollider.isTrigger ||
                !Mathf.Approximately(bodyCollider.radius * Mathf.Abs(bodyCollider.transform.lossyScale.x), runtime.BodyRadius) ||
                !Mathf.Approximately(bodyCollider.radius * Mathf.Abs(bodyCollider.transform.lossyScale.y), runtime.BodyRadius))
            {
                // Optional navigation must leave the controller's existing chase path available.
                Suspend(); navigation?.Dispose(); navigation = null;
                return false;
            }
            GuardMovement = true;
            Transform target = controller.Target;
            var targetHealth = target.GetComponent<Health>();
            if (targetHealth != null && targetHealth.Current <= 0) { Suspend(); radial = tangent = Vector2.zero; return true; }
            bool wasRecovering = recovery.IsActive;
            bool recoveryOwnsStep = runtime.Recover(recovery, bodyCollider, speed, dt,
                knockback != null && knockback.IsActive, out Vector2 escape);
            if (wasRecovering != recovery.IsActive) navigation.ResetAfterRecovery();
            if (recoveryOwnsStep)
            {
                radial = escape; tangent = Vector2.zero;
#if UNITY_EDITOR
                DebugPreferredVelocity = DebugAvoidanceVelocity = escape;
#endif
                movement.ResetChaseApproachTarget();
                return true;
            }
            if (cachedTarget != target)
            { cachedTarget = target; targetColliders = target.GetComponentsInChildren<Collider2D>(); }
            var barrier = target.GetComponent<BarrierHealth>();
            var approach = target.GetComponent<EnemyBarrierHoldBehavior>();
            var region = GetComponent<EnemyRegionState>();
            bool passage = controller.CurrentTargetType == EnemyTargetType.Barrier && barrier != null && barrier.IsBroken;
            var descriptor = new EnemyNavigationTarget(target, controller.CurrentTargetType, targetColliders,
                engagement.EngagementDistance, passage, controller.CurrentTargetType == EnemyTargetType.Barrier &&
                region != null && region.PlayerInside, approach != null ? approach.ApproachPosition : (Vector2)target.position,
                approach != null && approach.HasApproachAnchor);
            solveCenter = bodyCollider.bounds.center;
            Vector2 preferred = navigation.Compute(descriptor, solveCenter, speed, dt, Time.time,
                knockback: knockback != null && knockback.IsActive);
            feasible.Clear();
            // Do not project past a turn/terminal approach point as if the same velocity persisted there.
            horizon = Mathf.Max(dt, Mathf.Min(EnemyPredictiveAvoidance.PredictionHorizon,
                Vector2.Distance(solveCenter, navigation.SteeringPoint) / Mathf.Max(speed, 0.001f)));
            radial = avoidance.Compute(controller, target, preferred, IsFeasible);
#if UNITY_EDITOR
            DebugPreferredVelocity = preferred;
            DebugAvoidanceVelocity = radial;
#endif
            tangent = Vector2.zero;
            movement.ResetChaseApproachTarget(); movement.ResetPredictiveChase();
            return true;
        }
        private bool IsFeasible(Vector2 velocity)
        {
            if (velocity.sqrMagnitude == 0f) return true;
            if (feasible.TryGetValue(velocity, out bool safe)) return safe;
            safe = runtime.Segment(solveCenter, solveCenter + velocity * horizon) == StaticNavigationSampleState.Clear;
            feasible.Add(velocity, safe); return safe;
        }
        public Vector2 ConstrainFinalDisplacement(Vector2 displacement)
        {
#if UNITY_EDITOR
            DebugMovementTime = Time.fixedTime;
            DebugRequestedDisplacement = displacement;
            DebugAllowedDisplacement = displacement;
            DebugGuardState = StaticNavigationSampleState.Unknown;
#endif
            if (!GuardMovement || displacement.sqrMagnitude == 0f) return displacement;
            if (runtime == null || bodyCollider == null) return Vector2.zero;
            if (recovery.IsActive)
            {
                Vector2 recovered = runtime.ConstrainRecovery(recovery, bodyCollider, displacement);
#if UNITY_EDITOR
                DebugGuardCenter = bodyCollider.bounds.center;
                DebugAllowedDisplacement = recovered;
#endif
                return recovered;
            }
            Vector2 center = bodyCollider.bounds.center;
            var state = runtime.Segment(center, center + displacement);
            Vector2 allowed = state == StaticNavigationSampleState.Clear ? displacement : Vector2.zero;
#if UNITY_EDITOR
            DebugGuardCenter = center;
            DebugGuardState = state;
            DebugAllowedDisplacement = allowed;
#endif
            return allowed;
        }
        public void Suspend()
        {
            recovery.Reset();
            navigation?.Suspend(); avoidance.Reset(); GuardMovement = false;
        }
        private void OnDisable() => Suspend();
        private void OnDestroy() => navigation?.Dispose();
    }
}

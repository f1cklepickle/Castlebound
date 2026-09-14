using UnityEngine;
using Castlebound.Gameplay.AI;

public class EnemyLocomotion : MonoBehaviour
{
    [SerializeField] private EnemyKnockbackReceiver knockbackReceiver;
    [SerializeField] private EnemyRootReceiver rootReceiver;
    [SerializeField] private MonoBehaviour holdMovementPolicySource;

    private EnemySeparationCollider separationCollider;
    private float previousDistance;
    private int distanceTrend;
    private Vector2 lastNonZeroDirection = Vector2.right;
    private IEnemyHoldMovementPolicy holdMovementPolicy;
    private readonly EnemyChaseApproachTarget chaseApproachTarget = new EnemyChaseApproachTarget();
    private readonly EnemyBypassDirectionTransition bypassDirectionTransition = new EnemyBypassDirectionTransition();
    public bool HasChaseApproachTarget => chaseApproachTarget.IsActive;
    public bool IsChaseApproachTurning => bypassDirectionTransition.IsTurning;

    public EnemyController2D.State CurrentState { get; private set; } = EnemyController2D.State.CHASE;
    public bool IsChaseRequested { get; private set; }
    public bool IsInHoldRange => CurrentState == EnemyController2D.State.HOLD;
    public MonoBehaviour HoldMovementPolicySource => holdMovementPolicySource;

    public void RequestChase()
    {
        IsChaseRequested = true;
        CurrentState = EnemyController2D.State.CHASE;
    }

    public void ClearChaseRequest()
    {
        IsChaseRequested = false;
    }

    public void SetMovementState(EnemyController2D.State state)
    {
        CurrentState = state;
    }

    public void ComputeBaseMovement(
        Vector2 position,
        Transform steerTarget,
        Transform barrier,
        float surfaceDistance,
        float engagementDistance,
        float releaseMargin,
        float reseatBias,
        float speed,
        float orbitBase,
        float maxTangent,
        int outrunFrames,
        float epsilonDistance,
        float gapClockwise,
        float gapCounterClockwise,
        out Vector2 radial,
        out Vector2 tangent)
    {
        EnemyController2D.State movementState = CurrentState;
        EnemyMovement.ComputeMovement(
            position,
            steerTarget,
            barrier,
            surfaceDistance,
            engagementDistance,
            releaseMargin,
            reseatBias,
            speed,
            orbitBase,
            maxTangent,
            outrunFrames,
            epsilonDistance,
            gapClockwise,
            gapCounterClockwise,
            ref movementState,
            ref previousDistance,
            ref distanceTrend,
            ref lastNonZeroDirection,
            out radial,
            out tangent);
        CurrentState = movementState;
        // Settled melee Player HOLD does not rebalance from live gaps.
        // Explicit HOLD policies (ranged) and barrier movement keep their existing behavior.
        if (CurrentState == EnemyController2D.State.HOLD &&
            steerTarget != null && steerTarget.CompareTag("Player") &&
            (barrier == null || steerTarget != barrier) &&
            ResolveHoldMovementPolicy() == null)
        {
            tangent = Vector2.zero;
        }

        if (CurrentState == EnemyController2D.State.HOLD)
        {
            ResolveHoldMovementPolicy()?.Apply(default, ref radial, ref tangent);
        }
    }

    public void ApplyHoldMovementPolicy(
        EnemyHoldMovementContext context,
        ref Vector2 radial,
        ref Vector2 tangent)
    {
        if (CurrentState != EnemyController2D.State.HOLD)
            return;

        ResolveHoldMovementPolicy()?.Apply(context, ref radial, ref tangent);
    }

    public void ApplyHoldMovementPolicy(
        Vector2 position,
        Transform target,
        Vector2 localSeparation,
        bool hasNeighbors,
        Vector2 stableBias,
        float speed,
        ref Vector2 radial,
        ref Vector2 tangent)
    {
        Vector2 toTarget = target != null
            ? (Vector2)target.position - position
            : Vector2.zero;
        Vector2 directionToTarget = toTarget.sqrMagnitude > 0f
            ? toTarget.normalized
            : Vector2.zero;
        ApplyHoldMovementPolicy(
            new EnemyHoldMovementContext(
                directionToTarget,
                localSeparation,
                hasNeighbors,
                stableBias,
                speed),
            ref radial,
            ref tangent);
    }

    public void ResetChaseApproachTarget()
    {
        chaseApproachTarget.Reset();
        bypassDirectionTransition.Reset();
    }

    public bool TryApplyChaseApproachTarget(EnemyController2D owner, Transform player,
        bool surroundEligible, Vector2 stableBias, float surfaceDistance, float engagementDistance,
        float speed, float deltaTime, ref Vector2 radial, ref Vector2 tangent, out Vector2 point)
    {
        point = player != null ? (Vector2)player.position : Vector2.zero;
        var body = GetComponent<Rigidbody2D>();
        Vector2 position = body != null ? body.position : (Vector2)transform.position;
        if (!isActiveAndEnabled || CurrentState != EnemyController2D.State.CHASE ||
            !surroundEligible || ResolveHoldMovementPolicy() != null || owner == null ||
            player == null || owner.CurrentTargetType != EnemyTargetType.Player || owner.Target != player)
        {
            ResetChaseApproachTarget();
            return false;
        }

        bool hadBypassDirection = chaseApproachTarget.IsActive || bypassDirectionTransition.IsTurning;
        Vector2 ordinaryChase = radial + tangent;
        bool bypassing = chaseApproachTarget.TryGetTarget(owner, player, position, stableBias,
            surfaceDistance, engagementDistance, out point);
        if (bypassing)
        {
            Vector2 toPoint = point - position;
            // Preserve the existing waypoint speed cap and target direction as the desired request.
            float stepSpeed = deltaTime > 0f ? Mathf.Min(Mathf.Max(0f, speed), toPoint.magnitude / deltaTime) : 0f;
            radial = toPoint.normalized * stepSpeed;
            tangent = Vector2.zero;
        }

        Vector2 desired = radial + tangent;
        Vector2 turned = bypassDirectionTransition.Apply(desired, bypassing, deltaTime,
            ordinaryChase, player.GetInstanceID(), chaseApproachTarget.WaypointChanged);
        // Bypass and its transitions replace direction; they never add speed or restore
        // speed after #277. Ordinary CHASE keeps its existing #299 composition.
        if (bypassing || hadBypassDirection)
            turned = Vector2.ClampMagnitude(turned, Mathf.Max(0f, speed));
        if (!turned.Equals(desired))
        {
            radial = turned;
            tangent = Vector2.zero;
        }
        // On route release, retain direction history until the exit turn finishes.
        // ExecuteMovement still applies #277 to the final request without modification.
        return bypassing;
    }

    private void OnDisable()
    {
        ResetChaseApproachTarget();
    }

    private IEnemyHoldMovementPolicy ResolveHoldMovementPolicy()
    {
        if (holdMovementPolicy == null && holdMovementPolicySource != null)
            holdMovementPolicy = holdMovementPolicySource as IEnemyHoldMovementPolicy;

        return holdMovementPolicy;
    }

    public bool ExecuteMovement(Rigidbody2D body, Vector2 radial, Vector2 tangent, float deltaTime)
    {
        if (body == null)
            return false;

        if (rootReceiver == null)
            rootReceiver = GetComponent<EnemyRootReceiver>();
        if (rootReceiver != null && rootReceiver.IsRooted)
            return false;

        if (knockbackReceiver == null)
            knockbackReceiver = GetComponent<EnemyKnockbackReceiver>();

        Vector2 knockback = knockbackReceiver != null
            ? knockbackReceiver.ConsumeDisplacement(deltaTime)
            : Vector2.zero;
        Vector2 locomotionDisplacement = (radial + tangent) * deltaTime;
        if (separationCollider == null)
            separationCollider = GetComponentInChildren<EnemySeparationCollider>();
        if (separationCollider != null)
        {
            locomotionDisplacement = separationCollider
                .ConstrainLocomotionDisplacement(locomotionDisplacement);
        }

        Vector2 displacement = locomotionDisplacement + knockback;
        if (separationCollider != null)
        {
            separationCollider.RecordScheduledBodyPosition(
                body.position + displacement);
        }

        body.MovePosition(body.position + displacement);
        return displacement.sqrMagnitude > Mathf.Epsilon;
    }

#if UNITY_EDITOR
    public void Debug_SetHoldMovementPolicy(MonoBehaviour source)
    {
        holdMovementPolicySource = source;
        holdMovementPolicy = source as IEnemyHoldMovementPolicy;
    }
#endif
}

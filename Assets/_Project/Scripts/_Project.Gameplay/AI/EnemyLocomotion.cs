using UnityEngine;
using Castlebound.Gameplay.AI;

public class EnemyLocomotion : MonoBehaviour
{
    [SerializeField] private EnemyKnockbackReceiver knockbackReceiver;
    [SerializeField] private EnemyRootReceiver rootReceiver;
    [SerializeField] private MonoBehaviour holdMovementPolicySource;

    private float previousDistance;
    private int distanceTrend;
    private Vector2 lastNonZeroDirection = Vector2.right;
    private IEnemyHoldMovementPolicy holdMovementPolicy;

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
        Vector2 displacement = (radial + tangent) * deltaTime + knockback;
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

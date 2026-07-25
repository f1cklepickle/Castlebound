using UnityEngine;
using Castlebound.Gameplay.AI;

public class EnemyLocomotion : MonoBehaviour
{
    [SerializeField] private EnemyKnockbackReceiver knockbackReceiver;
    [SerializeField] private EnemyRootReceiver rootReceiver;

    private float previousDistance;
    private int distanceTrend;
    private Vector2 lastNonZeroDirection = Vector2.right;

    public EnemyController2D.State CurrentState { get; private set; } = EnemyController2D.State.CHASE;
    public bool IsChaseRequested { get; private set; }
    public bool IsInHoldRange => CurrentState == EnemyController2D.State.HOLD;

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
        float holdRadius,
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
            holdRadius,
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
    }

    public void ExecuteMovement(Rigidbody2D body, Vector2 radial, Vector2 tangent, float deltaTime)
    {
        if (body == null)
            return;

        if (rootReceiver == null)
            rootReceiver = GetComponent<EnemyRootReceiver>();
        if (rootReceiver != null && rootReceiver.IsRooted)
            return;

        if (knockbackReceiver == null)
            knockbackReceiver = GetComponent<EnemyKnockbackReceiver>();

        Vector2 knockback = knockbackReceiver != null
            ? knockbackReceiver.ConsumeDisplacement(deltaTime)
            : Vector2.zero;
        body.MovePosition(body.position + (radial + tangent) * deltaTime + knockback);
    }
}

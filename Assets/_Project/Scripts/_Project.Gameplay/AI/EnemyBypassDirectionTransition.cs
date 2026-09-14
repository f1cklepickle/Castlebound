using UnityEngine;

// Direction only, armed by entering/leaving bypass or replacing its waypoint. Ordinary CHASE is a passthrough.
public sealed class EnemyBypassDirectionTransition
{
    public const float TurnRateDegreesPerSecond = 225f;
    private Vector2 previousDirection;
    private bool previousWasBypassing;
    private int previousTargetId;
    public bool IsTurning { get; private set; }

    public void Reset()
    {
        previousDirection = Vector2.zero;
        previousWasBypassing = false;
        previousTargetId = 0;
        IsTurning = false;
    }

    public Vector2 Apply(Vector2 desiredVelocity, bool bypassing, float deltaTime,
        Vector2 entryDirection, int targetId = 0, bool waypointChanged = false)
    {
        if (previousTargetId != targetId) Reset();
        previousTargetId = targetId;
        if (desiredVelocity.sqrMagnitude <= 0f)
        {
            Reset();
            return desiredVelocity;
        }

        // A newly spawned/already blocked enemy has no preceding CHASE sample.
        if (previousDirection == Vector2.zero) previousDirection = entryDirection.normalized;
        if (bypassing != previousWasBypassing || (bypassing && waypointChanged))
            IsTurning = previousDirection != Vector2.zero;
        previousWasBypassing = bypassing;

        Vector2 result = desiredVelocity;
        if (IsTurning)
        {
            float currentAngle = Mathf.Atan2(previousDirection.y, previousDirection.x) * Mathf.Rad2Deg;
            float desiredAngle = Mathf.Atan2(desiredVelocity.y, desiredVelocity.x) * Mathf.Rad2Deg;
            float delta = Mathf.DeltaAngle(currentAngle, desiredAngle);
            float maximumTurn = TurnRateDegreesPerSecond * Mathf.Max(0f, deltaTime);
            if (Mathf.Abs(delta) <= maximumTurn)
            {
                // Return the exact existing request once aligned, including its original magnitude.
                IsTurning = false;
            }
            else
            {
                float angle = (currentAngle + Mathf.Clamp(delta, -maximumTurn, maximumTurn)) * Mathf.Deg2Rad;
                result = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * desiredVelocity.magnitude;
            }
        }
        previousDirection = result.normalized;
        return result;
    }
}

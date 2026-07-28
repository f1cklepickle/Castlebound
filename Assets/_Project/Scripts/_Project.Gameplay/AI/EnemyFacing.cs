using UnityEngine;

public class EnemyFacing : MonoBehaviour
{
    [SerializeField] private Transform visualTransform;
    [SerializeField, Min(0f)] private float turnSpeedDegreesPerSecond = 360f;
    [SerializeField, Range(0f, 180f)] private float attackAlignmentThreshold = 30f;
    [SerializeField] private Vector2 initialAimDirection = Vector2.down;

    public Vector2 AimDirection { get; private set; }

    private void Awake()
    {
        AimDirection = NormalizeOrZero(initialAimDirection);
        ApplyVisualDirection();
    }

    public void InitializeAimDirection(Vector2 direction)
    {
        AimDirection = NormalizeOrZero(direction);
        ApplyVisualDirection();
    }

    public void FaceTarget(Vector2 origin, Transform target, float deltaTime)
    {
        if (target == null)
            return;

        Vector2 targetDirection = (Vector2)target.position - origin;
        if (targetDirection.sqrMagnitude <= Mathf.Epsilon)
            return;

        AimDirection = TurnToward(
            AimDirection,
            targetDirection,
            turnSpeedDegreesPerSecond,
            deltaTime);
        ApplyVisualDirection();
    }

    public bool IsAlignedWith(Vector2 origin, Transform target)
    {
        if (target == null)
            return false;

        return IsDirectionAligned(
            AimDirection,
            (Vector2)target.position - origin,
            attackAlignmentThreshold);
    }

    public static bool IsDirectionAligned(
        Vector2 facingDirection,
        Vector2 targetDirection,
        float thresholdDegrees)
    {
        if (facingDirection.sqrMagnitude <= Mathf.Epsilon ||
            targetDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return false;
        }

        return Vector2.Angle(facingDirection, targetDirection) <=
               Mathf.Clamp(thresholdDegrees, 0f, 180f);
    }

    public static Vector2 TurnToward(
        Vector2 currentDirection,
        Vector2 targetDirection,
        float turnSpeedDegreesPerSecond,
        float deltaTime)
    {
        Vector2 target = NormalizeOrZero(targetDirection);
        if (target == Vector2.zero)
            return NormalizeOrZero(currentDirection);

        Vector2 current = NormalizeOrZero(currentDirection);
        if (current == Vector2.zero)
            return target;

        float maxDegreesDelta =
            Mathf.Max(0f, turnSpeedDegreesPerSecond) *
            Mathf.Max(0f, deltaTime);
        float currentAngle = Mathf.Atan2(current.y, current.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
        float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxDegreesDelta);
        float nextRadians = nextAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(nextRadians), Mathf.Sin(nextRadians));
    }

    public static float GetVisualRotationDegrees(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude <= Mathf.Epsilon)
            return 0f;

        return Vector2.SignedAngle(Vector2.down, aimDirection);
    }

    private void ApplyVisualDirection()
    {
        if (visualTransform == null || AimDirection == Vector2.zero)
            return;

        float angle = GetVisualRotationDegrees(AimDirection);
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private static Vector2 NormalizeOrZero(Vector2 direction)
    {
        return direction.sqrMagnitude > Mathf.Epsilon
            ? direction.normalized
            : Vector2.zero;
    }
}

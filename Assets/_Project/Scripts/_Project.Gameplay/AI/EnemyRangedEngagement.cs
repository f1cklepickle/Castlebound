using UnityEngine;

public class EnemyRangedEngagement : MonoBehaviour, IEnemyHoldMovementPolicy
{
    [SerializeField, Min(0f)] private float maxHoldSeparationSpeed = 1f;

    private EnemyApproachSpread approachSpread;

    public float MaxHoldSeparationSpeed => Mathf.Max(0f, maxHoldSeparationSpeed);

    public void Apply(EnemyHoldMovementContext context, ref Vector2 radial, ref Vector2 tangent)
    {
        tangent = Vector2.zero;

        if (approachSpread == null)
            approachSpread = GetComponent<EnemyApproachSpread>();
        if (approachSpread == null)
            return;

        Vector2 separation = approachSpread.ComputeHoldSeparation(
            context.DirectionToTarget,
            context.LocalSeparation,
            context.HasNeighbors,
            context.StableBias,
            context.Speed);
        tangent = Vector2.ClampMagnitude(separation, MaxHoldSeparationSpeed);
    }
}

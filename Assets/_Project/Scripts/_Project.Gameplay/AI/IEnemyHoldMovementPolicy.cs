using UnityEngine;

public interface IEnemyHoldMovementPolicy
{
    void Apply(EnemyHoldMovementContext context, ref Vector2 radial, ref Vector2 tangent);
}

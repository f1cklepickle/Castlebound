using UnityEngine;

public struct EnemyHoldMovementContext
{
    public EnemyHoldMovementContext(
        Vector2 directionToTarget,
        Vector2 localSeparation,
        bool hasNeighbors,
        Vector2 stableBias,
        float speed)
    {
        DirectionToTarget = directionToTarget;
        LocalSeparation = localSeparation;
        HasNeighbors = hasNeighbors;
        StableBias = stableBias;
        Speed = Mathf.Max(0f, speed);
    }

    public Vector2 DirectionToTarget { get; }
    public Vector2 LocalSeparation { get; }
    public bool HasNeighbors { get; }
    public Vector2 StableBias { get; }
    public float Speed { get; }
}

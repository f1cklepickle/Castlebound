using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public readonly struct EnemyAvoidanceNeighbor
    {
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly float HardRadius;

        public EnemyAvoidanceNeighbor(Vector2 position, Vector2 velocity, float hardRadius)
        {
            Position = position;
            Velocity = velocity;
            HardRadius = hardRadius;
        }
    }
}

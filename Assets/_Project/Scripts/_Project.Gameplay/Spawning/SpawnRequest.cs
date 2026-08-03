using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public readonly struct SpawnRequest
    {
        public string EnemyTypeId { get; }
        public string GateId { get; }
        public string LaneId { get; }
        public Vector2 Position { get; }
        public Vector2 ForwardDirection { get; }

        public SpawnRequest(string enemyTypeId, string gateId, Vector2 position)
            : this(enemyTypeId, gateId, "Default", position, Vector2.down)
        {
        }

        public SpawnRequest(string enemyTypeId, SpawnPoint spawnPoint)
            : this(enemyTypeId, spawnPoint.GateId, spawnPoint.LaneId, spawnPoint.Position, spawnPoint.ForwardDirection)
        {
        }

        public SpawnRequest(string enemyTypeId, string gateId, string laneId, Vector2 position, Vector2 forwardDirection)
        {
            EnemyTypeId = EnemyArchetypeIds.Canonicalize(enemyTypeId);
            GateId = gateId;
            LaneId = laneId;
            Position = position;
            ForwardDirection = forwardDirection;
        }
    }
}

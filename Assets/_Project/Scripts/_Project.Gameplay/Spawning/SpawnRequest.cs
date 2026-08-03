using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Gameplay.Spawning
{
    public readonly struct SpawnRequest
    {
        public string EnemyTypeId { get; }
        public string GateId { get; }
        public string LaneId { get; }
        public Vector2 Position { get; }
        public Vector2 ForwardDirection { get; }
        public EnemyEquipmentDefinition Equipment { get; }

        public SpawnRequest(string enemyTypeId, string gateId, Vector2 position)
            : this(enemyTypeId, gateId, "Default", position, Vector2.down, null)
        {
        }

        public SpawnRequest(string enemyTypeId, SpawnPoint spawnPoint, EnemyEquipmentDefinition equipment = null)
            : this(
                enemyTypeId,
                spawnPoint.GateId,
                spawnPoint.LaneId,
                spawnPoint.Position,
                spawnPoint.ForwardDirection,
                equipment)
        {
        }

        public SpawnRequest(string enemyTypeId, string gateId, string laneId, Vector2 position, Vector2 forwardDirection)
            : this(enemyTypeId, gateId, laneId, position, forwardDirection, null)
        {
        }

        public SpawnRequest(
            string enemyTypeId,
            string gateId,
            string laneId,
            Vector2 position,
            Vector2 forwardDirection,
            EnemyEquipmentDefinition equipment)
        {
            EnemyTypeId = EnemyArchetypeIds.Canonicalize(enemyTypeId);
            GateId = gateId;
            LaneId = laneId;
            Position = position;
            ForwardDirection = forwardDirection;
            Equipment = equipment;
        }
    }
}

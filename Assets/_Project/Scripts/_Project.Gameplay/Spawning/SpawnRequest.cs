using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public readonly struct SpawnRequest
    {
        public string EnemyTypeId { get; }
        public string GateId { get; }
        public Vector2 Position { get; }

        public SpawnRequest(string enemyTypeId, string gateId, Vector2 position)
        {
            EnemyTypeId = enemyTypeId;
            GateId = gateId;
            Position = position;
        }
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public readonly struct SpawnPoint
    {
        public string GateId { get; }
        public Vector2 Position { get; }

        public SpawnPoint(string gateId, Vector2 position)
        {
            GateId = gateId;
            Position = position;
        }
    }
}

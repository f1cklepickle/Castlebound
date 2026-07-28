using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public readonly struct SpawnPoint
    {
        public string GateId { get; }
        public string LaneId { get; }
        public Vector2 Position { get; }
        public Vector2 ForwardDirection { get; }

        public SpawnPoint(string gateId, Vector2 position)
            : this(gateId, "Default", position, Vector2.down)
        {
        }

        public SpawnPoint(string gateId, string laneId, Vector2 position, Vector2 forwardDirection)
        {
            GateId = gateId;
            LaneId = laneId;
            Position = position;
            ForwardDirection = forwardDirection.sqrMagnitude > Mathf.Epsilon
                ? forwardDirection.normalized
                : Vector2.zero;
        }
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class SpawnPointMarker : MonoBehaviour
    {
        [SerializeField] private string laneId = "Center";

        public void Initialize(string lane)
        {
            laneId = lane;
        }

        public SpawnPoint ToSpawnPoint()
        {
            var provider = GetComponentInParent<GateIdProvider>();
            var id = provider != null ? provider.GateId : string.Empty;

            return new SpawnPoint(id, laneId, (Vector2)transform.position, -transform.up);
        }
    }
}

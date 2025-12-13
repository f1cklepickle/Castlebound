using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class SpawnPointMarker : MonoBehaviour
    {
        [SerializeField] private string gateId;

        public SpawnPoint ToSpawnPoint()
        {
            return new SpawnPoint(gateId, (Vector2)transform.position);
        }
    }
}

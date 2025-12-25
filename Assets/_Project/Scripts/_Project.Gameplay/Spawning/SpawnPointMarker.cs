using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class SpawnPointMarker : MonoBehaviour
    {
        [SerializeField] private string gateId;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(gateId))
            {
                var provider = GetComponentInParent<GateIdProvider>();
                if (provider != null)
                {
                    gateId = provider.GateId;
                }
            }
        }

        public SpawnPoint ToSpawnPoint()
        {
            var id = gateId;
            if (string.IsNullOrWhiteSpace(id))
            {
                var provider = GetComponentInParent<GateIdProvider>();
                if (provider != null)
                {
                    id = provider.GateId;
                }
            }

            return new SpawnPoint(id, (Vector2)transform.position);
        }
    }
}

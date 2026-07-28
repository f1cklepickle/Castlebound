using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class GateIdProvider : MonoBehaviour
    {
        [SerializeField] private string gateId;

        public string GateId => gateId;

        public void Initialize(string id)
        {
            gateId = id;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(gateId))
            {
                gateId = gameObject.name;
            }
        }
    }
}

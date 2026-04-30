using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class TowerPlot : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private GameObject occupantInstance;

        public Transform Anchor => anchor != null ? anchor : transform;
        public GameObject OccupantInstance => occupantInstance;
        public bool IsOccupied => occupantInstance != null;

        public void SetAnchor(Transform value)
        {
            anchor = value;
        }

        public bool TryAssignOccupant(GameObject instance)
        {
            if (instance == null || occupantInstance != null)
            {
                return false;
            }

            occupantInstance = instance;
            return true;
        }

        public void ClearOccupant(GameObject instance = null)
        {
            if (instance == null || occupantInstance == instance)
            {
                occupantInstance = null;
            }
        }
    }
}

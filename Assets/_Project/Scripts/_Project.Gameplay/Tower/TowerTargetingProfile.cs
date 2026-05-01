using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [CreateAssetMenu(menuName = "Castlebound/Tower/Tower Targeting Profile")]
    public class TowerTargetingProfile : ScriptableObject
    {
        [SerializeField] private float minRange;
        [SerializeField] private float maxRange = 5f;
        [SerializeField] private float scanInterval = 0.2f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private TowerTargetSelectionMode selectionMode;

        public float MinRange
        {
            get => minRange;
            set
            {
                minRange = Mathf.Max(0f, value);
                maxRange = Mathf.Max(minRange, maxRange);
            }
        }

        public float MaxRange
        {
            get => maxRange;
            set => maxRange = Mathf.Max(minRange, value);
        }

        public float ScanInterval
        {
            get => scanInterval;
            set => scanInterval = Mathf.Max(0.02f, value);
        }

        public LayerMask TargetLayers
        {
            get => targetLayers;
            set => targetLayers = value;
        }

        public TowerTargetSelectionMode SelectionMode
        {
            get => selectionMode;
            set => selectionMode = value;
        }

        private void OnValidate()
        {
            minRange = Mathf.Max(0f, minRange);
            maxRange = Mathf.Max(minRange, maxRange);
            scanInterval = Mathf.Max(0.02f, scanInterval);
        }
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [CreateAssetMenu(menuName = "Castlebound/Tower/Tower Targeting Profile")]
    public class TowerTargetingProfile : ScriptableObject
    {
        [SerializeField] private float scanInterval = 0.2f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private TowerTargetSelectionMode selectionMode;

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
            scanInterval = Mathf.Max(0.02f, scanInterval);
        }
    }
}

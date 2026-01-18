using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    [CreateAssetMenu(menuName = "Castlebound/Barrier/Barrier Upgrade State")]
    public class BarrierUpgradeStateAsset : ScriptableObject
    {
        [SerializeField] private int tier;

        public int Tier
        {
            get => tier;
            set => tier = Mathf.Max(0, value);
        }

        public void IncrementTier()
        {
            Tier++;
        }
    }
}

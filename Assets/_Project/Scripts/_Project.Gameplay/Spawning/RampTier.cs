using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    [System.Serializable]
    public struct RampTier
    {
        public string enemyTypeId;
        public float weight;

        [Tooltip("0 means use weighted share of the generated wave count.")]
        public int baseSpawnCount;

        [Tooltip("How many enemies of this type to add each step when baseSpawnCount is set.")]
        public int countPerStep;

        [Tooltip("Apply countPerStep every N waves after this tier unlocks.")]
        public int stepSize;

        [Tooltip("Optional seeded equipment distribution for enemies emitted by this tier.")]
        public EnemyEquipmentLoadoutTable equipmentLoadout;
    }
}

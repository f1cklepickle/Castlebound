using Castlebound.Gameplay.AI;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    [System.Serializable]
    public struct EnemyEquipmentLoadoutEntry
    {
        [SerializeField] private EnemyEquipmentDefinition equipment;
        [SerializeField, Min(1)] private int startWave;
        [SerializeField, Min(0f)] private float startWeight;
        [SerializeField, Min(1)] private int endWave;
        [SerializeField, Min(0f)] private float endWeight;

        public EnemyEquipmentLoadoutEntry(
            EnemyEquipmentDefinition equipment,
            int startWave,
            float startWeight,
            int endWave,
            float endWeight)
        {
            this.equipment = equipment;
            this.startWave = Mathf.Max(1, startWave);
            this.startWeight = Mathf.Max(0f, startWeight);
            this.endWave = Mathf.Max(this.startWave, endWave);
            this.endWeight = Mathf.Max(0f, endWeight);
        }

        public EnemyEquipmentDefinition Equipment => equipment;

        public float GetWeight(int waveIndex)
        {
            int safeStartWave = Mathf.Max(1, startWave);
            int safeEndWave = Mathf.Max(safeStartWave, endWave);
            float safeStartWeight = Mathf.Max(0f, startWeight);
            float safeEndWeight = Mathf.Max(0f, endWeight);

            if (safeEndWave == safeStartWave)
            {
                return waveIndex < safeStartWave ? safeStartWeight : safeEndWeight;
            }

            float progress = Mathf.InverseLerp(safeStartWave, safeEndWave, waveIndex);
            return Mathf.Lerp(safeStartWeight, safeEndWeight, progress);
        }
    }
}

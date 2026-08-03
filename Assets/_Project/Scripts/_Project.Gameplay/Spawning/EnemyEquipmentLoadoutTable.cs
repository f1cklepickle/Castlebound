using Castlebound.Gameplay.AI;
using UnityEngine;
using Random = System.Random;

namespace Castlebound.Gameplay.Spawning
{
    [CreateAssetMenu(fileName = "EnemyEquipmentLoadout", menuName = "Spawning/Enemy Equipment Loadout")]
    public class EnemyEquipmentLoadoutTable : ScriptableObject
    {
        [SerializeField] private EnemyEquipmentLoadoutEntry[] entries = System.Array.Empty<EnemyEquipmentLoadoutEntry>();

        public EnemyEquipmentLoadoutEntry[] Entries
        {
            get => entries;
            set => entries = value ?? System.Array.Empty<EnemyEquipmentLoadoutEntry>();
        }

        public EnemyEquipmentDefinition Select(Random random, int waveIndex)
        {
            if (random == null || entries == null || entries.Length == 0)
            {
                return null;
            }

            float totalWeight = GetTotalWeight(waveIndex);
            if (totalWeight <= 0f)
            {
                return null;
            }

            double roll = random.NextDouble() * totalWeight;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Equipment == null)
                {
                    continue;
                }

                float weight = entries[i].GetWeight(waveIndex);
                if (weight <= 0f)
                {
                    continue;
                }

                if (roll < weight)
                {
                    return entries[i].Equipment;
                }

                roll -= weight;
            }

            return null;
        }

        public float GetSelectionChance(EnemyEquipmentDefinition equipment, int waveIndex)
        {
            if (equipment == null || entries == null)
            {
                return 0f;
            }

            float totalWeight = GetTotalWeight(waveIndex);
            if (totalWeight <= 0f)
            {
                return 0f;
            }

            float equipmentWeight = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Equipment == equipment)
                {
                    equipmentWeight += Mathf.Max(0f, entries[i].GetWeight(waveIndex));
                }
            }

            return equipmentWeight / totalWeight;
        }

        private float GetTotalWeight(int waveIndex)
        {
            float totalWeight = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Equipment != null)
                {
                    totalWeight += Mathf.Max(0f, entries[i].GetWeight(waveIndex));
                }
            }

            return totalWeight;
        }
    }
}

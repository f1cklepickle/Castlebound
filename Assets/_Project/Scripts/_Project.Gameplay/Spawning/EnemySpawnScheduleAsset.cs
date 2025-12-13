using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    [CreateAssetMenu(fileName = "EnemySpawnSchedule", menuName = "Spawning/Enemy Spawn Schedule")]
    public class EnemySpawnScheduleAsset : ScriptableObject
    {
        [System.Serializable]
        public struct SpawnSequenceConfig
        {
            public string enemyTypeId;
            public int spawnCount;
            public float intervalSeconds;
            public float initialDelaySeconds;
        }

        [SerializeField] private List<SpawnSequenceConfig> sequences = new List<SpawnSequenceConfig>();

        public EnemySpawnSchedule ToRuntimeSchedule()
        {
            var runtimeSequences = new List<SpawnSequence>();
            foreach (var seq in sequences)
            {
                runtimeSequences.Add(new SpawnSequence(seq.enemyTypeId, seq.spawnCount, seq.intervalSeconds, seq.initialDelaySeconds));
            }
            return new EnemySpawnSchedule(runtimeSequences);
        }
    }
}

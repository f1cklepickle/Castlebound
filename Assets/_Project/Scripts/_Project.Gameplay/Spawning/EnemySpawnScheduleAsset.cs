using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public enum SpawnMarkerStrategy
    {
        RoundRobin = 0,
        ShufflePrecompute = 1
    }

    [System.Serializable]
    public struct SpawnSequenceConfig
    {
        public string enemyTypeId;
        public int spawnCount;
        public float intervalSeconds;
        public float initialDelaySeconds;
    }

    [System.Serializable]
    public class WaveConfig
    {
        public List<SpawnSequenceConfig> sequences = new List<SpawnSequenceConfig>();

        [Header("Overrides")]
        public bool useStrategyOverride;
        public SpawnMarkerStrategy strategyOverride = SpawnMarkerStrategy.RoundRobin;

        public bool useSeedOverride;
        public int seedOverride;

        [Header("Pacing")]
        public float gapSeconds = 5f;
        public bool waitForClear = true;

        [Tooltip("0 means no cap")]
        public int maxAlive = 0;
    }

    [System.Serializable]
    public struct RampTier
    {
        public string enemyTypeId;
        public float weight;
    }

    [System.Serializable]
    public struct RampTierUnlock
    {
        public int waveIndex;
        public List<RampTier> tiers;
    }

    [System.Serializable]
    public class RampConfig
    {
        public int baseSpawnCount = 5;

        [Tooltip("How many enemies to add each step")]
        public int countPerStep = 1;

        [Tooltip("Apply countPerStep every N waves")]
        public int stepSize = 1;

        [Tooltip("Wave index to start ramping (1-based)")]
        public int startWave = 1;

        public List<RampTierUnlock> unlocks = new List<RampTierUnlock>();
    }

    [CreateAssetMenu(fileName = "EnemySpawnSchedule", menuName = "Spawning/Enemy Spawn Schedule")]
    public class EnemySpawnScheduleAsset : ScriptableObject
    {
        [Header("Defaults")]
        [SerializeField] private SpawnMarkerStrategy defaultStrategy = SpawnMarkerStrategy.RoundRobin;

        [Tooltip("Set true to use defaultSeed value")]
        [SerializeField] private bool useDefaultSeed;
        [SerializeField] private int defaultSeed;

        [Header("Authored Waves")]
        [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>();

        [Header("Optional Ramp")]
        [SerializeField] private RampConfig ramp;

        // Legacy sequences kept for backward compatibility; will be removed once waves/ramp are fully wired.
        [SerializeField, HideInInspector] private List<SpawnSequenceConfig> sequences = new List<SpawnSequenceConfig>();

        public EnemySpawnSchedule ToRuntimeSchedule()
        {
            // Temporary: preserve existing behavior until the new wave/ramp pipeline is wired.
            var runtimeSequences = new List<SpawnSequence>();
            foreach (var seq in sequences)
            {
                runtimeSequences.Add(new SpawnSequence(seq.enemyTypeId, seq.spawnCount, seq.intervalSeconds, seq.initialDelaySeconds));
            }
            return new EnemySpawnSchedule(runtimeSequences);
        }

        public WaveScheduleRuntime ToRuntimeWaveSchedule()
        {
            var seed = useDefaultSeed ? defaultSeed : 0;
            return new WaveScheduleRuntime(
                defaultStrategy: defaultStrategy,
                defaultSeed: seed,
                waves: waves,
                ramp: ramp);
        }
    }
}

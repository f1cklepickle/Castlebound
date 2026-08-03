using System.Collections.Generic;
using Castlebound.Gameplay.Balance;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    [CreateAssetMenu(fileName = "EnemySpawnSchedule", menuName = "Spawning/Enemy Spawn Schedule")]
    public class EnemySpawnScheduleAsset : ScriptableObject
    {
        [Header("Defaults")]
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private SpawnMarkerStrategy defaultStrategy = SpawnMarkerStrategy.RoundRobin;

        [Tooltip("Set true to use defaultSeed value")]
        [SerializeField] private bool useDefaultSeed;
        [SerializeField] private int defaultSeed;
        [SerializeField] private float defaultGapSeconds = 5f;
        [SerializeField] private bool defaultWaitForClear = true;
        [SerializeField] private int defaultMaxAlive = 0;

        [Header("Authored Waves")]
        [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>();

        [Header("Optional Ramp")]
        [SerializeField] private RampConfig ramp;

        // Legacy sequences kept for backward compatibility; will be removed once waves/ramp are fully wired.
        [SerializeField, HideInInspector] private List<SpawnSequenceConfig> sequences = new List<SpawnSequenceConfig>();

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public float DefaultGapSeconds
        {
            get => defaultGapSeconds;
            set => defaultGapSeconds = Mathf.Max(0f, value);
        }

        public int DefaultMaxAlive
        {
            get => defaultMaxAlive;
            set => defaultMaxAlive = Mathf.Max(0, value);
        }

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
            return ToRuntimeWaveSchedule(null);
        }

        public WaveScheduleRuntime ToRuntimeWaveSchedule(GameBalanceStation balanceStationOverride)
        {
            var waveTable = ResolveWaveTable(balanceStationOverride);
            var strategy = waveTable != null ? waveTable.DefaultStrategy : defaultStrategy;
            var seed = waveTable != null ? waveTable.ResolvedDefaultSeed : useDefaultSeed ? defaultSeed : 0;
            var gapSeconds = waveTable != null ? waveTable.DefaultGapSeconds : defaultGapSeconds;
            var waitForClear = waveTable != null ? waveTable.DefaultWaitForClear : defaultWaitForClear;
            var maxAlive = waveTable != null ? waveTable.DefaultMaxAlive : defaultMaxAlive;

            return new WaveScheduleRuntime(
                defaultStrategy: strategy,
                defaultSeed: seed,
                waves: waves,
                ramp: ramp,
                defaultGapSeconds: gapSeconds,
                defaultWaitForClear: waitForClear,
                defaultMaxAlive: maxAlive,
                generationBuild: waveTable != null ? waveTable.ActiveBuild : null);
        }

        private WaveBalanceTable ActiveWaveTable => balanceStation != null ? balanceStation.Wave : null;

        private WaveBalanceTable ResolveWaveTable(GameBalanceStation balanceStationOverride)
        {
            if (balanceStationOverride != null && balanceStationOverride.Wave != null)
            {
                return balanceStationOverride.Wave;
            }

            return ActiveWaveTable;
        }

        private void OnValidate()
        {
            DefaultGapSeconds = defaultGapSeconds;
            DefaultMaxAlive = defaultMaxAlive;
        }
    }
}

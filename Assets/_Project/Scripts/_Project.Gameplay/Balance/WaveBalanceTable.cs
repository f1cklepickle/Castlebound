using UnityEngine;
using Castlebound.Gameplay.Spawning;

namespace Castlebound.Gameplay.Balance
{
    [System.Serializable]
    public class WaveGenerationBuild
    {
        [SerializeField] private string buildId = "default";
        [SerializeField] private bool enabled = true;
        [SerializeField] private int baseSpawnCount = 5;
        [SerializeField] private int spawnCountPerStep = 3;
        [SerializeField] private int spawnCountStepSize = 1;
        [SerializeField] private int spawnCountStartWave = 1;
        [Tooltip("0 means no cap")]
        [SerializeField] private int maxSpawnCount = 0;
        [SerializeField] private float intervalSeconds = 0.5f;
        [SerializeField] private float initialDelaySeconds = 0f;

        public string BuildId
        {
            get => buildId;
            set => buildId = value;
        }

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public int BaseSpawnCount
        {
            get => baseSpawnCount;
            set => baseSpawnCount = Mathf.Max(0, value);
        }

        public int SpawnCountPerStep
        {
            get => spawnCountPerStep;
            set => spawnCountPerStep = Mathf.Max(0, value);
        }

        public int SpawnCountStepSize
        {
            get => spawnCountStepSize;
            set => spawnCountStepSize = Mathf.Max(1, value);
        }

        public int SpawnCountStartWave
        {
            get => spawnCountStartWave;
            set => spawnCountStartWave = Mathf.Max(1, value);
        }

        public int MaxSpawnCount
        {
            get => maxSpawnCount;
            set => maxSpawnCount = Mathf.Max(0, value);
        }

        public float IntervalSeconds
        {
            get => intervalSeconds;
            set => intervalSeconds = Mathf.Max(0f, value);
        }

        public float InitialDelaySeconds
        {
            get => initialDelaySeconds;
            set => initialDelaySeconds = Mathf.Max(0f, value);
        }

        public int GetSpawnCountForWave(int waveIndex)
        {
            int safeWaveIndex = Mathf.Max(1, waveIndex);
            int count = baseSpawnCount;
            if (safeWaveIndex >= spawnCountStartWave && spawnCountPerStep > 0)
            {
                int steps = (safeWaveIndex - spawnCountStartWave) / spawnCountStepSize;
                if (steps > 0)
                {
                    count += steps * spawnCountPerStep;
                }
            }

            return maxSpawnCount > 0 && count > maxSpawnCount ? maxSpawnCount : count;
        }

        public void Normalize()
        {
            BaseSpawnCount = baseSpawnCount;
            SpawnCountPerStep = spawnCountPerStep;
            SpawnCountStepSize = spawnCountStepSize;
            SpawnCountStartWave = spawnCountStartWave;
            MaxSpawnCount = maxSpawnCount;
            IntervalSeconds = intervalSeconds;
            InitialDelaySeconds = initialDelaySeconds;
        }
    }

    [CreateAssetMenu(menuName = "Castlebound/Balance/Wave Balance Table")]
    public class WaveBalanceTable : ScriptableObject
    {
        [SerializeField] private SpawnMarkerStrategy defaultStrategy = SpawnMarkerStrategy.RoundRobin;
        [SerializeField] private bool useDefaultSeed = false;
        [SerializeField] private int defaultSeed = 0;
        [SerializeField] private float defaultGapSeconds = 5f;
        [SerializeField] private bool defaultWaitForClear = true;
        [SerializeField] private int defaultMaxAlive = 0;
        [Header("Generated Wave Builds")]
        [SerializeField] private int activeBuildIndex = 0;
        [SerializeField] private WaveGenerationBuild[] generatedBuilds =
        {
            new WaveGenerationBuild()
        };

        public SpawnMarkerStrategy DefaultStrategy
        {
            get => defaultStrategy;
            set => defaultStrategy = value;
        }

        public bool UseDefaultSeed
        {
            get => useDefaultSeed;
            set => useDefaultSeed = value;
        }

        public int DefaultSeed
        {
            get => defaultSeed;
            set => defaultSeed = value;
        }

        public float DefaultGapSeconds
        {
            get => defaultGapSeconds;
            set => defaultGapSeconds = Mathf.Max(0f, value);
        }

        public bool DefaultWaitForClear
        {
            get => defaultWaitForClear;
            set => defaultWaitForClear = value;
        }

        public int DefaultMaxAlive
        {
            get => defaultMaxAlive;
            set => defaultMaxAlive = Mathf.Max(0, value);
        }

        public int ActiveBuildIndex
        {
            get => activeBuildIndex;
            set => activeBuildIndex = Mathf.Max(0, value);
        }

        public WaveGenerationBuild[] GeneratedBuilds
        {
            get => generatedBuilds;
            set => generatedBuilds = value;
        }

        public int ResolvedDefaultSeed => useDefaultSeed ? defaultSeed : 0;

        public WaveGenerationBuild ActiveBuild
        {
            get
            {
                if (generatedBuilds == null || generatedBuilds.Length == 0)
                {
                    return null;
                }

                int index = Mathf.Clamp(activeBuildIndex, 0, generatedBuilds.Length - 1);
                var build = generatedBuilds[index];
                return build != null && build.Enabled ? build : null;
            }
        }

        private void OnValidate()
        {
            DefaultGapSeconds = defaultGapSeconds;
            DefaultMaxAlive = defaultMaxAlive;
            ActiveBuildIndex = activeBuildIndex;
            if (generatedBuilds == null)
            {
                return;
            }

            for (int i = 0; i < generatedBuilds.Length; i++)
            {
                generatedBuilds[i]?.Normalize();
            }
        }
    }
}

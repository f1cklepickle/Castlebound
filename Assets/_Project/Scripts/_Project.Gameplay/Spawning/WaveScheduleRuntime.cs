using System.Collections.Generic;
using System.Linq;
using Castlebound.Gameplay.Balance;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public class WaveRuntime
    {
        public IReadOnlyList<SpawnSequenceConfig> Sequences { get; }
        public SpawnMarkerStrategy Strategy { get; }
        public int Seed { get; }
        public float GapSeconds { get; }
        public bool WaitForClear { get; }
        public int MaxAlive { get; }

        public WaveRuntime(IEnumerable<SpawnSequenceConfig> sequences, SpawnMarkerStrategy strategy, int seed, float gapSeconds, bool waitForClear, int maxAlive)
        {
            Sequences = sequences?.ToList() ?? new List<SpawnSequenceConfig>();
            Strategy = strategy;
            Seed = seed;
            GapSeconds = gapSeconds;
            WaitForClear = waitForClear;
            MaxAlive = maxAlive;
        }
    }

    public class WaveScheduleRuntime
    {
        private readonly SpawnMarkerStrategy _defaultStrategy;
        private readonly int _defaultSeed;
        private readonly List<WaveConfig> _waves;
        private readonly RampConfig _ramp;
        private readonly float _defaultGapSeconds;
        private readonly bool _defaultWaitForClear;
        private readonly int _defaultMaxAlive;
        private readonly WaveGenerationBuild _generationBuild;

        public WaveScheduleRuntime(SpawnMarkerStrategy defaultStrategy, int defaultSeed, IEnumerable<WaveConfig> waves, RampConfig ramp)
            : this(defaultStrategy, defaultSeed, waves, ramp, 5f, true, 0)
        {
        }

        public WaveScheduleRuntime(
            SpawnMarkerStrategy defaultStrategy,
            int defaultSeed,
            IEnumerable<WaveConfig> waves,
            RampConfig ramp,
            float defaultGapSeconds,
            bool defaultWaitForClear,
            int defaultMaxAlive)
            : this(
                defaultStrategy,
                defaultSeed,
                waves,
                ramp,
                defaultGapSeconds,
                defaultWaitForClear,
                defaultMaxAlive,
                null)
        {
        }

        public WaveScheduleRuntime(
            SpawnMarkerStrategy defaultStrategy,
            int defaultSeed,
            IEnumerable<WaveConfig> waves,
            RampConfig ramp,
            float defaultGapSeconds,
            bool defaultWaitForClear,
            int defaultMaxAlive,
            WaveGenerationBuild generationBuild)
        {
            _defaultStrategy = defaultStrategy;
            _defaultSeed = defaultSeed;
            _waves = waves?.ToList() ?? new List<WaveConfig>();
            _ramp = ramp;
            _defaultGapSeconds = defaultGapSeconds < 0f ? 0f : defaultGapSeconds;
            _defaultWaitForClear = defaultWaitForClear;
            _defaultMaxAlive = defaultMaxAlive < 0 ? 0 : defaultMaxAlive;
            _generationBuild = generationBuild != null && generationBuild.Enabled ? generationBuild : null;
        }

        public bool HasAuthoredWaves => _waves.Count > 0;
        public bool CanProvideWaves => HasAuthoredWaves || HasRampGeneration;

        public WaveRuntime GetWave(int waveIndex)
        {
            var authored = GetAuthoredWaveConfig(waveIndex);
            if (authored != null)
            {
                var strategy = authored.useStrategyOverride ? authored.strategyOverride : _defaultStrategy;
                var seed = authored.useSeedOverride ? authored.seedOverride : _defaultSeed;
                var gap = authored.gapSeconds;
                var waitForClear = authored.waitForClear;
                var maxAlive = authored.maxAlive;

                return new WaveRuntime(authored.sequences, strategy, seed, gap, waitForClear, maxAlive);
            }

            // Generate via ramp if available.
            var generated = GenerateRampWave(waveIndex);
            if (generated != null)
            {
                return generated;
            }

            // No wave available; fall back to defaults with empty sequence to avoid nulls.
            return new WaveRuntime(
                sequences: new List<SpawnSequenceConfig>(),
                strategy: _defaultStrategy,
                seed: _defaultSeed,
                gapSeconds: _defaultGapSeconds,
                waitForClear: _defaultWaitForClear,
                maxAlive: _defaultMaxAlive);
        }

        private WaveConfig GetAuthoredWaveConfig(int waveIndex)
        {
            if (waveIndex <= 0)
            {
                return null;
            }

            var zeroBased = waveIndex - 1;
            if (zeroBased < 0 || zeroBased >= _waves.Count)
            {
                return null;
            }

            return _waves[zeroBased];
        }

        private WaveRuntime GenerateRampWave(int waveIndex)
        {
            if (_ramp == null || waveIndex < _ramp.startWave)
            {
                return null;
            }

            var count = ResolveGeneratedSpawnCount(waveIndex);

            var pool = BuildTierPool(waveIndex);
            if (pool.Count == 0)
            {
                return null;
            }

            var sequences = BuildRampSequences(count, pool, waveIndex);

            return new WaveRuntime(
                sequences: sequences,
                strategy: _defaultStrategy,
                seed: _defaultSeed,
                gapSeconds: _defaultGapSeconds,
                waitForClear: _defaultWaitForClear,
                maxAlive: _defaultMaxAlive);
        }

        private int ResolveGeneratedSpawnCount(int waveIndex)
        {
            if (_generationBuild != null)
            {
                return _generationBuild.GetSpawnCountForWave(waveIndex);
            }

            var rampCount = _ramp.baseSpawnCount;
            if (_ramp.stepSize > 0 && _ramp.countPerStep != 0)
            {
                var steps = (waveIndex - _ramp.startWave) / _ramp.stepSize;
                if (steps > 0)
                {
                    rampCount += steps * _ramp.countPerStep;
                }
            }

            if (rampCount < _ramp.baseSpawnCount)
            {
                rampCount = _ramp.baseSpawnCount;
            }

            return rampCount;
        }

        private List<RuntimeRampTier> BuildTierPool(int waveIndex)
        {
            var pool = new List<RuntimeRampTier>();
            if (_ramp == null || _ramp.unlocks == null)
            {
                return pool;
            }

            foreach (var unlock in _ramp.unlocks)
            {
                if (waveIndex >= unlock.waveIndex && unlock.tiers != null)
                {
                    foreach (var tier in unlock.tiers.Where(t => !string.IsNullOrWhiteSpace(t.enemyTypeId)))
                    {
                        pool.Add(new RuntimeRampTier(tier, unlock.waveIndex));
                    }
                }
            }

            // If no weights are set, weights are treated as equal during spawn-count splitting.
            return pool;
        }

        private bool HasRampGeneration
        {
            get
            {
                if (_ramp == null || _ramp.unlocks == null)
                {
                    return false;
                }

                for (int i = 0; i < _ramp.unlocks.Count; i++)
                {
                    var unlock = _ramp.unlocks[i];
                    if (unlock.tiers == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < unlock.tiers.Count; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(unlock.tiers[j].enemyTypeId))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private List<SpawnSequenceConfig> BuildRampSequences(int totalCount, List<RuntimeRampTier> pool, int waveIndex)
        {
            var sequences = new List<SpawnSequenceConfig>();
            if (totalCount <= 0 || pool.Count == 0)
            {
                return sequences;
            }

            var explicitCounts = new Dictionary<int, int>();
            int explicitTotal = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].Tier.baseSpawnCount <= 0)
                {
                    continue;
                }

                int count = GetExplicitTierCount(pool[i], waveIndex);
                explicitCounts[i] = count;
                explicitTotal += count;
            }

            int weightedTotal = Mathf.Max(0, totalCount - explicitTotal);
            var weightedIndexes = new List<int>();
            float totalWeight = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                if (explicitCounts.ContainsKey(i))
                {
                    continue;
                }

                weightedIndexes.Add(i);
                totalWeight += pool[i].Tier.weight > 0f ? pool[i].Tier.weight : 1f;
            }

            if (totalWeight <= 0f)
            {
                totalWeight = weightedIndexes.Count;
            }

            int guaranteedCount = weightedTotal >= weightedIndexes.Count ? 1 : 0;
            int distributableCount = weightedTotal - guaranteedCount * weightedIndexes.Count;
            var fractionalCounts = new List<(RuntimeRampTier Tier, int Count, float Remainder, int Order)>(weightedIndexes.Count);
            int assignedCount = guaranteedCount * weightedIndexes.Count;
            foreach (var index in weightedIndexes)
            {
                var tier = pool[index];
                float weight = tier.Tier.weight > 0f ? tier.Tier.weight : 1f;
                float exactCount = distributableCount * (weight / totalWeight);
                int count = Mathf.FloorToInt(exactCount);
                assignedCount += count;
                fractionalCounts.Add((tier, guaranteedCount + count, exactCount - count, index));
            }

            int remainder = weightedTotal - assignedCount;
            fractionalCounts.Sort((a, b) => b.Remainder.CompareTo(a.Remainder));
            for (int i = 0; i < fractionalCounts.Count && remainder > 0; i++, remainder--)
            {
                var current = fractionalCounts[i];
                fractionalCounts[i] = (current.Tier, current.Count + 1, current.Remainder, current.Order);
            }

            var sequenceCounts = new Dictionary<int, int>();
            foreach (var pair in explicitCounts)
            {
                sequenceCounts[pair.Key] = pair.Value;
            }

            foreach (var entry in fractionalCounts)
            {
                sequenceCounts[entry.Order] = entry.Count;
            }

            for (int i = 0; i < pool.Count; i++)
            {
                if (!sequenceCounts.TryGetValue(i, out var count) || count <= 0)
                {
                    continue;
                }

                sequences.Add(new SpawnSequenceConfig
                {
                    enemyTypeId = pool[i].Tier.enemyTypeId,
                    spawnCount = count,
                    intervalSeconds = _generationBuild != null ? _generationBuild.IntervalSeconds : 1f,
                    initialDelaySeconds = _generationBuild != null ? _generationBuild.InitialDelaySeconds : 0f
                });
            }

            return sequences;
        }

        private static int GetExplicitTierCount(RuntimeRampTier tier, int waveIndex)
        {
            int count = Mathf.Max(0, tier.Tier.baseSpawnCount);
            if (tier.Tier.stepSize <= 0 || tier.Tier.countPerStep == 0)
            {
                return count;
            }

            int steps = Mathf.Max(0, waveIndex - tier.UnlockWaveIndex) / tier.Tier.stepSize;
            return Mathf.Max(0, count + steps * tier.Tier.countPerStep);
        }

        private readonly struct RuntimeRampTier
        {
            public RuntimeRampTier(RampTier tier, int unlockWaveIndex)
            {
                Tier = tier;
                UnlockWaveIndex = unlockWaveIndex;
            }

            public RampTier Tier { get; }
            public int UnlockWaveIndex { get; }
        }
    }
}

using System.Collections.Generic;
using System.Linq;

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
        private readonly System.Random _rng;

        public WaveScheduleRuntime(SpawnMarkerStrategy defaultStrategy, int defaultSeed, IEnumerable<WaveConfig> waves, RampConfig ramp)
        {
            _defaultStrategy = defaultStrategy;
            _defaultSeed = defaultSeed;
            _waves = waves?.ToList() ?? new List<WaveConfig>();
            _ramp = ramp;
            _rng = new System.Random(_defaultSeed);
        }

        public bool HasAuthoredWaves => _waves.Count > 0;

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
                gapSeconds: 5f,
                waitForClear: true,
                maxAlive: 0);
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

            var count = _ramp.baseSpawnCount;
            if (_ramp.stepSize > 0 && _ramp.countPerStep != 0)
            {
                var steps = (waveIndex - _ramp.startWave) / _ramp.stepSize;
                if (steps > 0)
                {
                    count += steps * _ramp.countPerStep;
                }
            }

            if (count < _ramp.baseSpawnCount)
            {
                count = _ramp.baseSpawnCount;
            }

            var pool = BuildTierPool(waveIndex);
            if (pool.Count == 0)
            {
                return null;
            }

            var chosenType = ChooseEnemyType(pool);

            var sequence = new SpawnSequenceConfig
            {
                enemyTypeId = chosenType,
                spawnCount = count,
                intervalSeconds = 1f,
                initialDelaySeconds = 0f
            };

            return new WaveRuntime(
                sequences: new List<SpawnSequenceConfig> { sequence },
                strategy: _defaultStrategy,
                seed: _defaultSeed,
                gapSeconds: 5f,
                waitForClear: true,
                maxAlive: 0);
        }

        private List<RampTier> BuildTierPool(int waveIndex)
        {
            var pool = new List<RampTier>();
            if (_ramp == null || _ramp.unlocks == null)
            {
                return pool;
            }

            foreach (var unlock in _ramp.unlocks)
            {
                if (waveIndex >= unlock.waveIndex && unlock.tiers != null)
                {
                    pool.AddRange(unlock.tiers.Where(t => !string.IsNullOrWhiteSpace(t.enemyTypeId)));
                }
            }

            // If no weights set, weights are effectively equal; we just pick the first for now.
            return pool;
        }

        private string ChooseEnemyType(List<RampTier> pool)
        {
            if (pool.Count == 0)
            {
                return string.Empty;
            }

            float totalWeight = 0f;
            foreach (var tier in pool)
            {
                totalWeight += tier.weight > 0f ? tier.weight : 1f;
            }

            if (totalWeight <= 0f)
            {
                return pool[0].enemyTypeId;
            }

            var roll = (float)(_rng.NextDouble() * totalWeight);
            float cumulative = 0f;
            foreach (var tier in pool)
            {
                var w = tier.weight > 0f ? tier.weight : 1f;
                cumulative += w;
                if (roll <= cumulative)
                {
                    return tier.enemyTypeId;
                }
            }

            return pool[pool.Count - 1].enemyTypeId;
        }
    }
}

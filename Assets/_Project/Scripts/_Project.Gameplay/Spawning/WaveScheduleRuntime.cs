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

        public WaveScheduleRuntime(SpawnMarkerStrategy defaultStrategy, int defaultSeed, IEnumerable<WaveConfig> waves, RampConfig ramp)
        {
            _defaultStrategy = defaultStrategy;
            _defaultSeed = defaultSeed;
            _waves = waves?.ToList() ?? new List<WaveConfig>();
            _ramp = ramp;
        }

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

            // No authored wave; fall back to defaults with empty sequences for now (ramp to be wired later).
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
    }
}

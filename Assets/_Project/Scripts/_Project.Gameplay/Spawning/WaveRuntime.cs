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

        public WaveRuntime(
            IEnumerable<SpawnSequenceConfig> sequences,
            SpawnMarkerStrategy strategy,
            int seed,
            float gapSeconds,
            bool waitForClear,
            int maxAlive)
        {
            Sequences = sequences?.ToList() ?? new List<SpawnSequenceConfig>();
            Strategy = strategy;
            Seed = seed;
            GapSeconds = gapSeconds;
            WaitForClear = waitForClear;
            MaxAlive = maxAlive;
        }
    }
}

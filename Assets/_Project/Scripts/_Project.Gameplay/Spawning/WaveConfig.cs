using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
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
        public int maxAlive;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
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
}

using System.Collections.Generic;

namespace Castlebound.Gameplay.Spawning
{
    [System.Serializable]
    public struct RampTierUnlock
    {
        public int waveIndex;
        public List<RampTier> tiers;
    }
}

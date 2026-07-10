using Castlebound.Gameplay.Spawning;

namespace Castlebound.Gameplay.Castle
{
    public static class CastleShopAccessPolicy
    {
        public static bool CanOpen(bool hasCastleRegionTracker, bool playerInsideCastle, WavePhase phase)
        {
            return hasCastleRegionTracker && playerInsideCastle && phase == WavePhase.PreWave;
        }
    }
}

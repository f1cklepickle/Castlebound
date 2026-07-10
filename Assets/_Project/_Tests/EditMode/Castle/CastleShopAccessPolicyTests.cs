using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;

namespace Castlebound.Tests.Castle
{
    public class CastleShopAccessPolicyTests
    {
        [Test]
        public void CanOpen_AllowsOnlyInsideCastleDuringPreWave()
        {
            Assert.IsTrue(CastleShopAccessPolicy.CanOpen(true, true, WavePhase.PreWave));
            Assert.IsFalse(CastleShopAccessPolicy.CanOpen(true, false, WavePhase.PreWave));
            Assert.IsFalse(CastleShopAccessPolicy.CanOpen(true, true, WavePhase.InWave));
        }

        [Test]
        public void CanOpen_BlocksWhenCastleRegionTrackerIsUnavailable()
        {
            Assert.IsFalse(CastleShopAccessPolicy.CanOpen(false, true, WavePhase.PreWave));
        }
    }
}

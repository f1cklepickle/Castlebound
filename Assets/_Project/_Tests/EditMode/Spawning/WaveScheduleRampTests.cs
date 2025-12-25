using System.Collections.Generic;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;

namespace Castlebound.Tests.Spawning
{
    public class WaveScheduleRampTests
    {
        [Test]
        public void Ramp_GeneratesCountsAndUnlocksTypes()
        {
            var ramp = new RampConfig
            {
                baseSpawnCount = 5,
                countPerStep = 2,
                stepSize = 2,
                startWave = 1,
                unlocks = new List<RampTierUnlock>
                {
                    new RampTierUnlock
                    {
                        waveIndex = 1,
                        tiers = new List<RampTier> { new RampTier { enemyTypeId = "grunt", weight = 1f } }
                    },
                    new RampTierUnlock
                    {
                        waveIndex = 3,
                        tiers = new List<RampTier> { new RampTier { enemyTypeId = "archer", weight = 1f } }
                    }
                }
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp);

            var wave1 = schedule.GetWave(1);
            Assert.IsNotNull(wave1);
            Assert.AreEqual(5, wave1.Sequences[0].spawnCount, "Wave1 count should start at base.");
            Assert.AreEqual("grunt", wave1.Sequences[0].enemyTypeId, "Wave1 should use first unlocked tier.");

            var wave2 = schedule.GetWave(2);
            Assert.IsNotNull(wave2);
            Assert.AreEqual(5, wave2.Sequences[0].spawnCount, "No step until wave3 (stepSize=2 starting at wave1).");
            Assert.AreEqual("grunt", wave2.Sequences[0].enemyTypeId, "Wave2 still only grunt.");

            var wave3 = schedule.GetWave(3);
            Assert.IsNotNull(wave3);
            Assert.AreEqual(7, wave3.Sequences[0].spawnCount, "Wave3 adds countPerStep (5 + 2).");
            Assert.AreEqual("grunt", wave3.Sequences[0].enemyTypeId, "Type selection currently takes first in pool; pool now includes archer too.");
        }
    }
}

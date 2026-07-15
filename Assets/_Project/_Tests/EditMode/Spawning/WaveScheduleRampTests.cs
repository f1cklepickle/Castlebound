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
            Assert.AreEqual(2, wave3.Sequences.Count, "Wave3 should support multiple enemy types in one generated schedule.");
            Assert.AreEqual("grunt", wave3.Sequences[0].enemyTypeId);
            Assert.AreEqual("archer", wave3.Sequences[1].enemyTypeId);
            Assert.AreEqual(4, wave3.Sequences[0].spawnCount);
            Assert.AreEqual(3, wave3.Sequences[1].spawnCount);
        }

        [Test]
        public void Ramp_UsesWeightsWhenSplittingTypes()
        {
            var ramp = new RampConfig
            {
                baseSpawnCount = 5,
                countPerStep = 0,
                stepSize = 1,
                startWave = 1,
                unlocks = new List<RampTierUnlock>
                {
                    new RampTierUnlock
                    {
                        waveIndex = 1,
                        tiers = new List<RampTier>
                        {
                            new RampTier { enemyTypeId = "grunt", weight = 0.1f },
                            new RampTier { enemyTypeId = "archer", weight = 2f }
                        }
                    }
                }
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 123,
                waves: null,
                ramp: ramp);

            var wave = schedule.GetWave(1);

            Assert.AreEqual(2, wave.Sequences.Count);
            Assert.AreEqual("grunt", wave.Sequences[0].enemyTypeId);
            Assert.AreEqual("archer", wave.Sequences[1].enemyTypeId);
            Assert.AreEqual(1, wave.Sequences[0].spawnCount);
            Assert.AreEqual(4, wave.Sequences[1].spawnCount, "Weights should bias split counts toward archer.");
        }

        [Test]
        public void Ramp_UsesExplicitTierCounts_WhenConfigured()
        {
            var ramp = new RampConfig
            {
                baseSpawnCount = 6,
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
                        tiers = new List<RampTier>
                        {
                            new RampTier
                            {
                                enemyTypeId = "lurker",
                                weight = 0.35f,
                                baseSpawnCount = 1,
                                countPerStep = 1,
                                stepSize = 2
                            }
                        }
                    }
                }
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp);

            var wave3 = schedule.GetWave(3);
            Assert.AreEqual("grunt", wave3.Sequences[0].enemyTypeId);
            Assert.AreEqual("lurker", wave3.Sequences[1].enemyTypeId);
            Assert.AreEqual(7, wave3.Sequences[0].spawnCount);
            Assert.AreEqual(1, wave3.Sequences[1].spawnCount);

            var wave5 = schedule.GetWave(5);
            Assert.AreEqual(8, wave5.Sequences[0].spawnCount);
            Assert.AreEqual(2, wave5.Sequences[1].spawnCount);
        }
    }
}

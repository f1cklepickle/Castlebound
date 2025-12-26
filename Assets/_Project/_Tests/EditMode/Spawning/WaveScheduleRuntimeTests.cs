using System.Collections.Generic;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;

namespace Castlebound.Tests.Spawning
{
    public class WaveScheduleRuntimeTests
    {
        [Test]
        public void UsesOverridesWhenProvidedElseFallsBackToDefaults()
        {
            var authoredWaves = new List<WaveConfig>
            {
                new WaveConfig
                {
                    sequences = new List<SpawnSequenceConfig>
                    {
                        new SpawnSequenceConfig
                        {
                            enemyTypeId = "grunt",
                            spawnCount = 3,
                            intervalSeconds = 1f,
                            initialDelaySeconds = 0f
                        }
                    },
                    useStrategyOverride = true,
                    strategyOverride = SpawnMarkerStrategy.ShufflePrecompute,
                    useSeedOverride = true,
                    seedOverride = 42,
                    gapSeconds = 2f,
                    waitForClear = true,
                    maxAlive = 5
                }
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 123,
                waves: authoredWaves,
                ramp: null);

            var wave1 = schedule.GetWave(1);

            Assert.AreEqual(SpawnMarkerStrategy.ShufflePrecompute, wave1.Strategy, "Wave should use strategy override when provided.");
            Assert.AreEqual(42, wave1.Seed, "Wave should use seed override when provided.");
            Assert.AreEqual(2f, wave1.GapSeconds, "Wave should respect per-wave gap.");
            Assert.IsTrue(wave1.WaitForClear, "Wave should respect wait-for-clear flag.");
            Assert.AreEqual(5, wave1.MaxAlive, "Wave should respect per-wave maxAlive.");

            var wave2 = schedule.GetWave(2);

            Assert.AreEqual(SpawnMarkerStrategy.RoundRobin, wave2.Strategy, "Wave should fall back to default strategy when no wave override exists.");
            Assert.AreEqual(123, wave2.Seed, "Wave should fall back to default seed when no wave override exists.");
            Assert.IsTrue(wave2.WaitForClear, "Wait-for-clear should default to true.");
            Assert.AreEqual(5f, wave2.GapSeconds, "Gap should default to 5 seconds.");
            Assert.AreEqual(0, wave2.MaxAlive, "MaxAlive should default to unset (0).");
        }
    }
}

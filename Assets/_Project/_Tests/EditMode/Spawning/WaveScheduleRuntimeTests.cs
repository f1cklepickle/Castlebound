using System.Collections.Generic;
using Castlebound.Gameplay.Balance;
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

        [Test]
        public void FallbackWave_UsesProvidedSharedDefaults()
        {
            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.ShufflePrecompute,
                defaultSeed: 42,
                waves: null,
                ramp: null,
                defaultGapSeconds: 2f,
                defaultWaitForClear: false,
                defaultMaxAlive: 6);

            var wave = schedule.GetWave(1);

            Assert.AreEqual(SpawnMarkerStrategy.ShufflePrecompute, wave.Strategy);
            Assert.AreEqual(42, wave.Seed);
            Assert.AreEqual(2f, wave.GapSeconds);
            Assert.IsFalse(wave.WaitForClear);
            Assert.AreEqual(6, wave.MaxAlive);
        }

        [Test]
        public void CanProvideWaves_IsTrueForAuthoredOrGeneratedSchedules()
        {
            var authoredSchedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: new[]
                {
                    new WaveConfig
                    {
                        sequences = new List<SpawnSequenceConfig>
                        {
                            new SpawnSequenceConfig { enemyTypeId = "grunt", spawnCount = 1 }
                        }
                    }
                },
                ramp: null);

            var generatedSchedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: CreateSingleTierRamp(baseSpawnCount: 5, countPerStep: 1, stepSize: 1, startWave: 1));

            var emptySchedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: null);

            Assert.IsTrue(authoredSchedule.CanProvideWaves);
            Assert.IsTrue(generatedSchedule.CanProvideWaves);
            Assert.IsFalse(emptySchedule.CanProvideWaves);
        }

        [Test]
        public void GeneratedRampWave_UsesProvidedSharedPacingDefaults()
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
                        tiers = new List<RampTier> { new RampTier { enemyTypeId = "grunt", weight = 1f } }
                    }
                }
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp,
                defaultGapSeconds: 3f,
                defaultWaitForClear: false,
                defaultMaxAlive: 4);

            var wave = schedule.GetWave(1);

            Assert.AreEqual(3f, wave.GapSeconds);
            Assert.IsFalse(wave.WaitForClear);
            Assert.AreEqual(4, wave.MaxAlive);
        }

        [Test]
        public void GeneratedRampWave_UsesActiveBuildSpawnCountScaling()
        {
            var ramp = CreateSingleTierRamp(baseSpawnCount: 99, countPerStep: 99, stepSize: 1, startWave: 1);
            var build = new WaveGenerationBuild
            {
                BaseSpawnCount = 4,
                SpawnCountPerStep = 3,
                SpawnCountStepSize = 2,
                SpawnCountStartWave = 2
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp,
                defaultGapSeconds: 5f,
                defaultWaitForClear: true,
                defaultMaxAlive: 0,
                generationBuild: build);

            var wave1 = schedule.GetWave(1);
            var wave2 = schedule.GetWave(2);
            var wave4 = schedule.GetWave(4);

            Assert.AreEqual(4, wave1.Sequences[0].spawnCount);
            Assert.AreEqual(4, wave2.Sequences[0].spawnCount);
            Assert.AreEqual(7, wave4.Sequences[0].spawnCount);
        }

        [Test]
        public void GeneratedRampWave_ActiveBuildHonorsMaxCap()
        {
            var ramp = CreateSingleTierRamp(baseSpawnCount: 5, countPerStep: 1, stepSize: 1, startWave: 1);
            var build = new WaveGenerationBuild
            {
                BaseSpawnCount = 4,
                SpawnCountPerStep = 10,
                SpawnCountStepSize = 1,
                SpawnCountStartWave = 1,
                MaxSpawnCount = 12
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp,
                defaultGapSeconds: 5f,
                defaultWaitForClear: true,
                defaultMaxAlive: 0,
                generationBuild: build);

            var wave3 = schedule.GetWave(3);

            Assert.AreEqual(12, wave3.Sequences[0].spawnCount);
        }

        [Test]
        public void GeneratedRampWave_PreservesRampSpawnCountsWithoutActiveBuild()
        {
            var ramp = CreateSingleTierRamp(baseSpawnCount: 5, countPerStep: 2, stepSize: 2, startWave: 1);
            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp,
                defaultGapSeconds: 5f,
                defaultWaitForClear: true,
                defaultMaxAlive: 0,
                generationBuild: null);

            var wave3 = schedule.GetWave(3);

            Assert.AreEqual(7, wave3.Sequences[0].spawnCount);
        }

        [Test]
        public void GeneratedRampWave_ActiveBuildCanJumpFromFiveToFifteenOnWaveTwo()
        {
            var ramp = CreateSingleTierRamp(baseSpawnCount: 99, countPerStep: 99, stepSize: 1, startWave: 1);
            var build = new WaveGenerationBuild
            {
                BaseSpawnCount = 5,
                SpawnCountPerStep = 10,
                SpawnCountStepSize = 1,
                SpawnCountStartWave = 1
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: null,
                ramp: ramp,
                defaultGapSeconds: 5f,
                defaultWaitForClear: true,
                defaultMaxAlive: 0,
                generationBuild: build);

            Assert.AreEqual(5, schedule.GetWave(1).Sequences[0].spawnCount);
            Assert.AreEqual(15, schedule.GetWave(2).Sequences[0].spawnCount);
        }

        private static RampConfig CreateSingleTierRamp(int baseSpawnCount, int countPerStep, int stepSize, int startWave)
        {
            return new RampConfig
            {
                baseSpawnCount = baseSpawnCount,
                countPerStep = countPerStep,
                stepSize = stepSize,
                startWave = startWave,
                unlocks = new List<RampTierUnlock>
                {
                    new RampTierUnlock
                    {
                        waveIndex = 1,
                        tiers = new List<RampTier> { new RampTier { enemyTypeId = "grunt", weight = 1f } }
                    }
                }
            };
        }
    }
}

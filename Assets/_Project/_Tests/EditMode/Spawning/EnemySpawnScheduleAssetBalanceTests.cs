using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.IO;

namespace Castlebound.Tests.Spawning
{
    public class EnemySpawnScheduleAssetBalanceTests
    {
        [Test]
        public void ToRuntimeWaveSchedule_UsesWaveBalanceTableDefaultsWhenAssigned()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<WaveBalanceTable>();
            var scheduleAsset = ScriptableObject.CreateInstance<EnemySpawnScheduleAsset>();

            try
            {
                table.DefaultStrategy = SpawnMarkerStrategy.ShufflePrecompute;
                table.UseDefaultSeed = true;
                table.DefaultSeed = 99;
                table.DefaultGapSeconds = 2f;
                table.DefaultWaitForClear = false;
                table.DefaultMaxAlive = 7;
                station.Wave = table;
                scheduleAsset.BalanceStation = station;

                var wave = scheduleAsset.ToRuntimeWaveSchedule().GetWave(1);

                Assert.That(wave.Strategy, Is.EqualTo(SpawnMarkerStrategy.ShufflePrecompute));
                Assert.That(wave.Seed, Is.EqualTo(99));
                Assert.That(wave.GapSeconds, Is.EqualTo(2f).Within(0.001f));
                Assert.IsFalse(wave.WaitForClear);
                Assert.That(wave.MaxAlive, Is.EqualTo(7));
            }
            finally
            {
                Object.DestroyImmediate(scheduleAsset);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void ToRuntimeWaveSchedule_UsesActiveWaveBuildForGeneratedCounts()
        {
            var station = ScriptableObject.CreateInstance<GameBalanceStation>();
            var table = ScriptableObject.CreateInstance<WaveBalanceTable>();
            var scheduleAsset = ScriptableObject.CreateInstance<EnemySpawnScheduleAsset>();

            try
            {
                table.GeneratedBuilds = new[]
                {
                    new WaveGenerationBuild
                    {
                        BuildId = "default",
                        BaseSpawnCount = 5,
                        SpawnCountPerStep = 10,
                        SpawnCountStepSize = 1,
                        SpawnCountStartWave = 1
                    }
                };
                station.Wave = table;
                scheduleAsset.BalanceStation = station;
                SetRamp(scheduleAsset);

                var runtime = scheduleAsset.ToRuntimeWaveSchedule();

                Assert.That(runtime.GetWave(1).Sequences[0].spawnCount, Is.EqualTo(5));
                Assert.That(runtime.GetWave(2).Sequences[0].spawnCount, Is.EqualTo(15));
            }
            finally
            {
                Object.DestroyImmediate(scheduleAsset);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(station);
            }
        }

        [Test]
        public void ToRuntimeWaveSchedule_UsesOverrideStationBeforeScheduleStation()
        {
            var scheduleStation = ScriptableObject.CreateInstance<GameBalanceStation>();
            var overrideStation = ScriptableObject.CreateInstance<GameBalanceStation>();
            var scheduleTable = ScriptableObject.CreateInstance<WaveBalanceTable>();
            var overrideTable = ScriptableObject.CreateInstance<WaveBalanceTable>();
            var scheduleAsset = ScriptableObject.CreateInstance<EnemySpawnScheduleAsset>();

            try
            {
                scheduleTable.DefaultMaxAlive = 3;
                overrideTable.DefaultMaxAlive = 9;
                scheduleStation.Wave = scheduleTable;
                overrideStation.Wave = overrideTable;
                scheduleAsset.BalanceStation = scheduleStation;

                var wave = scheduleAsset.ToRuntimeWaveSchedule(overrideStation).GetWave(1);

                Assert.That(wave.MaxAlive, Is.EqualTo(9));
            }
            finally
            {
                Object.DestroyImmediate(scheduleAsset);
                Object.DestroyImmediate(overrideTable);
                Object.DestroyImmediate(scheduleTable);
                Object.DestroyImmediate(overrideStation);
                Object.DestroyImmediate(scheduleStation);
            }
        }

        [Test]
        public void MainPrototypeSpawnerRunner_ReferencesCentralBalanceStation()
        {
            string sceneYaml = File.ReadAllText("Assets/_Project/Scenes/MainPrototype.unity");

            StringAssert.Contains(
                "balanceStation: {fileID: 11400000, guid: d70caaeb4e66400f8127d62f459cae21, type: 2}",
                sceneYaml,
                "MainPrototype EnemySpawnerRunner should pass the central station into wave generation.");
        }

        [Test]
        public void MainPrototypeSpawnerRunner_MapsGruntAndLurkerPrefabs()
        {
            string sceneYaml = File.ReadAllText("Assets/_Project/Scenes/MainPrototype.unity");

            StringAssert.Contains("enemyTypeId: grunt", sceneYaml);
            StringAssert.Contains("enemyTypeId: lurker", sceneYaml);
            StringAssert.Contains(
                "prefab: {fileID: 6394411849047796180, guid: da2e30e6f4e649dd90cab820b6231b8e, type: 3}",
                sceneYaml,
                "MainPrototype EnemySpawnerRunner should map lurker spawns to the Lurker prefab.");
        }

        [Test]
        public void BasicSpawnSchedule_UnlocksLurkerInGeneratedRamp()
        {
            var scheduleAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemySpawnScheduleAsset>(
                "Assets/_Project/Spawning/BasicSpawnSchedule.asset");

            Assert.NotNull(scheduleAsset, "BasicSpawnSchedule asset must exist.");

            var wave = scheduleAsset.ToRuntimeWaveSchedule().GetWave(3);

            Assert.That(wave.Sequences.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(wave.Sequences[0].enemyTypeId, Is.EqualTo("grunt"));
            Assert.That(wave.Sequences[1].enemyTypeId, Is.EqualTo("lurker"));
            Assert.That(wave.Sequences[1].spawnCount, Is.EqualTo(1));

            var laterWave = scheduleAsset.ToRuntimeWaveSchedule().GetWave(5);
            Assert.That(laterWave.Sequences[1].enemyTypeId, Is.EqualTo("lurker"));
            Assert.That(laterWave.Sequences[1].spawnCount, Is.EqualTo(2));
        }

        [Test]
        public void ToRuntimeWaveSchedule_PreservesFallbackDefaultsWithoutBalanceTable()
        {
            var scheduleAsset = ScriptableObject.CreateInstance<EnemySpawnScheduleAsset>();

            try
            {
                var wave = scheduleAsset.ToRuntimeWaveSchedule().GetWave(1);

                Assert.That(wave.Strategy, Is.EqualTo(SpawnMarkerStrategy.RoundRobin));
                Assert.That(wave.Seed, Is.EqualTo(0));
                Assert.That(wave.GapSeconds, Is.EqualTo(5f).Within(0.001f));
                Assert.IsTrue(wave.WaitForClear);
                Assert.That(wave.MaxAlive, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(scheduleAsset);
            }
        }

        private static void SetRamp(EnemySpawnScheduleAsset scheduleAsset)
        {
            var rampField = typeof(EnemySpawnScheduleAsset).GetField(
                "ramp",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rampField.SetValue(scheduleAsset, new RampConfig
            {
                baseSpawnCount = 99,
                countPerStep = 99,
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
            });
        }
    }
}

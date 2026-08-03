using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Spawning
{
    public class GoblinEquipmentDistributionTests
    {
        private const string SchedulePath = "Assets/_Project/Spawning/BasicSpawnSchedule.asset";
        private const string MeleeLoadoutPath =
            "Assets/_Project/Spawning/Loadouts/GoblinMeleeEquipmentLoadout.asset";
        private const string RangedLoadoutPath =
            "Assets/_Project/Spawning/Loadouts/GoblinRangedEquipmentLoadout.asset";

        [Test]
        public void BasicSchedule_AuthorsExpandableGoblinLoadoutsAndWaveChanceRamp()
        {
            var scheduleAsset = AssetDatabase.LoadAssetAtPath<EnemySpawnScheduleAsset>(SchedulePath);
            var meleeLoadout = AssetDatabase.LoadAssetAtPath<EnemyEquipmentLoadoutTable>(MeleeLoadoutPath);
            var rangedLoadout = AssetDatabase.LoadAssetAtPath<EnemyEquipmentLoadoutTable>(RangedLoadoutPath);
            var club = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Club.asset");
            var rock = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Rock.asset");

            Assert.NotNull(scheduleAsset);
            Assert.NotNull(meleeLoadout);
            Assert.NotNull(rangedLoadout);
            Assert.That(meleeLoadout.GetSelectionChance(club, 1), Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(meleeLoadout.GetSelectionChance(club, 5), Is.EqualTo(5f / 9f).Within(0.0001f));
            Assert.That(meleeLoadout.GetSelectionChance(club, 10), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(rangedLoadout.GetSelectionChance(rock, 1), Is.EqualTo(1f));

            var schedule = scheduleAsset.ToRuntimeWaveSchedule();
            Assert.That(FindSequence(schedule.GetWave(1), EnemyArchetypeIds.GoblinMelee).equipmentLoadout,
                Is.SameAs(meleeLoadout));
            Assert.That(FindSequence(schedule.GetWave(2), EnemyArchetypeIds.GoblinRanged).equipmentLoadout,
                Is.SameAs(rangedLoadout));
            Assert.That(FindSequence(schedule.GetWave(3), EnemyArchetypeIds.GoblinMelee).equipmentLoadout,
                Is.SameAs(meleeLoadout));
            Assert.That(FindSequence(schedule.GetWave(3), EnemyArchetypeIds.GoblinRanged).equipmentLoadout,
                Is.SameAs(rangedLoadout));
        }

        [Test]
        public void AuthoredSequence_EmitsSelectedEquipmentWithoutChangingArchetypeCount()
        {
            var unarmed = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var club = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var table = ScriptableObject.CreateInstance<EnemyEquipmentLoadoutTable>();

            try
            {
                table.Entries = new[]
                {
                    new EnemyEquipmentLoadoutEntry(unarmed, 1, 0f, 1, 0f),
                    new EnemyEquipmentLoadoutEntry(club, 1, 100f, 1, 100f)
                };
                var wave = new WaveConfig
                {
                    sequences = new List<SpawnSequenceConfig>
                    {
                        new SpawnSequenceConfig
                        {
                            enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                            spawnCount = 5,
                            equipmentLoadout = table
                        }
                    },
                    waitForClear = false
                };

                var requests = CreateSpawner(wave).Tick(0.1f, currentAlive: 0);

                Assert.That(requests, Has.Count.EqualTo(5));
                for (int i = 0; i < requests.Count; i++)
                {
                    Assert.That(requests[i].EnemyTypeId, Is.EqualTo(EnemyArchetypeIds.GoblinMelee));
                    Assert.That(requests[i].Equipment, Is.SameAs(club));
                }
            }
            finally
            {
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(club);
                Object.DestroyImmediate(unarmed);
            }
        }

        [Test]
        public void AuthoredSequence_SameSeed_ReplaysSelectedEquipmentOrder()
        {
            var unarmed = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var club = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var table = ScriptableObject.CreateInstance<EnemyEquipmentLoadoutTable>();

            try
            {
                table.Entries = new[]
                {
                    new EnemyEquipmentLoadoutEntry(unarmed, 1, 50f, 1, 50f),
                    new EnemyEquipmentLoadoutEntry(club, 1, 50f, 1, 50f)
                };
                var wave = new WaveConfig
                {
                    useSeedOverride = true,
                    seedOverride = 236,
                    sequences = new List<SpawnSequenceConfig>
                    {
                        new SpawnSequenceConfig
                        {
                            enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                            spawnCount = 12,
                            equipmentLoadout = table
                        }
                    },
                    waitForClear = false
                };

                var firstRun = CreateSpawner(wave).Tick(0.1f, currentAlive: 0);
                var replay = CreateSpawner(wave).Tick(0.1f, currentAlive: 0);

                Assert.That(replay, Has.Count.EqualTo(firstRun.Count));
                for (int i = 0; i < firstRun.Count; i++)
                {
                    Assert.That(replay[i].Equipment, Is.SameAs(firstRun[i].Equipment));
                }
            }
            finally
            {
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(club);
                Object.DestroyImmediate(unarmed);
            }
        }

        [Test]
        public void GeneratedTier_PreservesAuthoredEquipmentLoadoutReference()
        {
            var table = ScriptableObject.CreateInstance<EnemyEquipmentLoadoutTable>();
            try
            {
                var ramp = new RampConfig
                {
                    startWave = 1,
                    baseSpawnCount = 2,
                    unlocks = new List<RampTierUnlock>
                    {
                        new RampTierUnlock
                        {
                            waveIndex = 1,
                            tiers = new List<RampTier>
                            {
                                new RampTier
                                {
                                    enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                                    weight = 1f,
                                    equipmentLoadout = table
                                }
                            }
                        }
                    }
                };
                var schedule = new WaveScheduleRuntime(
                    SpawnMarkerStrategy.RoundRobin,
                    defaultSeed: 0,
                    waves: null,
                    ramp: ramp);

                var generated = schedule.GetWave(1);

                Assert.That(generated.Sequences, Has.Count.EqualTo(1));
                Assert.That(generated.Sequences[0].equipmentLoadout, Is.SameAs(table));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        private static EnemyWaveSpawner CreateSpawner(WaveConfig wave)
        {
            var schedule = new WaveScheduleRuntime(
                SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: new[] { wave },
                ramp: null);

            return new EnemyWaveSpawner(
                schedule,
                new[] { new SpawnPoint("GateA", Vector2.zero) });
        }

        private static SpawnSequenceConfig FindSequence(WaveRuntime wave, string enemyTypeId)
        {
            for (int i = 0; i < wave.Sequences.Count; i++)
            {
                if (wave.Sequences[i].enemyTypeId == enemyTypeId)
                {
                    return wave.Sequences[i];
                }
            }

            Assert.Fail($"Expected sequence for '{enemyTypeId}'.");
            return default;
        }
    }
}

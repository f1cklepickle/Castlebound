using System.Collections.Generic;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Spawning
{
    public class GoblinWaveCompositionTests
    {
        [Test]
        public void LegacyGruntId_EmitsCanonicalMeleeGoblinRequest()
        {
            var wave = CreateWave(
                new SpawnSequenceConfig
                {
                    enemyTypeId = EnemyArchetypeIds.LegacyGrunt,
                    spawnCount = 1
                });
            var spawner = CreateSpawner(wave);

            var ready = spawner.Tick(0.1f, currentAlive: 0);

            Assert.That(ready, Has.Count.EqualTo(1));
            Assert.That(ready[0].EnemyTypeId, Is.EqualTo(EnemyArchetypeIds.GoblinMelee));
        }

        [Test]
        public void MixedWave_ReplaysExactCountsInDeterministicArchetypeOrder()
        {
            var wave = CreateWave(
                new SpawnSequenceConfig
                {
                    enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                    spawnCount = 2
                },
                new SpawnSequenceConfig
                {
                    enemyTypeId = EnemyArchetypeIds.GoblinRanged,
                    spawnCount = 1
                });

            var firstRun = CreateSpawner(wave).Tick(0.1f, currentAlive: 0);
            var replay = CreateSpawner(wave).Tick(0.1f, currentAlive: 0);

            CollectionAssert.AreEqual(
                new[]
                {
                    EnemyArchetypeIds.GoblinMelee,
                    EnemyArchetypeIds.GoblinMelee,
                    EnemyArchetypeIds.GoblinRanged
                },
                GetEnemyTypeIds(firstRun));
            CollectionAssert.AreEqual(GetEnemyTypeIds(firstRun), GetEnemyTypeIds(replay));
        }

        [Test]
        public void ZeroCountMeleeSequence_PreservesRangedCountAndTiming()
        {
            var wave = CreateWave(
                new SpawnSequenceConfig
                {
                    enemyTypeId = EnemyArchetypeIds.GoblinMelee,
                    spawnCount = 0,
                    intervalSeconds = 0f,
                    initialDelaySeconds = 0f
                },
                new SpawnSequenceConfig
                {
                    enemyTypeId = EnemyArchetypeIds.GoblinRanged,
                    spawnCount = 2,
                    intervalSeconds = 0.25f,
                    initialDelaySeconds = 0.5f
                });
            var spawner = CreateSpawner(wave);

            Assert.That(spawner.Tick(0.49f, currentAlive: 0), Is.Empty);

            var first = spawner.Tick(0.02f, currentAlive: 0);
            Assert.That(first, Has.Count.EqualTo(1));
            Assert.That(first[0].EnemyTypeId, Is.EqualTo(EnemyArchetypeIds.GoblinRanged));

            var second = spawner.Tick(0.25f, currentAlive: 0);
            Assert.That(second, Has.Count.EqualTo(1));
            Assert.That(second[0].EnemyTypeId, Is.EqualTo(EnemyArchetypeIds.GoblinRanged));
        }

        private static WaveConfig CreateWave(params SpawnSequenceConfig[] sequences)
        {
            return new WaveConfig
            {
                sequences = new List<SpawnSequenceConfig>(sequences),
                waitForClear = false
            };
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

        private static string[] GetEnemyTypeIds(IReadOnlyList<SpawnRequest> requests)
        {
            var ids = new string[requests.Count];
            for (int i = 0; i < requests.Count; i++)
            {
                ids[i] = requests[i].EnemyTypeId;
            }

            return ids;
        }
    }
}

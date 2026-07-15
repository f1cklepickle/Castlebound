using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Spawning;

namespace Castlebound.Tests.Spawning
{
    public class EnemySpawnerTests
    {
        [Test]
        public void SelectsEachGateBeforeRepeating()
        {
            var spawnPoints = new[]
            {
                new SpawnPoint("NorthGate", new Vector2(-6f, 2f)),
                new SpawnPoint("EastGate", new Vector2(4f, 1f)),
                new SpawnPoint("SouthGate", new Vector2(0f, -3f)),
            };

            var schedule = new EnemySpawnSchedule(new[]
            {
                new SpawnSequence("grunt", spawnCount: 5, intervalSeconds: 1f, initialDelaySeconds: 0f),
            });

            var spawner = new EnemySpawner(schedule, spawnPoints);

            var gateOrder = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                var ready = spawner.Tick(1f);
                Assert.AreEqual(1, ready.Count, "Each tick should emit exactly one spawn for this config.");
                gateOrder.Add(ready[0].GateId);
            }

            CollectionAssert.AreEqual(
                new[] { "NorthGate", "EastGate", "SouthGate", "NorthGate", "EastGate" },
                gateOrder,
                "Gate selection should round-robin through available spawn points, ensuring coverage before repeating.");
        }

        [Test]
        public void RespectsInitialDelayAndIntervals()
        {
            var spawnPoints = new[]
            {
                new SpawnPoint("NorthGate", new Vector2(-6f, 2f)),
                new SpawnPoint("EastGate", new Vector2(4f, 1f)),
            };

            var schedule = new EnemySpawnSchedule(new[]
            {
                new SpawnSequence("grunt", spawnCount: 3, intervalSeconds: 0.4f, initialDelaySeconds: 0.5f),
            });

            var spawner = new EnemySpawner(schedule, spawnPoints);

            // No spawn before initial delay.
            var first = spawner.Tick(0.3f);
            Assert.AreEqual(0, first.Count, "Should not spawn before the initial delay elapses.");

            // Initial delay consumed here -> first spawn.
            var second = spawner.Tick(0.2f);
            Assert.AreEqual(1, second.Count, "First spawn should fire when initial delay is reached.");

            // Next intervals at 0.4s each.
            var third = spawner.Tick(0.4f);
            Assert.AreEqual(1, third.Count, "Second spawn should fire after interval.");

            var fourth = spawner.Tick(0.4f);
            Assert.AreEqual(1, fourth.Count, "Third spawn should fire after interval.");

            var emittedGateIds = new List<string>
            {
                second[0].GateId,
                third[0].GateId,
                fourth[0].GateId
            };

            CollectionAssert.AreEqual(
                new[] { "NorthGate", "EastGate", "NorthGate" },
                emittedGateIds,
                "Gate selection continues round-robin as spawns continue.");
        }

        [Test]
        public void UpdatesWaveIndexProvider_WhenWaveStarts()
        {
            var runnerGo = new GameObject("EnemySpawnerRunner");
            var runner = runnerGo.AddComponent<EnemySpawnerRunner>();
            var waveProvider = runnerGo.AddComponent<WaveIndexProviderComponent>();
            waveProvider.CurrentWaveIndex = 1;

            var handleWaveStarted = typeof(EnemySpawnerRunner).GetMethod(
                "HandleWaveStarted",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(handleWaveStarted, "Expected private HandleWaveStarted(int) method.");
            handleWaveStarted.Invoke(runner, new object[] { 3 });

            Assert.That(
                waveProvider.CurrentWaveIndex,
                Is.EqualTo(3),
                "Runner should propagate wave starts into WaveIndexProviderComponent for gameplay/UI consumers.");

            Object.DestroyImmediate(runnerGo);
        }

        [Test]
        public void WaveSpawner_EmitsEachSequenceEnemyType_WhenSequencesAreReadyTogether()
        {
            var wave = new WaveConfig
            {
                sequences = new List<SpawnSequenceConfig>
                {
                    new SpawnSequenceConfig
                    {
                        enemyTypeId = "grunt",
                        spawnCount = 1,
                        intervalSeconds = 1f,
                        initialDelaySeconds = 0f
                    },
                    new SpawnSequenceConfig
                    {
                        enemyTypeId = "lurker",
                        spawnCount = 1,
                        intervalSeconds = 1f,
                        initialDelaySeconds = 0f
                    }
                }
            };

            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 0,
                waves: new[] { wave },
                ramp: null);
            var spawner = new EnemyWaveSpawner(
                schedule,
                new[] { new SpawnPoint("GateA", new Vector2(0f, 0f)) });

            var ready = spawner.Tick(0.1f, currentAlive: 0);

            Assert.That(ready.Count, Is.EqualTo(2));
            Assert.That(ready[0].EnemyTypeId, Is.EqualTo("grunt"));
            Assert.That(ready[1].EnemyTypeId, Is.EqualTo("lurker"));
        }
    }
}

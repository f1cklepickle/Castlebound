using Castlebound.Gameplay.Spawning;
using NUnit.Framework;

namespace Castlebound.Tests.Spawning
{
    public class WaveMenuGateTests
    {
        [Test]
        public void PreWave_FiresOnlyAfterGapAndClear()
        {
            var schedule = new WaveScheduleRuntime(
                defaultStrategy: SpawnMarkerStrategy.RoundRobin,
                defaultSeed: 1,
                waves: new[]
                {
                    new WaveConfig
                    {
                        gapSeconds = 5f,
                        waitForClear = true,
                        sequences = new System.Collections.Generic.List<SpawnSequenceConfig>()
                    }
                },
                ramp: null);

            var spawner = new EnemyWaveSpawner(schedule, new[] { new SpawnPoint("GateA", new UnityEngine.Vector2(0, 0)) });
            bool waveEnded = false;
            spawner.OnWaveEnded += () => waveEnded = true;

            spawner.Tick(0.1f, currentAlive: 1);
            Assert.IsFalse(waveEnded, "Should not end wave while enemies remain.");

            spawner.Tick(0.1f, currentAlive: 0);
            Assert.IsFalse(waveEnded, "Gap should delay pre-wave transition.");

            spawner.Tick(5f, currentAlive: 0);
            Assert.IsTrue(waveEnded, "Pre-wave should trigger after gap completes.");
        }
    }
}

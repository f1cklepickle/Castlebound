using Castlebound.Gameplay.Balance;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Balance
{
    public class WaveBalanceTableTests
    {
        private const string BalanceStationPath = "Assets/_Project/Balance/GameBalanceStation.asset";
        private const string WaveBalanceTablePath = "Assets/_Project/Balance/WaveBalanceTable.asset";

        [Test]
        public void Defaults_MirrorCurrentWaveRuntimeTuning()
        {
            var table = ScriptableObject.CreateInstance<WaveBalanceTable>();

            try
            {
                Assert.That(table.DefaultStrategy, Is.EqualTo(SpawnMarkerStrategy.RoundRobin));
                Assert.IsFalse(table.UseDefaultSeed);
                Assert.That(table.DefaultSeed, Is.EqualTo(0));
                Assert.That(table.ResolvedDefaultSeed, Is.EqualTo(0));
                Assert.That(table.DefaultGapSeconds, Is.EqualTo(5f).Within(0.001f));
                Assert.IsTrue(table.DefaultWaitForClear);
                Assert.That(table.DefaultMaxAlive, Is.EqualTo(0));
                Assert.That(table.ActiveBuildIndex, Is.EqualTo(0));
                Assert.NotNull(table.ActiveBuild);
                Assert.That(table.ActiveBuild.BuildId, Is.EqualTo("default"));
                Assert.IsTrue(table.ActiveBuild.Enabled);
                Assert.That(table.ActiveBuild.BaseSpawnCount, Is.EqualTo(5));
                Assert.That(table.ActiveBuild.SpawnCountPerStep, Is.EqualTo(10));
                Assert.That(table.ActiveBuild.SpawnCountStepSize, Is.EqualTo(1));
                Assert.That(table.ActiveBuild.SpawnCountStartWave, Is.EqualTo(1));
                Assert.That(table.ActiveBuild.MaxSpawnCount, Is.EqualTo(0));
                Assert.That(table.ActiveBuild.IntervalSeconds, Is.EqualTo(1f).Within(0.001f));
                Assert.That(table.ActiveBuild.InitialDelaySeconds, Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ScalarProperties_ClampToSafeValues()
        {
            var table = ScriptableObject.CreateInstance<WaveBalanceTable>();

            try
            {
                table.DefaultGapSeconds = -1f;
                table.DefaultMaxAlive = -1;
                table.ActiveBuildIndex = -1;
                var build = table.ActiveBuild;
                build.BaseSpawnCount = -1;
                build.SpawnCountPerStep = -1;
                build.SpawnCountStepSize = 0;
                build.SpawnCountStartWave = 0;
                build.MaxSpawnCount = -1;
                build.IntervalSeconds = -1f;
                build.InitialDelaySeconds = -1f;

                Assert.That(table.DefaultGapSeconds, Is.EqualTo(0f));
                Assert.That(table.DefaultMaxAlive, Is.EqualTo(0));
                Assert.That(table.ActiveBuildIndex, Is.EqualTo(0));
                Assert.That(build.BaseSpawnCount, Is.EqualTo(0));
                Assert.That(build.SpawnCountPerStep, Is.EqualTo(0));
                Assert.That(build.SpawnCountStepSize, Is.EqualTo(1));
                Assert.That(build.SpawnCountStartWave, Is.EqualTo(1));
                Assert.That(build.MaxSpawnCount, Is.EqualTo(0));
                Assert.That(build.IntervalSeconds, Is.EqualTo(0f));
                Assert.That(build.InitialDelaySeconds, Is.EqualTo(0f));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ActiveBuild_ResolvesSelectedEnabledBuild()
        {
            var table = ScriptableObject.CreateInstance<WaveBalanceTable>();
            try
            {
                var slow = new WaveGenerationBuild { BuildId = "slow", BaseSpawnCount = 5 };
                var fast = new WaveGenerationBuild { BuildId = "fast", BaseSpawnCount = 15 };
                table.GeneratedBuilds = new[] { slow, fast };
                table.ActiveBuildIndex = 1;

                Assert.AreSame(fast, table.ActiveBuild);

                fast.Enabled = false;

                Assert.IsNull(table.ActiveBuild);
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ResolvedDefaultSeed_UsesSeedOnlyWhenEnabled()
        {
            var table = ScriptableObject.CreateInstance<WaveBalanceTable>();

            try
            {
                table.DefaultSeed = 42;

                Assert.That(table.ResolvedDefaultSeed, Is.EqualTo(0));

                table.UseDefaultSeed = true;

                Assert.That(table.ResolvedDefaultSeed, Is.EqualTo(42));
            }
            finally
            {
                Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void ProjectAssets_WireWaveTableThroughCentralBalanceStation()
        {
            var station = AssetDatabase.LoadAssetAtPath<GameBalanceStation>(BalanceStationPath);
            var table = AssetDatabase.LoadAssetAtPath<WaveBalanceTable>(WaveBalanceTablePath);

            Assert.NotNull(station, "Central GameBalanceStation asset must exist.");
            Assert.NotNull(table, "WaveBalanceTable asset must exist.");
            Assert.AreSame(table, station.Wave, "Central station should reference the authored wave table.");
            Assert.That(table.DefaultStrategy, Is.EqualTo(SpawnMarkerStrategy.RoundRobin));
            Assert.That(table.DefaultGapSeconds, Is.EqualTo(5f).Within(0.001f));
            Assert.IsTrue(table.DefaultWaitForClear);
            Assert.That(table.DefaultMaxAlive, Is.EqualTo(0));
            Assert.NotNull(table.ActiveBuild);
            Assert.That(table.ActiveBuild.BaseSpawnCount, Is.EqualTo(5));
            Assert.That(table.ActiveBuild.SpawnCountPerStep, Is.EqualTo(10));
            Assert.That(table.ActiveBuild.SpawnCountStepSize, Is.EqualTo(1));
            Assert.That(table.ActiveBuild.SpawnCountStartWave, Is.EqualTo(1));
            Assert.That(table.ActiveBuild.MaxSpawnCount, Is.EqualTo(0));
        }
    }
}

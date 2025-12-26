using System.Collections.Generic;
using System.Linq;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.Spawning
{
    public class SpawnMarkerOrderBuilderTests
    {
        [Test]
        public void ShufflePrecompute_CoversAllGatesAndIsDeterministicWithSeed()
        {
            var markers = new List<SpawnPoint>
            {
                new SpawnPoint("North", new Vector2(-1f, 1f)),
                new SpawnPoint("East", new Vector2(1f, 1f)),
                new SpawnPoint("South", new Vector2(1f, -1f)),
                new SpawnPoint("West", new Vector2(-1f, -1f)),
            };

            var orderA = SpawnMarkerOrderBuilder.BuildGateOrder(markers, spawnCount: 10, SpawnMarkerStrategy.ShufflePrecompute, seed: 1234);
            var orderB = SpawnMarkerOrderBuilder.BuildGateOrder(markers, spawnCount: 10, SpawnMarkerStrategy.ShufflePrecompute, seed: 1234);
            var orderC = SpawnMarkerOrderBuilder.BuildGateOrder(markers, spawnCount: 10, SpawnMarkerStrategy.ShufflePrecompute, seed: 42);

            Assert.AreEqual(10, orderA.Count, "Gate order should match the requested spawn count.");

            var coverage = new HashSet<string>(orderA.Select(p => p.GateId));
            CollectionAssert.AreEquivalent(markers.Select(m => m.GateId).ToList(), coverage.ToList(), "Each gate should appear at least once when spawnCount >= marker count.");

            CollectionAssert.AreEqual(orderA.Select(p => p.GateId).ToList(), orderB.Select(p => p.GateId).ToList(), "Same seed should produce the same gate order.");
            CollectionAssert.AreNotEqual(orderA.Select(p => p.GateId).ToList(), orderC.Select(p => p.GateId).ToList(), "Different seeds should produce different gate orders.");
        }

        [Test]
        public void ShufflePrecompute_WhenSpawnCountLowerThanMarkers_WarnsAndUsesUniqueGates()
        {
            var markers = new List<SpawnPoint>
            {
                new SpawnPoint("North", new Vector2(-1f, 1f)),
                new SpawnPoint("East", new Vector2(1f, 1f)),
                new SpawnPoint("South", new Vector2(1f, -1f)),
            };

            LogAssert.Expect(LogType.Warning, "SpawnMarkerOrderBuilder: spawnCount (2) is less than marker count (3); cannot cover all gates.");

            var order = SpawnMarkerOrderBuilder.BuildGateOrder(markers, spawnCount: 2, SpawnMarkerStrategy.ShufflePrecompute, seed: 999);

            Assert.AreEqual(2, order.Count, "Gate order should match requested spawn count even when below marker count.");
            Assert.AreEqual(2, order.Select(p => p.GateId).Distinct().Count(), "Gate order should not repeat gates when spawnCount < marker count.");
        }
    }
}

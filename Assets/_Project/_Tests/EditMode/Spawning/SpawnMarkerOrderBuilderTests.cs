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
        public void Marker_InheritsGateIdentityAndCarriesAuthoredLaneAndDirection()
        {
            var barrier = new GameObject("Barrier");
            var provider = barrier.AddComponent<GateIdProvider>();
            provider.Initialize("barrier_n");
            var markerObject = new GameObject("LeftLane");
            markerObject.transform.SetParent(barrier.transform);
            markerObject.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            var marker = markerObject.AddComponent<SpawnPointMarker>();
            marker.Initialize("Left");

            try
            {
                var point = marker.ToSpawnPoint();

                Assert.That(point.GateId, Is.EqualTo("barrier_n"));
                Assert.That(point.LaneId, Is.EqualTo("Left"));
                Assert.That(Vector2.Distance(point.ForwardDirection, Vector2.left), Is.LessThan(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(barrier);
            }
        }

        [Test]
        public void RoundRobin_GroupsLanesByGateBeforeSelectingLane()
        {
            var markers = new List<SpawnPoint>
            {
                new SpawnPoint("North", "Center", Vector2.zero, Vector2.down),
                new SpawnPoint("North", "Left", Vector2.left, Vector2.down),
                new SpawnPoint("North", "Right", Vector2.right, Vector2.down),
                new SpawnPoint("East", "Center", Vector2.one, Vector2.left),
                new SpawnPoint("East", "Left", Vector2.up, Vector2.left),
                new SpawnPoint("East", "Right", Vector2.down, Vector2.left)
            };

            var order = SpawnMarkerOrderBuilder.BuildGateOrder(markers, 6, SpawnMarkerStrategy.RoundRobin);

            CollectionAssert.AreEqual(new[] { "North", "East", "North", "East", "North", "East" }, order.Select(p => p.GateId));
            CollectionAssert.AreEqual(new[] { "Center", "Center", "Left", "Left", "Right", "Right" }, order.Select(p => p.LaneId));
        }

        [Test]
        public void ShufflePrecompute_LaneSelectionIsSeededWithoutWeightingGateCoverage()
        {
            var markers = new List<SpawnPoint>
            {
                new SpawnPoint("North", "Center", Vector2.zero, Vector2.down),
                new SpawnPoint("North", "Left", Vector2.left, Vector2.down),
                new SpawnPoint("North", "Right", Vector2.right, Vector2.down),
                new SpawnPoint("East", "Only", Vector2.one, Vector2.left)
            };

            var evenlyAuthoredMarkers = new List<SpawnPoint>(markers)
            {
                new SpawnPoint("East", "Left", Vector2.up, Vector2.left),
                new SpawnPoint("East", "Right", Vector2.down, Vector2.left)
            };

            var orderA = SpawnMarkerOrderBuilder.BuildGateOrder(markers, 8, SpawnMarkerStrategy.ShufflePrecompute, 1234);
            var orderB = SpawnMarkerOrderBuilder.BuildGateOrder(markers, 8, SpawnMarkerStrategy.ShufflePrecompute, 1234);
            var evenlyAuthoredOrder = SpawnMarkerOrderBuilder.BuildGateOrder(evenlyAuthoredMarkers, 8, SpawnMarkerStrategy.ShufflePrecompute, 1234);

            CollectionAssert.AreEqual(orderA.Select(p => $"{p.GateId}:{p.LaneId}"), orderB.Select(p => $"{p.GateId}:{p.LaneId}"));
            CollectionAssert.AreEqual(orderA.Select(p => p.GateId), evenlyAuthoredOrder.Select(p => p.GateId), "Adding lanes must not change seeded barrier selection.");
        }

        [Test]
        public void ShufflePrecompute_CoversEveryLaneBeforeRepeatingWithinGate()
        {
            var markers = new List<SpawnPoint>
            {
                new SpawnPoint("North", "Center", Vector2.zero, Vector2.down),
                new SpawnPoint("North", "Left", Vector2.left, Vector2.down),
                new SpawnPoint("North", "Right", Vector2.right, Vector2.down)
            };

            var order = SpawnMarkerOrderBuilder.BuildGateOrder(markers, 3, SpawnMarkerStrategy.ShufflePrecompute, 42);

            CollectionAssert.AreEquivalent(new[] { "Center", "Left", "Right" }, order.Select(p => p.LaneId));
        }

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

            LogAssert.Expect(LogType.Warning, "SpawnMarkerOrderBuilder: spawnCount (2) is less than gate count (3); cannot cover all gates.");

            var order = SpawnMarkerOrderBuilder.BuildGateOrder(markers, spawnCount: 2, SpawnMarkerStrategy.ShufflePrecompute, seed: 999);

            Assert.AreEqual(2, order.Count, "Gate order should match requested spawn count even when below marker count.");
            Assert.AreEqual(2, order.Select(p => p.GateId).Distinct().Count(), "Gate order should not repeat gates when spawnCount < marker count.");
        }
    }
}

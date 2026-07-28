using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Spawning
{
    public static class SpawnMarkerOrderBuilder
    {
        public static List<SpawnPoint> BuildGateOrder(IReadOnlyList<SpawnPoint> spawnPoints, int spawnCount, SpawnMarkerStrategy strategy, int? seed = null)
        {
            var result = new List<SpawnPoint>();

            if (spawnCount <= 0 || spawnPoints == null || spawnPoints.Count == 0)
            {
                return result;
            }

            BuildGroups(spawnPoints, out var gateIds, out var lanesByGate);
            var gateIndices = new List<int>(spawnCount);
            var rng = seed.HasValue ? new System.Random(seed.Value) : null;

            switch (strategy)
            {
                case SpawnMarkerStrategy.RoundRobin:
                    BuildRoundRobin(gateIds.Count, spawnCount, gateIndices);
                    break;
                case SpawnMarkerStrategy.ShufflePrecompute:
                    BuildShuffleWithCoverage(gateIds.Count, spawnCount, gateIndices, rng);
                    break;
                default:
                    BuildRoundRobin(gateIds.Count, spawnCount, gateIndices);
                    break;
            }

            SelectLanes(gateIndices, lanesByGate, strategy, rng, result);

            return result;
        }

        private static void BuildGroups(
            IReadOnlyList<SpawnPoint> spawnPoints,
            out List<string> gateIds,
            out List<List<SpawnPoint>> lanesByGate)
        {
            gateIds = new List<string>();
            lanesByGate = new List<List<SpawnPoint>>();
            var gateIndexById = new Dictionary<string, int>();

            for (var markerIndex = 0; markerIndex < spawnPoints.Count; markerIndex++)
            {
                var point = spawnPoints[markerIndex];
                var groupingId = string.IsNullOrWhiteSpace(point.GateId)
                    ? $"__ungrouped_{markerIndex}"
                    : point.GateId;

                if (!gateIndexById.TryGetValue(groupingId, out var gateIndex))
                {
                    gateIndex = gateIds.Count;
                    gateIndexById.Add(groupingId, gateIndex);
                    gateIds.Add(groupingId);
                    lanesByGate.Add(new List<SpawnPoint>());
                }

                lanesByGate[gateIndex].Add(point);
            }
        }

        private static void BuildRoundRobin(int gateCount, int spawnCount, List<int> output)
        {
            var gateIndex = 0;
            for (int i = 0; i < spawnCount; i++)
            {
                output.Add(gateIndex);
                gateIndex = (gateIndex + 1) % gateCount;
            }
        }

        private static void BuildShuffleWithCoverage(int gateCount, int spawnCount, List<int> output, System.Random rng)
        {
            if (spawnCount < gateCount)
            {
                Debug.LogWarning($"SpawnMarkerOrderBuilder: spawnCount ({spawnCount}) is less than gate count ({gateCount}); cannot cover all gates.");
                AppendShuffledDistinct(output, gateCount, spawnCount, rng);
            }
            else
            {
                // Ensure each gate appears once.
                for (int i = 0; i < gateCount; i++)
                {
                    output.Add(i);
                }

                // Fill remaining slots with random gates.
                while (output.Count < spawnCount)
                {
                    output.Add(GetRandomIndex(gateCount, rng));
                }

                // Shuffle the full order.
                Shuffle(output, rng);
            }
        }

        private static void SelectLanes(
            IReadOnlyList<int> gateIndices,
            IReadOnlyList<List<SpawnPoint>> lanesByGate,
            SpawnMarkerStrategy strategy,
            System.Random rng,
            List<SpawnPoint> output)
        {
            var nextLaneByGate = new int[lanesByGate.Count];
            var shuffledLaneIndices = new List<List<int>>(lanesByGate.Count);
            for (var gateIndex = 0; gateIndex < lanesByGate.Count; gateIndex++)
            {
                var indices = new List<int>(lanesByGate[gateIndex].Count);
                for (var laneIndex = 0; laneIndex < lanesByGate[gateIndex].Count; laneIndex++)
                {
                    indices.Add(laneIndex);
                }

                if (strategy == SpawnMarkerStrategy.ShufflePrecompute)
                {
                    Shuffle(indices, rng);
                }

                shuffledLaneIndices.Add(indices);
            }

            foreach (var gateIndex in gateIndices)
            {
                var lanes = lanesByGate[gateIndex];
                int laneIndex;
                if (strategy == SpawnMarkerStrategy.ShufflePrecompute)
                {
                    if (nextLaneByGate[gateIndex] >= lanes.Count)
                    {
                        Shuffle(shuffledLaneIndices[gateIndex], rng);
                        nextLaneByGate[gateIndex] = 0;
                    }

                    laneIndex = shuffledLaneIndices[gateIndex][nextLaneByGate[gateIndex]++];
                }
                else
                {
                    laneIndex = nextLaneByGate[gateIndex] % lanes.Count;
                    nextLaneByGate[gateIndex]++;
                }

                output.Add(lanes[laneIndex]);
            }
        }

        private static void AppendShuffledDistinct(List<int> output, int gateCount, int takeCount, System.Random rng)
        {
            var pool = new List<int>(gateCount);
            for (int i = 0; i < gateCount; i++)
            {
                pool.Add(i);
            }

            Shuffle(pool, rng);

            for (int i = 0; i < takeCount && i < pool.Count; i++)
            {
                output.Add(pool[i]);
            }
        }

        private static int GetRandomIndex(int exclusiveMax, System.Random rng)
        {
            if (rng != null)
            {
                return rng.Next(0, exclusiveMax);
            }

            return UnityEngine.Random.Range(0, exclusiveMax);
        }

        private static void Shuffle(List<int> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = rng != null ? rng.Next(0, i + 1) : UnityEngine.Random.Range(0, i + 1);
                var tmp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = tmp;
            }
        }
    }
}

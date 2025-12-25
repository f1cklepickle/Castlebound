using System;
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

            switch (strategy)
            {
                case SpawnMarkerStrategy.RoundRobin:
                    BuildRoundRobin(spawnPoints, spawnCount, result);
                    break;
                case SpawnMarkerStrategy.ShufflePrecompute:
                    BuildShuffleWithCoverage(spawnPoints, spawnCount, result, seed);
                    break;
                default:
                    BuildRoundRobin(spawnPoints, spawnCount, result);
                    break;
            }

            return result;
        }

        private static void BuildRoundRobin(IReadOnlyList<SpawnPoint> spawnPoints, int spawnCount, List<SpawnPoint> output)
        {
            var gateIndex = 0;
            for (int i = 0; i < spawnCount; i++)
            {
                output.Add(spawnPoints[gateIndex]);
                gateIndex = (gateIndex + 1) % spawnPoints.Count;
            }
        }

        private static void BuildShuffleWithCoverage(IReadOnlyList<SpawnPoint> spawnPoints, int spawnCount, List<SpawnPoint> output, int? seed)
        {
            var rng = seed.HasValue ? new System.Random(seed.Value) : null;
            var markerCount = spawnPoints.Count;

            var indices = new List<int>(spawnCount);

            if (spawnCount < markerCount)
            {
                Debug.LogWarning($"SpawnMarkerOrderBuilder: spawnCount ({spawnCount}) is less than marker count ({markerCount}); cannot cover all gates.");
                AppendShuffledDistinct(indices, markerCount, spawnCount, rng);
            }
            else
            {
                // Ensure each gate appears once.
                for (int i = 0; i < markerCount; i++)
                {
                    indices.Add(i);
                }

                // Fill remaining slots with random gates.
                while (indices.Count < spawnCount)
                {
                    indices.Add(GetRandomIndex(markerCount, rng));
                }

                // Shuffle the full order.
                Shuffle(indices, rng);
            }

            foreach (var idx in indices)
            {
                output.Add(spawnPoints[idx]);
            }
        }

        private static void AppendShuffledDistinct(List<int> output, int markerCount, int takeCount, System.Random rng)
        {
            var pool = new List<int>(markerCount);
            for (int i = 0; i < markerCount; i++)
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

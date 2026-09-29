using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Fixed storage, no physics queries or all-pairs scan. Buckets contain reachable AABBs,
    // not just starting centres, so later bounded redirection cannot escape discovery.
    public sealed class EnemySeparationDiscovery
    {
        public const int BodyCapacity = 256;
        private const int BucketCapacity = 4096, NodeCapacity = 16384;
        private const float CellSize = 1f;
        private readonly Vector2Int[] keys = new Vector2Int[BucketCapacity];
        private readonly int[] bucketStamps = new int[BucketCapacity], heads = new int[BucketCapacity];
        private readonly int[] owners = new int[NodeCapacity], next = new int[NodeCapacity];
        private readonly int[] pairStamps = new int[BodyCapacity * BodyCapacity];
        private readonly Rect[] envelopes = new Rect[BodyCapacity];
        private int stamp, nodeCount;
        public EnemySeparationPair[] Pairs { get; }
        public int PairCount { get; private set; }
        public bool Saturated { get; private set; }

        public EnemySeparationDiscovery(int pairCapacity = 8192)
        {
            if (pairCapacity < 1) throw new ArgumentOutOfRangeException(nameof(pairCapacity));
            Pairs = new EnemySeparationPair[pairCapacity];
        }

        public bool Build(EnemySeparationBody[] bodies, int count)
        {
            PairCount = nodeCount = 0; Saturated = false;
            if (++stamp == int.MaxValue)
            { Array.Clear(bucketStamps, 0, bucketStamps.Length); Array.Clear(pairStamps, 0, pairStamps.Length); stamp = 1; }
            if (count < 0 || count > BodyCapacity || count > bodies.Length) return Fail();
            for (int i = 0; i < count; i++)
            {
                var body = bodies[i];
                float reach = body.Radius + Mathf.Max(0f, body.Budget) + body.External.magnitude + 0.001f;
                if (!Finite(reach) || !Finite(body.Position.x) || !Finite(body.Position.y) || reach < 0f) return Fail();
                Rect envelope = envelopes[i] = new Rect(body.Position - Vector2.one * reach, Vector2.one * (2f * reach));
                int x0 = Mathf.FloorToInt(envelope.xMin / CellSize), x1 = Mathf.FloorToInt(envelope.xMax / CellSize);
                int y0 = Mathf.FloorToInt(envelope.yMin / CellSize), y1 = Mathf.FloorToInt(envelope.yMax / CellSize);
                if ((long)x1 - x0 > 64 || (long)y1 - y0 > 64 ||
                    ((long)x1 - x0 + 1) * ((long)y1 - y0 + 1) > 64) return Fail();
                for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    int bucket = Bucket(new Vector2Int(x, y));
                    if (bucket < 0 || nodeCount == NodeCapacity) return Fail();
                    for (int n = heads[bucket]; n >= 0; n = next[n])
                    {
                        int j = owners[n], key = j * BodyCapacity + i;
                        if (pairStamps[key] == stamp || !envelope.Overlaps(envelopes[j])) continue;
                        pairStamps[key] = stamp;
                        if (PairCount == Pairs.Length) return Fail();
                        bool first = bodies[j].Id < body.Id;
                        Pairs[PairCount++] = new EnemySeparationPair { First = first ? j : i, Second = first ? i : j,
                            FirstId = first ? bodies[j].Id : body.Id, SecondId = first ? body.Id : bodies[j].Id };
                    }
                    owners[nodeCount] = i; next[nodeCount] = heads[bucket]; heads[bucket] = nodeCount++;
                }
            }
            Array.Sort(Pairs, 0, PairCount);
            return true;
        }

        private int Bucket(Vector2Int key)
        {
            int slot = unchecked(key.x * 73856093 ^ key.y * 19349663) & (BucketCapacity - 1);
            for (int probe = 0; probe < BucketCapacity; probe++, slot = (slot + 1) & (BucketCapacity - 1))
            {
                if (bucketStamps[slot] != stamp)
                { bucketStamps[slot] = stamp; keys[slot] = key; heads[slot] = -1; return slot; }
                if (keys[slot] == key) return slot;
            }
            return -1;
        }
        private bool Fail() { Saturated = true; return false; }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationWorldCacheTests
    {
        private Grid authority;
        private StaticNavigationLayout layout;
        [SetUp] public void SetUp()
        {
            authority = new GameObject("WorldGrid", typeof(Grid)).GetComponent<Grid>();
            layout = new StaticNavigationLayout(authority, new BoundsInt(-8, -8, 0, 32, 32, 1));
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(authority.gameObject);

        [Test]
        public void ColdCell_QueuesOnce_AndWarmReadsPerformNoSampling()
        {
            int calls = 0;
            var cache = new StaticNavigationWorldCache(layout, p => { calls++; return StaticNavigationSampleState.Clear; },
                (a, b) => StaticNavigationSampleState.Clear);
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(calls, Is.Zero);
            Assert.That(cache.PendingSamples, Is.EqualTo(1));
            Assert.That(cache.SamplePending(0), Is.Zero);
            Assert.That(cache.SamplePending(1), Is.EqualTo(1));
            for (int i = 0; i < 10; i++) Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(cache.SamplePending(128), Is.Zero);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(cache.DebugUniqueCellSamples, Is.EqualTo(1));
            Assert.That(cache.DebugCacheHits, Is.EqualTo(10));
        }

        [Test]
        public void UndirectedEdge_ReusesEndpointsAcrossOverlappingRegions()
        {
            int points = 0, edges = 0;
            var cache = new StaticNavigationWorldCache(layout,
                p => { points++; return StaticNavigationSampleState.Clear; },
                (a, b) => { edges++; return StaticNavigationSampleState.Clear; });
            Assert.That(cache.GetTraversalState(Vector2Int.zero, Vector2Int.right), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.SamplePending(128), Is.EqualTo(2));
            Assert.That(cache.GetTraversalState(Vector2Int.zero, Vector2Int.right), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.SamplePending(128), Is.EqualTo(1));
            Assert.That(cache.GetTraversalState(Vector2Int.right, Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(cache.GetTraversalState(Vector2Int.right, Vector2Int.right * 2), Is.EqualTo(StaticNavigationSampleState.Unknown));
            cache.SamplePending(128);
            cache.GetTraversalState(Vector2Int.right, Vector2Int.right * 2);
            cache.SamplePending(128);
            Assert.That(points, Is.EqualTo(3));
            Assert.That(edges, Is.EqualTo(2));
            Assert.That(cache.DebugUniqueEdgeSamples, Is.EqualTo(2));
            Assert.That(cache.DebugSamplingQueries, Is.EqualTo(5));
        }

        [Test]
        public void BlockedOrthogonalEdge_RejectsDiagonalEvenWhenDiagonalSweepIsClear()
        {
            var cache = new StaticNavigationWorldCache(layout, p => StaticNavigationSampleState.Clear,
                (a, b) => a == new Vector2(0.125f, 0.125f) && b == new Vector2(0.375f, 0.125f)
                    ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear);
            StaticNavigationSampleState result = StaticNavigationSampleState.Unknown;
            for (int i = 0; i < 8 && result == StaticNavigationSampleState.Unknown; i++)
            {
                result = cache.GetTraversalState(Vector2Int.zero, Vector2Int.one);
                cache.SamplePending(128);
            }
            Assert.That(result, Is.EqualTo(StaticNavigationSampleState.Blocked));
            Assert.That(cache.GetTraversalState(Vector2Int.one, Vector2Int.zero), Is.EqualTo(result));
        }

        [Test]
        public void UnknownSamples_RemainPendingAndRetryWithoutFalseClear()
        {
            bool saturated = true;
            var cache = new StaticNavigationWorldCache(layout,
                p => saturated ? StaticNavigationSampleState.Unknown : StaticNavigationSampleState.Clear,
                (a, b) => StaticNavigationSampleState.Clear);
            cache.GetCellState(Vector2Int.zero);
            Assert.That(cache.SamplePending(128), Is.EqualTo(1));
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.DebugUniqueCellSamples, Is.Zero);
            saturated = false;
            cache.SamplePending(1);
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(cache.DebugSamplingQueries, Is.EqualTo(2));
        }

        [Test]
        public void SamplingBudget_BoundsQueriesRegardlessOfPendingRegionSize()
        {
            var cache = new StaticNavigationWorldCache(layout, p => StaticNavigationSampleState.Clear,
                (a, b) => StaticNavigationSampleState.Clear);
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) cache.GetCellState(new Vector2Int(x, y));
            Assert.That(cache.SamplePending(128), Is.EqualTo(128));
            Assert.That(cache.DebugUniqueCellSamples, Is.EqualTo(128));
            Assert.That(cache.PendingSamples, Is.EqualTo(128));
        }
    }
}

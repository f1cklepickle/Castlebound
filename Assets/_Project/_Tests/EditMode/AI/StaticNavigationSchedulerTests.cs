using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationSchedulerTests
    {
        private Grid authority;
        private int frame;
        [SetUp] public void SetUp()
        {
            authority = new GameObject("WorldGrid", typeof(Grid)).GetComponent<Grid>();
            frame = 0;
        }
        [TearDown] public void TearDown() => Object.DestroyImmediate(authority.gameObject);

        [Test]
        public void ColdRequestSpansTicks_WarmRequestReusesAllQueries_AndExpansionCapStillApplies()
        {
            var bounds = new BoundsInt(0, 0, 0, 50, 50, 1);
            var cache = new StaticNavigationWorldCache(new StaticNavigationLayout(authority, bounds),
                p => Mathf.FloorToInt(p.x * 4f) == 25 ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear,
                (a, b) => StaticNavigationSampleState.Clear);
            var scheduler = new StaticNavigationScheduler(cache);
            Assert.IsTrue(scheduler.TrySubmit(bounds, Vector2Int.zero, new Vector2Int(49, 49), out int first));
            scheduler.Tick(frame++);
            Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
            var cold = Finish(scheduler, first);
            Assert.That(cold.Status, Is.EqualTo(StaticNavigationPathStatus.NoPath));
            int coldQueries = cache.DebugSamplingQueries, coldTicks = scheduler.DebugTicks;
            Assert.That(coldQueries, Is.GreaterThan(128));
            Assert.IsTrue(scheduler.TrySubmit(bounds, Vector2Int.zero, new Vector2Int(49, 49), out int second));
            scheduler.Tick(frame++);
            Assert.That(scheduler.DebugLastExpansions, Is.EqualTo(256));
            Assert.That(scheduler.DebugLastSamplingQueries, Is.Zero);
            Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
            var warm = Finish(scheduler, second);
            Assert.That(warm.ExpandedNodes, Is.EqualTo(cold.ExpandedNodes));
            Assert.That(cache.DebugSamplingQueries, Is.EqualTo(coldQueries));
            TestContext.WriteLine($"Cold: {coldQueries} samples, {cold.ExpandedNodes} expansions, {coldTicks} ticks; warm: 0 new samples, {scheduler.DebugTicks - coldTicks} ticks.");
        }

        [Test]
        public void FIFOAndOnePublicationPerFrame_AreEnforcedEvenForTrivialWarmRoutes()
        {
            var bounds = new BoundsInt(0, 0, 0, 8, 8, 1);
            var cache = Open(bounds);
            var scheduler = new StaticNavigationScheduler(cache);
            Assert.IsTrue(scheduler.TrySubmit(bounds, Vector2Int.zero, Vector2Int.zero, out int a));
            Assert.IsTrue(scheduler.TrySubmit(bounds, Vector2Int.one, Vector2Int.one, out int b));
            Assert.IsTrue(scheduler.Tick(0));
            Assert.IsTrue(scheduler.TryGetCompleted(out int id, out var result));
            Assert.That(id, Is.EqualTo(a));
            Assert.That(result.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
            Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
            Assert.IsFalse(scheduler.Tick(0), "A repeated service call cannot spend another frame budget.");
            Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
            Assert.IsTrue(scheduler.Tick(1));
            Assert.IsTrue(scheduler.TryGetCompleted(out id, out _));
            Assert.That(id, Is.EqualTo(b));
        }

        [Test]
        public void OverlappingRequestBounds_ReusePreviouslySampledRegion()
        {
            var bounds = new BoundsInt(0, 0, 0, 32, 32, 1);
            var cache = Open(bounds);
            var scheduler = new StaticNavigationScheduler(cache);
            scheduler.TrySubmit(bounds, Vector2Int.zero, new Vector2Int(24, 24), out int first);
            var initial = Finish(scheduler, first);
            int before = cache.DebugSamplingQueries;
            var subregion = new BoundsInt(0, 0, 0, 16, 16, 1);
            scheduler.TrySubmit(subregion, Vector2Int.zero, new Vector2Int(12, 12), out int second);
            var overlap = Finish(scheduler, second);
            Assert.That(initial.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
            Assert.That(overlap.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
            Assert.That(cache.DebugSamplingQueries, Is.EqualTo(before));
        }

        [Test]
        public void OversizedOrOutOfCacheRequests_AreRejectedWithoutSampling()
        {
            var cache = Open(new BoundsInt(0, 0, 0, 128, 128, 1));
            var scheduler = new StaticNavigationScheduler(cache);
            Assert.IsFalse(scheduler.TrySubmit(new BoundsInt(0, 0, 0, 65, 64, 1), Vector2Int.zero, Vector2Int.one, out _));
            Assert.IsFalse(scheduler.TrySubmit(new BoundsInt(0, 0, 0, 97, 1, 1), Vector2Int.zero, Vector2Int.one, out _));
            Assert.IsFalse(scheduler.TrySubmit(new BoundsInt(-1, 0, 0, 8, 8, 1), Vector2Int.zero, Vector2Int.one, out _));
            scheduler.Tick(0);
            Assert.That(cache.DebugSamplingQueries, Is.Zero);
            Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
        }

        [Test]
        public void SaturationCannotPublishFalseRoute_AndRetriesStayWithinBudget()
        {
            bool saturated = true;
            var bounds = new BoundsInt(0, 0, 0, 8, 8, 1);
            var cache = new StaticNavigationWorldCache(new StaticNavigationLayout(authority, bounds),
                p => saturated ? StaticNavigationSampleState.Unknown : StaticNavigationSampleState.Clear,
                (a, b) => StaticNavigationSampleState.Clear);
            var scheduler = new StaticNavigationScheduler(cache);
            scheduler.TrySubmit(bounds, Vector2Int.zero, Vector2Int.one, out int id);
            scheduler.Tick(frame++);
            Assert.That(scheduler.DebugLastSamplingQueries, Is.EqualTo(128));
            Assert.That(scheduler.DebugLastExpansions, Is.Zero);
            Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
            saturated = false;
            Assert.That(Finish(scheduler, id).Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
        }

        [Test]
        public void ActualVaultLikeCollider_ColdAndWarmPathsMatchWithoutRepeatedPhysicsQueries()
        {
            authority.transform.position = new Vector3(10000f, 10000f, 0f);
            var obstacle = new GameObject("Vault-like solid");
            try
            {
                obstacle.layer = LayerMask.NameToLayer("Walls");
                obstacle.transform.position = authority.transform.position + new Vector3(2f, 2f, 0f);
                obstacle.AddComponent<BoxCollider2D>().size = new Vector2(1.1f, 0.9f);
                Physics2D.SyncTransforms();
                var bounds = new BoundsInt(0, 0, 0, 16, 16, 1);
                var world = new StaticNavigationWorld2D(0.87684506f);
                var cache = new StaticNavigationWorldCache(new StaticNavigationLayout(authority, bounds), world);
                var scheduler = new StaticNavigationScheduler(cache);
                scheduler.TrySubmit(bounds, Vector2Int.zero, new Vector2Int(15, 15), out int a);
                var cold = Finish(scheduler, a);
                Assert.That(cold.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
                int queries = world.DebugPhysicsQueryCount;
                Assert.That(queries, Is.EqualTo(cache.DebugUniqueCellSamples + cache.DebugUniqueEdgeSamples));
                scheduler.TrySubmit(bounds, Vector2Int.zero, new Vector2Int(15, 15), out int b);
                var warm = Finish(scheduler, b);
                CollectionAssert.AreEqual(cold.Cells, warm.Cells);
                Assert.That(world.DebugPhysicsQueryCount, Is.EqualTo(queries));
                Assert.That(warm.ExpandedNodes, Is.EqualTo(cold.ExpandedNodes));
                TestContext.WriteLine($"Vault route: {queries} cold physics queries, 0 warm queries; {cold.ExpandedNodes} expansions each.");
            }
            finally { Object.DestroyImmediate(obstacle); Physics2D.SyncTransforms(); }
        }

        private StaticNavigationWorldCache Open(BoundsInt bounds) => new StaticNavigationWorldCache(
            new StaticNavigationLayout(authority, bounds), p => StaticNavigationSampleState.Clear, (a, b) => StaticNavigationSampleState.Clear);

        private StaticNavigationPathResult Finish(StaticNavigationScheduler scheduler, int expectedId)
        {
            for (int i = 0; i < 2000; i++)
            {
                scheduler.Tick(frame++);
                Assert.That(scheduler.DebugLastSamplingQueries, Is.LessThanOrEqualTo(128));
                Assert.That(scheduler.DebugLastExpansions, Is.LessThanOrEqualTo(256));
                if (!scheduler.TryGetCompleted(out int id, out var result)) continue;
                Assert.That(id, Is.EqualTo(expectedId));
                Assert.IsFalse(scheduler.TryGetCompleted(out _, out _));
                return result;
            }
            Assert.Fail("Scheduler failed to finish within the fixture's finite tick limit.");
            return null;
        }
    }
}

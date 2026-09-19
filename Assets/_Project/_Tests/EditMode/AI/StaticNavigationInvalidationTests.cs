using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationInvalidationTests
    {
        private Grid authority;
        private GameObject obstacle;
        private StaticNavigationLayout layout;
        [SetUp] public void SetUp()
        {
            authority = new GameObject("WorldGrid", typeof(Grid)).GetComponent<Grid>();
            authority.transform.position = new Vector3(100f, 100f, 0f);
            layout = new StaticNavigationLayout(authority, new BoundsInt(-32, -32, 0, 96, 64, 1));
        }
        [TearDown] public void TearDown()
        {
            if (obstacle != null) Object.DestroyImmediate(obstacle);
            Object.DestroyImmediate(authority.gameObject);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ColliderChange_ResamplesLocally_WithoutDiscardingDistantCell(bool initiallyEnabled)
        {
            obstacle = new GameObject("Barrier solid", typeof(BoxCollider2D));
            obstacle.transform.position = layout.CellToWorld(Vector2Int.zero);
            var box = obstacle.GetComponent<BoxCollider2D>();
            box.size = Vector2.one;
            Physics2D.SyncTransforms();
            Rect dirty = Rect.MinMaxRect(box.bounds.min.x, box.bounds.min.y, box.bounds.max.x, box.bounds.max.y);
            box.enabled = initiallyEnabled;
            var cache = new StaticNavigationWorldCache(layout);
            var far = new Vector2Int(40, 0);
            cache.GetCellState(Vector2Int.zero); cache.GetCellState(far); cache.SamplePending(128);
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(initiallyEnabled ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear));
            cache.InvalidateWorldBounds(dirty, dirty);
            box.enabled = !initiallyEnabled;
            Physics2D.SyncTransforms();
            int queries = cache.DebugSamplingQueries;
            Assert.That(cache.GetCellState(far), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.SamplePending(128), Is.EqualTo(1));
            Assert.That(cache.DebugSamplingQueries, Is.EqualTo(queries + 1));
            Assert.That(cache.GetCellState(Vector2Int.zero), Is.EqualTo(initiallyEnabled ? StaticNavigationSampleState.Clear : StaticNavigationSampleState.Blocked));
        }

        [Test]
        public void OldAndNewBounds_OverlapSafely_AndDoNotInvalidateSpaceBetweenDistantEdits()
        {
            var cache = Open();
            var oldCell = Vector2Int.zero; var newCell = new Vector2Int(40, 0); var between = new Vector2Int(20, 0);
            foreach (var c in new[] { oldCell, newCell, between }) cache.GetCellState(c);
            cache.SamplePending(128);
            cache.InvalidateWorldBounds(At(oldCell), At(newCell));
            cache.InvalidateWorldBounds(At(oldCell), At(oldCell));
            Assert.That(cache.Revision, Is.EqualTo(2));
            Assert.That(cache.GetCellState(oldCell), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.GetCellState(newCell), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.GetCellState(between), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(cache.SamplePending(128), Is.EqualTo(2));
        }

        [TestCase(15, 16)]
        [TestCase(-1, 0)]
        public void EdgesAcrossRegionBoundaries_AndDiagonalDependencies_AreInvalidated(int left, int right)
        {
            var cache = Open();
            var a = new Vector2Int(left, 0); var b = new Vector2Int(right, 0); var diagonal = b + Vector2Int.up;
            WarmEdge(cache, a, b); WarmEdge(cache, a, diagonal);
            var distant = new Vector2Int(40, 10);
            WarmEdge(cache, distant, distant + Vector2Int.right);
            cache.InvalidateWorldBounds(At(b), At(b));
            Assert.That(cache.GetTraversalState(a, b), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.GetTraversalState(diagonal, a), Is.EqualTo(StaticNavigationSampleState.Unknown));
            Assert.That(cache.GetTraversalState(distant, distant + Vector2Int.right), Is.EqualTo(StaticNavigationSampleState.Clear));
            WarmEdge(cache, a, b); WarmEdge(cache, a, diagonal);
        }

        [Test]
        public void PaddingIncludesBodyMarginAndOneCell_AndPendingEdgesCannotTrustOldEndpoints()
        {
            bool blocked = false;
            int edgeQueries = 0;
            var cache = new StaticNavigationWorldCache(layout,
                p => blocked ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear,
                (a, b) => { edgeQueries++; return StaticNavigationSampleState.Clear; });
            var a = Vector2Int.zero; var b = Vector2Int.right;
            cache.GetTraversalState(a, b); cache.SamplePending(128);
            cache.GetTraversalState(a, b); // Edge queued with old clear endpoints.
            Vector2 p = layout.CellToWorld(a) - Vector2.right * (cache.ClearanceRadius + cache.CellSize - 0.01f);
            var dirty = new Rect(p, Vector2.zero);
            cache.InvalidateWorldBounds(dirty, dirty);
            blocked = true;
            Assert.That(cache.SamplePending(128), Is.Zero);
            Assert.That(cache.GetCellState(a), Is.EqualTo(StaticNavigationSampleState.Unknown));
            cache.SamplePending(128);
            Assert.That(cache.GetTraversalState(a, b), Is.EqualTo(StaticNavigationSampleState.Blocked));
            Assert.That(edgeQueries, Is.Zero);
        }

        [Test]
        public void ActiveSearchRestarts_WithoutResettingTotalExpansionBudget()
        {
            var cache = Open();
            var bounds = new BoundsInt(0, 0, 0, 8, 8, 1);
            var search = new StaticNavigationSearch(cache, bounds, Vector2Int.zero, new Vector2Int(7, 7), 3);
            for (int i = 0; i < 100 && search.ExpandedNodes < 2; i++)
            { search.Step(1); cache.SamplePending(128); }
            Assert.That(search.ExpandedNodes, Is.EqualTo(2));
            cache.InvalidateWorldBounds(At(Vector2Int.zero), At(Vector2Int.zero));
            for (int i = 0; i < 100 && search.Result == null; i++)
            { search.Step(256); cache.SamplePending(128); }
            Assert.That(search.Result, Is.Not.Null);
            Assert.That(search.Result.Status, Is.EqualTo(StaticNavigationPathStatus.BudgetExhausted));
            Assert.That(search.ExpandedNodes, Is.EqualTo(3));
            Assert.That(search.Result.WorldRevision, Is.EqualTo(cache.Revision));
        }

        [Test]
        public void CompletedButUnreadResults_AreRepairedInFifoOrder_WithUnchangedTickCaps()
        {
            bool blocked = false;
            var cache = new StaticNavigationWorldCache(layout,
                p => blocked ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear,
                (a, b) => StaticNavigationSampleState.Clear);
            var scheduler = new StaticNavigationScheduler(cache);
            var bounds = new BoundsInt(0, 0, 0, 2, 2, 1);
            Assert.That(scheduler.TrySubmit(bounds, Vector2Int.zero, Vector2Int.one, out int first), Is.True);
            Assert.That(scheduler.TrySubmit(bounds, Vector2Int.zero, Vector2Int.one, out int second), Is.True);
            int frame = 0;
            for (; frame < 20; frame++) scheduler.Tick(frame);
            blocked = true;
            cache.InvalidateWorldBounds(At(Vector2Int.zero), At(Vector2Int.zero));
            Assert.That(scheduler.TryGetCompleted(out _, out _), Is.False);
            foreach (int expected in new[] { first, second })
            {
                int actual = 0;
                StaticNavigationPathResult result = null;
                for (int i = 0; i < 100 && result == null; i++)
                {
                    scheduler.Tick(frame++);
                    Assert.That(scheduler.DebugLastExpansions, Is.LessThanOrEqualTo(256));
                    Assert.That(scheduler.DebugLastSamplingQueries, Is.LessThanOrEqualTo(128));
                    scheduler.TryGetCompleted(out actual, out result);
                }
                Assert.That(actual, Is.EqualTo(expected));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Status, Is.EqualTo(StaticNavigationPathStatus.InvalidStart));
            }
        }

        private StaticNavigationWorldCache Open() => new StaticNavigationWorldCache(layout,
            p => StaticNavigationSampleState.Clear, (a, b) => StaticNavigationSampleState.Clear);
        private Rect At(Vector2Int cell) => new Rect(layout.CellToWorld(cell), Vector2.zero);
        private static void WarmEdge(StaticNavigationWorldCache cache, Vector2Int a, Vector2Int b)
        {
            for (int i = 0; i < 8 && cache.GetTraversalState(a, b) == StaticNavigationSampleState.Unknown; i++)
                cache.SamplePending(128);
            Assert.That(cache.GetTraversalState(a, b), Is.EqualTo(StaticNavigationSampleState.Clear));
        }
    }
}

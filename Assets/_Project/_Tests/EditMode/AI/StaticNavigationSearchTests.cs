using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationSearchTests
    {
        private Grid authority;
        [SetUp] public void SetUp() => authority = new GameObject("WorldGrid", typeof(Grid)).GetComponent<Grid>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(authority.gameObject);

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(256)]
        public void IncrementalAndLazySearch_MatchSynchronousDetourExactly(int stepSize)
        {
            var bounds = new BoundsInt(-4, -4, 0, 24, 24, 1);
            System.Func<Vector2, bool> point = p => !(p.x > 1f && p.x < 2f && p.y > 0f && p.y < 3f);
            var eager = new StaticNavigationGrid(authority, bounds, point, (a, b) => true);
            var start = new Vector2Int(-2, 6); var goal = new Vector2Int(17, 6);
            var expected = new StaticNavigationPathfinder().FindPath(eager, start, goal);
            Assert.That(expected.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
            var cache = new StaticNavigationWorldCache(new StaticNavigationLayout(authority, bounds),
                p => point(p) ? StaticNavigationSampleState.Clear : StaticNavigationSampleState.Blocked,
                (a, b) => StaticNavigationSampleState.Clear);
            var search = new StaticNavigationSearch(cache, bounds, start, goal);
            search.Step(stepSize);
            Assert.IsTrue(search.WaitingForSampling);
            Assert.IsNull(search.Result, "No partial route may be published while data is unknown.");
            for (int i = 0; i < 3000 && search.Result == null; i++)
            {
                cache.SamplePending(13);
                Assert.That(search.Step(stepSize), Is.LessThanOrEqualTo(stepSize));
            }
            Assert.IsNotNull(search.Result);
            Assert.That(search.Result.Status, Is.EqualTo(expected.Status));
            Assert.That(search.Result.Cost, Is.EqualTo(expected.Cost));
            Assert.That(search.ExpandedNodes, Is.EqualTo(expected.ExpandedNodes));
            CollectionAssert.AreEqual(expected.Cells, search.Result.Cells);
            CollectionAssert.AreEqual(expected.WorldPoints, search.Result.WorldPoints);
        }

        [Test]
        public void StepsOf256_ResumeAndEnforce4096TotalExpansions()
        {
            var bounds = new BoundsInt(0, 0, 0, 70, 70, 1);
            var grid = new StaticNavigationGrid(authority, bounds,
                p => !(Mathf.FloorToInt(p.x * 4f) >= 68 && Mathf.FloorToInt(p.y * 4f) >= 68) ||
                     Mathf.FloorToInt(p.x * 4f) == 69 && Mathf.FloorToInt(p.y * 4f) == 69,
                (a, b) => true);
            var search = new StaticNavigationSearch(grid, bounds, Vector2Int.zero, new Vector2Int(69, 69));
            for (int i = 0; i < 16; i++)
            {
                Assert.That(search.Step(256), Is.EqualTo(256));
                if (i < 15) Assert.IsNull(search.Result);
            }
            Assert.That(search.Result.Status, Is.EqualTo(StaticNavigationPathStatus.BudgetExhausted));
            Assert.That(search.Result.ExpandedNodes, Is.EqualTo(4096));
            Assert.IsEmpty(search.Result.Cells);
            Assert.That(search.Step(256), Is.Zero);
        }

        [Test]
        public void TerminalContracts_ArePreservedWithLazyEndpointValidation()
        {
            var bounds = new BoundsInt(0, 0, 0, 3, 1, 1);
            var cache = new StaticNavigationWorldCache(new StaticNavigationLayout(authority, bounds),
                p => p.x > 0.25f && p.x < 0.5f ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear,
                (a, b) => StaticNavigationSampleState.Clear);
            var search = new StaticNavigationSearch(cache, bounds, Vector2Int.zero, new Vector2Int(2, 0));
            for (int i = 0; i < 20 && search.Result == null; i++) { search.Step(1); cache.SamplePending(128); }
            Assert.That(search.Result.Status, Is.EqualTo(StaticNavigationPathStatus.NoPath));
            var invalid = new StaticNavigationSearch(cache, bounds, Vector2Int.zero, Vector2Int.right);
            invalid.Step(1);
            Assert.That(invalid.Result.Status, Is.EqualTo(StaticNavigationPathStatus.InvalidGoal));
            var same = new StaticNavigationSearch(cache, bounds, Vector2Int.zero, Vector2Int.zero, 0);
            same.Step(0);
            Assert.That(same.Result.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
            Assert.That(same.Result.ExpandedNodes, Is.Zero);
        }
    }
}

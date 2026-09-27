using System;
using System.Collections.Generic;
using System.Linq;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationPathfinderTests
    {
        private UnityEngine.Grid worldGrid;
        [SetUp] public void SetUp() => worldGrid = new GameObject("WorldGrid", typeof(UnityEngine.Grid)).GetComponent<UnityEngine.Grid>();
        [TearDown] public void TearDown() => UnityEngine.Object.DestroyImmediate(worldGrid.gameObject);
        private readonly StaticNavigationPathfinder finder = new StaticNavigationPathfinder();

        [Test]
        public void StraightAndDiagonalPaths_HaveExpectedCostsAndOrderedEndpoints()
        {
            var grid = Grid(5, 5);
            var straight = finder.FindPath(grid, Vector2Int.zero, new Vector2Int(4, 0));
            Valid(grid, straight, Vector2Int.zero, new Vector2Int(4, 0));
            Assert.That(straight.Cost, Is.EqualTo(4d));
            var diagonal = finder.FindPath(grid, Vector2Int.zero, new Vector2Int(4, 4));
            Valid(grid, diagonal, Vector2Int.zero, new Vector2Int(4, 4));
            Assert.That(diagonal.Cost, Is.EqualTo(4d * Math.Sqrt(2d)).Within(1e-9));
        }

        [TestCase("box")]
        [TestCase("wall")]
        [TestCase("u")]
        public void StaticObstacles_FindValidDetourIncludingEscapeFromConcavity(string shape)
        {
            Func<int, int, bool> blocked = (x, y) => shape == "box"
                ? x >= 3 && x <= 5 && y >= 2 && y <= 4
                : shape == "wall" ? x == 4 && y < 6
                : (x == 2 || x == 6) && y >= 2 && y <= 5 || y == 2 && x >= 2 && x <= 6;
            var grid = Grid(9, 8, blocked);
            Vector2Int start = shape == "u" ? new Vector2Int(4, 3) : new Vector2Int(0, 3);
            Vector2Int goal = shape == "u" ? new Vector2Int(4, 0) : new Vector2Int(8, 3);
            var result = finder.FindPath(grid, start, goal);
            Valid(grid, result, start, goal);
            Assert.That(result.Cost, Is.GreaterThan(Vector2Int.Distance(start, goal)));
            if (shape == "u") Assert.IsTrue(result.Cells.Any(c => c.y >= 6));
        }

        [Test]
        public void DisconnectedGoal_ReturnsNoRouteAndNoPartialPath()
        {
            var result = finder.FindPath(Grid(5, 5, (x, y) => x == 2), new Vector2Int(0, 2), new Vector2Int(4, 2));
            Assert.That(result.Status, Is.EqualTo(StaticNavigationPathStatus.NoPath));
            Assert.IsEmpty(result.Cells);
            Assert.IsEmpty(result.WorldPoints);
        }

        [Test]
        public void EqualCostAlternatives_SelectSameOrderedRouteOnEverySearch()
        {
            var grid = Grid(7, 7, (x, y) => x == 3 && y == 3);
            var start = new Vector2Int(1, 3);
            var goal = new Vector2Int(5, 3);
            var first = finder.FindPath(grid, start, goal);
            Valid(grid, first, start, goal);
            for (int i = 0; i < 20; i++)
                CollectionAssert.AreEqual(first.Cells, finder.FindPath(grid, start, goal).Cells);
        }

        [Test]
        public void Budget_IsExactAndDistinctFromExhaustedFrontier()
        {
            var grid = Grid(5, 1);
            Assert.That(finder.FindPath(grid, Vector2Int.zero, new Vector2Int(4, 0), 0).Status,
                Is.EqualTo(StaticNavigationPathStatus.BudgetExhausted));
            var limited = finder.FindPath(grid, Vector2Int.zero, new Vector2Int(4, 0), 3);
            Assert.That(limited.Status, Is.EqualTo(StaticNavigationPathStatus.BudgetExhausted));
            Assert.That(limited.ExpandedNodes, Is.EqualTo(3));
            Assert.IsEmpty(limited.Cells);
            Assert.That(finder.FindPath(grid, Vector2Int.zero, new Vector2Int(4, 0), 4).Status,
                Is.EqualTo(StaticNavigationPathStatus.PathFound));
            var isolated = Grid(3, 1, (x, y) => x == 1);
            Assert.That(finder.FindPath(isolated, Vector2Int.zero, new Vector2Int(2, 0), 1).Status,
                Is.EqualTo(StaticNavigationPathStatus.NoPath));
        }

        [Test]
        public void Endpoints_AreValidatedAndIdenticalEndpointNeedsNoExpansion()
        {
            var grid = Grid(3, 3, (x, y) => x == 1 && y == 1);
            Assert.That(finder.FindPath(grid, new Vector2Int(-1, 0), Vector2Int.zero).Status, Is.EqualTo(StaticNavigationPathStatus.InvalidStart));
            Assert.That(finder.FindPath(grid, Vector2Int.one, Vector2Int.zero).Status, Is.EqualTo(StaticNavigationPathStatus.InvalidStart));
            Assert.That(finder.FindPath(grid, Vector2Int.zero, Vector2Int.one).Status, Is.EqualTo(StaticNavigationPathStatus.InvalidGoal));
            Assert.That(finder.FindPath(grid, Vector2Int.zero, new Vector2Int(3, 0)).Status, Is.EqualTo(StaticNavigationPathStatus.InvalidGoal));
            var same = finder.FindPath(grid, Vector2Int.zero, Vector2Int.zero, 0);
            Valid(grid, same, Vector2Int.zero, Vector2Int.zero);
            Assert.That(same.Cost, Is.Zero);
            Assert.That(same.ExpandedNodes, Is.Zero);
            Assert.Throws<ArgumentOutOfRangeException>(() => finder.FindPath(grid, Vector2Int.zero, Vector2Int.zero, -1));
        }

        [TestCase(2.2f, 4, StaticNavigationPathStatus.PathFound)]
        [TestCase(2.2f, 2, StaticNavigationPathStatus.NoPath)]
        [TestCase(1.5f, 4, StaticNavigationPathStatus.NoPath)]
        public void ColliderSampledPassage_UsesActualEnemyBodyClearance(float gap, int subdivisions, StaticNavigationPathStatus expected)
        {
            Vector2 origin = new Vector2(10000f, 10000f);
            var objects = new List<GameObject>();
            try
            {
                // Vertical wall spans the bounded map, with one central doorway.
                foreach (float sign in new[] { -1f, 1f })
                {
                    var item = new GameObject("Passage wall");
                    objects.Add(item);
                    item.layer = LayerMask.NameToLayer("Walls");
                    item.transform.position = origin + new Vector2(0f, sign * (gap * 0.5f + 3f));
                    var box = item.AddComponent<BoxCollider2D>();
                    box.size = new Vector2(1f, 6f);
                }
                Physics2D.SyncTransforms();
                var world = new StaticNavigationWorld2D(0.87684506f, 0.02f);
                worldGrid.transform.position = origin;
                var grid = new StaticNavigationGrid(worldGrid,
                    new BoundsInt(-4 * subdivisions, -4 * subdivisions, 0, 8 * subdivisions, 8 * subdivisions, 1),
                    world.IsPointClear, world.IsSegmentClear, subdivisions);
                var start = new Vector2Int(-3 * subdivisions, 0);
                var goal = new Vector2Int(3 * subdivisions, 0);
                var result = finder.FindPath(grid, start, goal);
                Assert.That(result.Status, Is.EqualTo(expected));
                if (expected == StaticNavigationPathStatus.PathFound)
                {
                    Valid(grid, result, start, goal);
                    for (int i = 1; i < result.WorldPoints.Count; i++)
                        Assert.IsTrue(world.IsSegmentClear(result.WorldPoints[i - 1], result.WorldPoints[i]));
                }
            }
            finally
            {
                foreach (var item in objects) UnityEngine.Object.DestroyImmediate(item);
                Physics2D.SyncTransforms();
            }
        }

        private StaticNavigationGrid Grid(int width, int height, Func<int, int, bool> blocked = null)
            => new StaticNavigationGrid(worldGrid, new BoundsInt(0, 0, 0, width, height, 1),
                p => blocked == null || !blocked(Mathf.FloorToInt(p.x * 4f), Mathf.FloorToInt(p.y * 4f)), (a, b) => true);

        private static void Valid(StaticNavigationGrid grid, StaticNavigationPathResult result, Vector2Int start, Vector2Int goal)
        {
            Assert.That(result.Status, Is.EqualTo(StaticNavigationPathStatus.PathFound));
            Assert.That(result.Cells[0], Is.EqualTo(start));
            Assert.That(result.Cells[result.Cells.Count - 1], Is.EqualTo(goal));
            Assert.That(result.Cells.Distinct().Count(), Is.EqualTo(result.Cells.Count));
            Assert.That(result.WorldPoints.Count, Is.EqualTo(result.Cells.Count));
            for (int i = 0; i < result.Cells.Count; i++)
            {
                Assert.IsTrue(grid.IsWalkable(result.Cells[i]));
                Assert.That(result.WorldPoints[i], Is.EqualTo(grid.CellToWorld(result.Cells[i])));
                if (i > 0) Assert.IsTrue(grid.CanTraverse(result.Cells[i - 1], result.Cells[i]));
            }
        }
    }
}

using System;
using System.Linq;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationGridTests
    {
        private Grid authority;
        [SetUp] public void SetUp() => authority = new GameObject("WorldGrid", typeof(Grid)).GetComponent<Grid>();
        [TearDown] public void TearDown() => UnityEngine.Object.DestroyImmediate(authority.gameObject);
        [TestCase(2)]
        [TestCase(4)]
        public void Subdivision_ReusesTranslatedRotatedWorldGridAndNegativeCoordinates(int subdivisions)
        {
            authority.transform.position = new Vector3(10f, -7f, 0f);
            authority.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
            var grid = new StaticNavigationGrid(authority, new BoundsInt(-8, -8, 0, 16, 16, 1),
                p => true, (a, b) => true, subdivisions);
            Assert.That(grid.CellSize, Is.EqualTo(authority.cellSize.x / subdivisions).Within(0.00001f));
            for (int y = -8; y < 8; y++)
                for (int x = -8; x < 8; x++)
                {
                    var cell = new Vector2Int(x, y);
                    Vector2 expected = authority.LocalToWorld(authority.CellToLocalInterpolated(
                        new Vector3((x + 0.5f) / subdivisions, (y + 0.5f) / subdivisions, 0f)));
                    Vector2 actual = grid.CellToWorld(cell);
                    Assert.That(Vector2.Distance(actual, expected), Is.LessThan(0.00001f));
                    Assert.IsTrue(grid.TryWorldToCell(actual, out var mapped));
                    Assert.That(mapped, Is.EqualTo(cell));
                    var parent = authority.WorldToCell(actual);
                    Assert.That(parent.x, Is.EqualTo(Mathf.FloorToInt((float)x / subdivisions)));
                    Assert.That(parent.y, Is.EqualTo(Mathf.FloorToInt((float)y / subdivisions)));
                }
            Vector2 average = Vector2.zero;
            for (int y = 0; y < subdivisions; y++)
                for (int x = 0; x < subdivisions; x++) average += grid.CellToWorld(new Vector2Int(x, y));
            Assert.That(Vector2.Distance(average / (subdivisions * subdivisions), authority.GetCellCenterWorld(Vector3Int.zero)),
                Is.LessThan(0.00001f));
        }

        [Test]
        public void WorldGridScale_IsReused_AndBuildingOccupancyIsIndependent()
        {
            authority.cellSize = new Vector3(2f, 2f, 0f);
            authority.transform.localScale = new Vector3(2f, 2f, 1f);
            var occupancy = new CastleOccupancyMap();
            occupancy.Occupy(Vector2.zero, GridFootprint.OneByOne);
            var grid = Open();
            Assert.That(grid.Subdivisions, Is.EqualTo(4));
            Assert.That(grid.CellSize, Is.EqualTo(1f));
            Assert.That(grid.CellToWorld(Vector2Int.zero), Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.IsTrue(grid.TryWorldToCell(new Vector2(1.5f, 0.5f), out var cell));
            Assert.That(cell, Is.EqualTo(Vector2Int.right));
            Assert.IsTrue(occupancy.IsAnyCellOccupied(Vector2.zero, GridFootprint.OneByOne));
            Assert.IsTrue(grid.IsWalkable(Vector2Int.zero));
            occupancy.Release(Vector2.zero, GridFootprint.OneByOne);
            var blocked = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 1, 1, 1), p => false, (a, b) => false);
            Assert.IsFalse(blocked.IsWalkable(Vector2Int.zero));
            Assert.IsFalse(occupancy.IsAnyCellOccupied(Vector2.zero, GridFootprint.OneByOne));
        }

        [Test]
        public void UnsupportedGeometry_IsRejectedRatherThanSilentlyMisaligned()
        {
            authority.cellGap = Vector3.right * 0.1f;
            Assert.Throws<ArgumentException>(() => Open());
        }

        [Test]
        public void Mapping_UsesCellCentersAndHalfOpenBounds()
        {
            var grid = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 4, 3, 1), p => true, (a, b) => true);
            Assert.That(grid.CellSize, Is.EqualTo(0.25f));
            Assert.That(grid.CellToWorld(Vector2Int.zero), Is.EqualTo(new Vector2(0.125f, 0.125f)));
            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                {
                    var expected = new Vector2Int(x, y);
                    Assert.IsTrue(grid.TryWorldToCell(grid.CellToWorld(expected), out var actual));
                    Assert.That(actual, Is.EqualTo(expected));
                }
            Assert.IsTrue(grid.TryWorldToCell(Vector2.zero, out _));
            Assert.IsFalse(grid.TryWorldToCell(new Vector2(1f, 0), out _));
            Assert.IsFalse(grid.TryWorldToCell(new Vector2(-0.001f, 0), out _));
            Assert.IsFalse(grid.TryWorldToCell(new Vector2(float.NaN, 0), out _));
            Assert.IsFalse(grid.IsWalkable(new Vector2Int(-1, 0)));
            Assert.IsEmpty(grid.GetNeighbors(new Vector2Int(-1, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => grid.CellToWorld(new Vector2Int(4, 0)));
        }

        [Test]
        public void OpenInterior_HasFourCardinalAndFourDiagonalNeighbors()
        {
            var grid = Open();
            var center = new Vector2Int(1, 1);
            var neighbors = grid.GetNeighbors(center).ToArray();
            Assert.That(neighbors.Length, Is.EqualTo(8));
            Assert.That(neighbors.Count(n => n.x == center.x || n.y == center.y), Is.EqualTo(4));
            Assert.That(grid.GetNeighbors(Vector2Int.zero).Count(), Is.EqualTo(3));
        }

        [Test]
        public void BlockedOrthogonalCell_PreventsDiagonalCornerCutting()
        {
            var grid = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 3, 3, 1),
                p => !(p.x > 0.25f && p.x < 0.5f && p.y < 0.25f), (a, b) => true);
            Assert.IsFalse(grid.IsWalkable(new Vector2Int(1, 0)));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.zero, Vector2Int.one));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.one, Vector2Int.zero));
        }

        [Test]
        public void ThinObstacleOnOrthogonalEdge_PreventsDiagonalDespiteClearCenters()
        {
            var grid = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 3, 3, 1), p => true,
                (a, b) => !(a == new Vector2(0.125f, 0.125f) && b == new Vector2(0.375f, 0.125f)));
            Assert.IsTrue(grid.IsWalkable(Vector2Int.one));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.zero, Vector2Int.right));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.right, Vector2Int.zero));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.zero, Vector2Int.one));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.one, Vector2Int.zero));
        }

        [Test]
        public void BlockedDiagonalSweep_CannotBeTraversed()
        {
            var grid = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 3, 3, 1), p => true,
                (a, b) => a.x == b.x || a.y == b.y);
            Assert.IsFalse(grid.CanTraverse(Vector2Int.zero, Vector2Int.one));
            Assert.IsTrue(grid.CanTraverse(Vector2Int.zero, Vector2Int.right));
            Assert.IsFalse(grid.CanTraverse(Vector2Int.zero, new Vector2Int(2, 0)));
        }

        [Test]
        public void Snapshot_DoesNotChangeUntilResampled()
        {
            bool clear = true;
            var grid = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 1, 1, 1), p => clear, (a, b) => clear);
            clear = false;
            Assert.IsTrue(grid.IsWalkable(Vector2Int.zero));
            var replacement = new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 1, 1, 1), p => clear, (a, b) => clear);
            Assert.IsFalse(replacement.IsWalkable(Vector2Int.zero));
        }

        [Test]
        public void InvalidBoundsAndSubdivision_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 0, 3, 1), p => true, (a, b) => true));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 3, 3, 1), p => true, (a, b) => true, 0));
        }

        private StaticNavigationGrid Open() => new StaticNavigationGrid(authority, new BoundsInt(0, 0, 0, 3, 3, 1), p => true, (a, b) => true);
    }
}

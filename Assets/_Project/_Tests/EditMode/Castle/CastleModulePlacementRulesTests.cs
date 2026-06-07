using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class CastleModulePlacementRulesTests
    {
        [Test]
        public void GridFootprint_EnumerateCells_ReturnsDeclaredCellAreaFromOrigin()
        {
            var footprint = new GridFootprint(3, 2);
            var cells = footprint.EnumerateCells(new Vector2Int(10, -2));

            Assert.That(cells, Is.EqualTo(new[]
            {
                new Vector2Int(10, -2),
                new Vector2Int(11, -2),
                new Vector2Int(12, -2),
                new Vector2Int(10, -1),
                new Vector2Int(11, -1),
                new Vector2Int(12, -1)
            }));
        }

        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(-1, 1)]
        public void GridFootprint_Throws_WhenDimensionsAreNotPositive(int width, int height)
        {
            Assert.That(() => new GridFootprint(width, height), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void CanPlaceAt_ReturnsFalse_WhenFootprintIsInvalid()
        {
            var occupancy = new CastleOccupancyMap();
            var canPlace = CastlePlacementRules.CanPlaceAt(new Vector2(0f, 0f), occupancy, default);

            Assert.IsFalse(canPlace);
        }

        [TestCase(0f, 0f, true)]
        [TestCase(3f, -6f, true)]
        [TestCase(1f, 0f, false)]
        [TestCase(3f, 2.9f, false)]
        public void IsOnLattice_ReturnsExpected(float x, float y, bool expected)
        {
            var result = CastlePlacementRules.IsOnLattice(new Vector2(x, y));
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void CanPlace3x3At_ReturnsTrue_WhenAreaIsFreeAndOnLattice()
        {
            var occupancy = new CastleOccupancyMap();
            var canPlace = CastlePlacementRules.CanPlace3x3At(new Vector2(0f, 0f), occupancy);

            Assert.IsTrue(canPlace);
        }

        [Test]
        public void CanPlaceAt_ReturnsTrue_WhenOneByOneFootprintAreaIsFreeAndOnLattice()
        {
            var occupancy = new CastleOccupancyMap();
            var canPlace = CastlePlacementRules.CanPlaceAt(
                new Vector2(0f, 0f),
                occupancy,
                GridFootprint.OneByOne);

            Assert.IsTrue(canPlace);
        }

        [Test]
        public void CanPlaceAt_ReturnsFalse_WhenOneByOneFootprintCellIsOccupied()
        {
            var occupancy = new CastleOccupancyMap();
            occupancy.Occupy(new Vector2(0f, 0f), GridFootprint.OneByOne);

            var canPlace = CastlePlacementRules.CanPlaceAt(
                new Vector2(0f, 0f),
                occupancy,
                GridFootprint.OneByOne);

            Assert.IsFalse(canPlace);
        }

        [Test]
        public void CanPlaceAt_ReturnsFalse_WhenDeclaredFootprintOverlapsOccupiedCell()
        {
            var occupancy = new CastleOccupancyMap();
            occupancy.Occupy(new Vector2(3f, 3f), GridFootprint.OneByOne);

            var canPlace = CastlePlacementRules.CanPlaceAt(
                new Vector2(0f, 0f),
                occupancy,
                GridFootprint.ThreeByThree);

            Assert.IsTrue(canPlace);

            canPlace = CastlePlacementRules.CanPlaceAt(
                new Vector2(3f, 3f),
                occupancy,
                new GridFootprint(6, 2));

            Assert.IsFalse(canPlace);
        }

        [Test]
        public void CanPlace3x3At_ReturnsFalse_WhenAnyCoveredCellIsOccupied()
        {
            var occupancy = new CastleOccupancyMap();
            occupancy.Occupy3x3(new Vector2(0f, 0f));

            var canPlace = CastlePlacementRules.CanPlace3x3At(new Vector2(0f, 0f), occupancy);
            Assert.IsFalse(canPlace);
        }

        [Test]
        public void CanPlace3x3At_ReturnsFalse_WhenOffLattice()
        {
            var occupancy = new CastleOccupancyMap();
            var canPlace = CastlePlacementRules.CanPlace3x3At(new Vector2(1f, 0f), occupancy);

            Assert.IsFalse(canPlace);
        }

        [Test]
        public void CanPlaceAt_ReturnsFalse_WhenOffLattice()
        {
            var occupancy = new CastleOccupancyMap();
            var canPlace = CastlePlacementRules.CanPlaceAt(
                new Vector2(1f, 0f),
                occupancy,
                GridFootprint.OneByOne);

            Assert.IsFalse(canPlace);
        }
    }
}

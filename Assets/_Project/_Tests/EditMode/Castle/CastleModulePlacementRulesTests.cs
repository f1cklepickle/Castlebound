using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class CastleModulePlacementRulesTests
    {
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
    }
}

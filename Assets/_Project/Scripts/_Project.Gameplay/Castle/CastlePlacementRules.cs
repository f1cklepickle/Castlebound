using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public static class CastlePlacementRules
    {
        private const float LatticeStep = 3f;
        private const float LatticeTolerance = 0.001f;

        public static bool IsOnLattice(Vector2 worldPosition)
        {
            return IsMultipleOfStep(worldPosition.x) && IsMultipleOfStep(worldPosition.y);
        }

        public static bool CanPlace3x3At(Vector2 worldPosition, CastleOccupancyMap occupancy)
        {
            if (occupancy == null || !IsOnLattice(worldPosition))
            {
                return false;
            }

            return !occupancy.IsAny3x3CellOccupied(worldPosition);
        }

        private static bool IsMultipleOfStep(float value)
        {
            var scaled = value / LatticeStep;
            return Mathf.Abs(scaled - Mathf.Round(scaled)) <= LatticeTolerance;
        }
    }
}

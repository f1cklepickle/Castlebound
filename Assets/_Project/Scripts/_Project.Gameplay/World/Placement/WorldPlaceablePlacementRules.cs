using Castlebound.Gameplay.Castle;
using UnityEngine;

namespace Castlebound.Gameplay.World.Placement
{
    public static class WorldPlaceablePlacementRules
    {
        public static Vector2 SnapToGrid(Vector2 worldPosition, float cellSize = 1f)
        {
            if (cellSize <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be positive.");
            }

            return new Vector2(
                Mathf.Round(worldPosition.x / cellSize) * cellSize,
                Mathf.Round(worldPosition.y / cellSize) * cellSize);
        }

        public static bool CanPlaceAt(
            PlaceableObjectDefinition definition,
            Vector2 snappedWorldPosition,
            PlaceablePlacementSurface availableSurface,
            CastleOccupancyMap occupancy)
        {
            if (definition == null || !definition.IsValid || occupancy == null)
            {
                return false;
            }

            if (definition.PlacementSurface != availableSurface)
            {
                return false;
            }

            return !occupancy.IsAnyCellOccupied(snappedWorldPosition, definition.Footprint);
        }
    }
}

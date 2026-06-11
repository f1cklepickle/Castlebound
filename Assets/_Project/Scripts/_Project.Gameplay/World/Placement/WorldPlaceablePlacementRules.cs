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

        public static PlaceablePlacementSurface ResolveAvailableSurface(
            Vector2 snappedWorldPosition,
            GridFootprint footprint,
            PlaceablePlacementSurface fallbackSurface,
            Collider2D castleRegion)
        {
            if (castleRegion == null || !footprint.IsValid)
            {
                return fallbackSurface;
            }

            foreach (var cell in EnumerateFootprintCells(snappedWorldPosition, footprint))
            {
                if (castleRegion.OverlapPoint(cell))
                {
                    return PlaceablePlacementSurface.CastleFloor;
                }
            }

            return PlaceablePlacementSurface.OutsideGround;
        }

        private static System.Collections.Generic.IEnumerable<Vector2> EnumerateFootprintCells(
            Vector2 snappedWorldPosition,
            GridFootprint footprint)
        {
            var origin = new Vector2Int(
                Mathf.RoundToInt(snappedWorldPosition.x),
                Mathf.RoundToInt(snappedWorldPosition.y));

            foreach (var cell in footprint.EnumerateCells(origin))
            {
                yield return new Vector2(cell.x, cell.y);
            }
        }
    }
}

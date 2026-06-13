using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class CastleOccupancyMap
    {
        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        public void Occupy(Vector2 worldPosition, GridFootprint footprint)
        {
            if (!footprint.IsValid)
            {
                throw new System.ArgumentException("Cannot occupy an invalid grid footprint.", nameof(footprint));
            }

            foreach (var cell in EnumerateCells(worldPosition, footprint))
            {
                occupiedCells.Add(cell);
            }
        }

        public void Occupy3x3(Vector2 worldPosition)
        {
            Occupy(worldPosition, GridFootprint.ThreeByThree);
        }

        public void Release(Vector2 worldPosition, GridFootprint footprint)
        {
            if (!footprint.IsValid)
            {
                throw new System.ArgumentException("Cannot release an invalid grid footprint.", nameof(footprint));
            }

            foreach (var cell in EnumerateCells(worldPosition, footprint))
            {
                occupiedCells.Remove(cell);
            }
        }

        public bool IsAnyCellOccupied(Vector2 worldPosition, GridFootprint footprint)
        {
            if (!footprint.IsValid)
            {
                throw new System.ArgumentException("Cannot query an invalid grid footprint.", nameof(footprint));
            }

            foreach (var cell in EnumerateCells(worldPosition, footprint))
            {
                if (occupiedCells.Contains(cell))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAny3x3CellOccupied(Vector2 worldPosition)
        {
            return IsAnyCellOccupied(worldPosition, GridFootprint.ThreeByThree);
        }

        private static IEnumerable<Vector2Int> EnumerateCells(Vector2 worldPosition, GridFootprint footprint)
        {
            var origin = new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
            return footprint.EnumerateCells(origin);
        }
    }
}

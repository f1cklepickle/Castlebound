using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class CastleOccupancyMap
    {
        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        public void Occupy3x3(Vector2 worldPosition)
        {
            foreach (var cell in Enumerate3x3Cells(worldPosition))
            {
                occupiedCells.Add(cell);
            }
        }

        public bool IsAny3x3CellOccupied(Vector2 worldPosition)
        {
            foreach (var cell in Enumerate3x3Cells(worldPosition))
            {
                if (occupiedCells.Contains(cell))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<Vector2Int> Enumerate3x3Cells(Vector2 worldPosition)
        {
            var origin = new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));

            for (var y = 0; y < 3; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    yield return new Vector2Int(origin.x + x, origin.y + y);
                }
            }
        }
    }
}

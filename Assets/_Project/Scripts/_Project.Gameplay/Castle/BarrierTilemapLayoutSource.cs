using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierTilemapLayoutSource
    {
        private readonly Tilemap barrierTilemap;

        public BarrierTilemapLayoutSource(Tilemap tilemap)
        {
            barrierTilemap = tilemap;
        }

        public IEnumerable<BarrierPlacementSlot> GetSlots()
        {
            if (barrierTilemap == null)
            {
                yield break;
            }

            foreach (var pos in barrierTilemap.cellBounds.allPositionsWithin)
            {
                var tile = barrierTilemap.GetTile(pos);
                if (tile == null)
                {
                    continue;
                }

                if (!BarrierTileSideMapper.TryMapTileNameToSide(tile.name, out var side))
                {
                    continue;
                }

                var world = barrierTilemap.GetCellCenterWorld(pos);

                yield return new BarrierPlacementSlot
                {
                    Id = $"barrier_{pos.x}_{pos.y}",
                    Position = world,
                    Side = side
                };
            }
        }
    }
}

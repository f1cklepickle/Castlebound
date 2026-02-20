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
                var snapped = new Vector2(
                    Mathf.Round(world.x / 3f) * 3f,
                    Mathf.Round(world.y / 3f) * 3f);

                yield return new BarrierPlacementSlot
                {
                    Id = $"barrier_{pos.x}_{pos.y}",
                    Position = snapped,
                    Side = side
                };
            }
        }
    }
}

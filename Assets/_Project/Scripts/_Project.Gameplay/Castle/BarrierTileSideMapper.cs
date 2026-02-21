using System;

namespace Castlebound.Gameplay.Castle
{
    public static class BarrierTileSideMapper
    {
        public static bool TryMapTileNameToSide(string tileName, out BarrierSide side)
        {
            side = default;
            if (string.IsNullOrWhiteSpace(tileName))
            {
                return false;
            }

            if (tileName.EndsWith("Top", StringComparison.OrdinalIgnoreCase))
            {
                side = BarrierSide.North;
                return true;
            }

            if (tileName.EndsWith("Bottom", StringComparison.OrdinalIgnoreCase))
            {
                side = BarrierSide.South;
                return true;
            }

            if (tileName.EndsWith("Left", StringComparison.OrdinalIgnoreCase))
            {
                side = BarrierSide.West;
                return true;
            }

            if (tileName.EndsWith("Right", StringComparison.OrdinalIgnoreCase))
            {
                side = BarrierSide.East;
                return true;
            }

            return false;
        }
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierPlacementSlot
    {
        public string Id { get; set; }
        public Vector2 Position { get; set; }
        public BarrierSide Side { get; set; }
    }
}

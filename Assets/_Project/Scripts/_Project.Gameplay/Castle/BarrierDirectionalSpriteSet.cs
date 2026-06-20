using System;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    [Serializable]
    public class BarrierDirectionalSpriteSet
    {
        [SerializeField] private Sprite north;
        [SerializeField] private Sprite east;
        [SerializeField] private Sprite south;
        [SerializeField] private Sprite west;

        public BarrierDirectionalSpriteSet()
        {
        }

        public BarrierDirectionalSpriteSet(Sprite north, Sprite east, Sprite south, Sprite west)
        {
            this.north = north;
            this.east = east;
            this.south = south;
            this.west = west;
        }

        public Sprite GetSprite(BarrierSide side)
        {
            return side switch
            {
                BarrierSide.North => north,
                BarrierSide.East => east,
                BarrierSide.South => south,
                BarrierSide.West => west,
                _ => null
            };
        }
    }
}

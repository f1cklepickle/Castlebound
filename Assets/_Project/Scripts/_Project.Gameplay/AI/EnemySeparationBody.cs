using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // One immutable starting pose and owned movement allowance per fixed-step solve.
    // External displacement is observed, never allocated to another actor or speed-clamped here.
    public struct EnemySeparationBody
    {
        public int Id;
        public Vector2 Position, Desired, External, Displacement;
        public float Radius, Budget;
        public bool Locked;
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public sealed class UnityTimeProvider : ITimeProvider
    {
        public float Time => UnityEngine.Time.time;
    }
}

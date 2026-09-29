using System;

namespace Castlebound.Gameplay.AI
{
    public struct EnemySeparationPair : IComparable<EnemySeparationPair>
    {
        public int First, Second, FirstId, SecondId;
        public int CompareTo(EnemySeparationPair other)
        {
            int order = FirstId.CompareTo(other.FirstId);
            return order != 0 ? order : SecondId.CompareTo(other.SecondId);
        }
    }
}

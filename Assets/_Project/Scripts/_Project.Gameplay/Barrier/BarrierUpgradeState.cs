namespace Castlebound.Gameplay.Barrier
{
    public class BarrierUpgradeState
    {
        public int Tier { get; private set; }

        public void IncrementTier()
        {
            Tier++;
        }
    }
}

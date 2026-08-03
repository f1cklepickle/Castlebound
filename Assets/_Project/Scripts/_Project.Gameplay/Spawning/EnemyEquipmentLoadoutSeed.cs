namespace Castlebound.Gameplay.Spawning
{
    public static class EnemyEquipmentLoadoutSeed
    {
        public static int Combine(int scheduleSeed, int waveIndex, int sequenceIndex)
        {
            unchecked
            {
                int seed = 17;
                seed = seed * 31 + scheduleSeed;
                seed = seed * 31 + waveIndex;
                seed = seed * 31 + sequenceIndex;
                return seed;
            }
        }
    }
}

namespace Castlebound.Gameplay.Stats
{
    public sealed class RunStats
    {
        public int WavesSurvived { get; private set; }
        public int EnemiesKilled { get; private set; }
        public int DamageDealt { get; private set; }
        public int DamageTaken { get; private set; }
        public int RepairsPerformed { get; private set; }
        public int HealthRestored { get; private set; }
        public int GoldEarned { get; private set; }
        public int GoldSpent { get; private set; }

        public void RecordWaveSurvived() => WavesSurvived++;
        public void RecordEnemyKilled() => EnemiesKilled++;
        public void RecordRepair() => RepairsPerformed++;
        public void RecordDamageDealt(int amount) => DamageDealt += Positive(amount);
        public void RecordDamageTaken(int amount) => DamageTaken += Positive(amount);
        public void RecordHealthRestored(int amount) => HealthRestored += Positive(amount);
        public void RecordGoldEarned(int amount) => GoldEarned += Positive(amount);
        public void RecordGoldSpent(int amount) => GoldSpent += Positive(amount);

        private static int Positive(int amount) => amount > 0 ? amount : 0;
    }
}

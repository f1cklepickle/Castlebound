using Castlebound.Gameplay.Stats;

namespace Castlebound.Gameplay.UI
{
    public static class RunSummaryFormatter
    {
        private const string ValueColor = "#78D68A";

        public static string Format(RunStats stats)
        {
            stats ??= new RunStats();
            return $"<b>RUN SUMMARY</b>\n\n" +
                   Line("Waves Survived", stats.WavesSurvived) +
                   Line("Enemies Defeated", stats.EnemiesKilled) +
                   Line("Damage Dealt", stats.DamageDealt) +
                   Line("Damage Taken", stats.DamageTaken) +
                   Line("Repairs Made", stats.RepairsPerformed) +
                   Line("Health Restored", stats.HealthRestored) +
                   Line("Gold Earned", stats.GoldEarned) +
                   Line("Gold Spent", stats.GoldSpent) +
                   "\nRise again. The castle still stands.";
        }

        private static string Line(string label, int value) =>
            $"{label}    <color={ValueColor}><b>{value}</b></color>\n";
    }
}

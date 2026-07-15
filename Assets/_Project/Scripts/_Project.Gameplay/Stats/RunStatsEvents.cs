using System;

namespace Castlebound.Gameplay.Stats
{
    public static class RunStatsEvents
    {
        public static event Action WaveSurvived;
        public static event Action EnemyKilled;
        public static event Action RepairPerformed;
        public static event Action<int> DamageDealt;
        public static event Action<int> HealthRestored;
        public static event Action<int> GoldEarned;
        public static event Action<int> GoldSpent;

        public static void RaiseWaveSurvived() => WaveSurvived?.Invoke();
        public static void RaiseEnemyKilled() => EnemyKilled?.Invoke();
        public static void RaiseRepairPerformed() => RepairPerformed?.Invoke();
        public static void RaiseDamageDealt(int amount) => DamageDealt?.Invoke(amount);
        public static void RaiseHealthRestored(int amount) => HealthRestored?.Invoke(amount);
        public static void RaiseGoldEarned(int amount) => GoldEarned?.Invoke(amount);
        public static void RaiseGoldSpent(int amount) => GoldSpent?.Invoke(amount);
    }
}

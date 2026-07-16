using UnityEngine;

namespace Castlebound.Gameplay.Stats
{
    public sealed class RunStatsTracker : MonoBehaviour
    {
        private bool isTracking;

        public RunStats Stats { get; } = new RunStats();

        private void OnEnable()
        {
            BeginTracking();
        }

        public void BeginTracking()
        {
            if (isTracking)
                return;

            RunStatsEvents.WaveSurvived += Stats.RecordWaveSurvived;
            RunStatsEvents.EnemyKilled += Stats.RecordEnemyKilled;
            RunStatsEvents.DamageDealt += Stats.RecordDamageDealt;
            RunStatsEvents.DamageTaken += Stats.RecordDamageTaken;
            RunStatsEvents.RepairPerformed += Stats.RecordRepair;
            RunStatsEvents.HealthRestored += Stats.RecordHealthRestored;
            RunStatsEvents.GoldEarned += Stats.RecordGoldEarned;
            RunStatsEvents.GoldSpent += Stats.RecordGoldSpent;
            isTracking = true;
        }

        private void OnDisable()
        {
            StopTracking();
        }

        public void StopTracking()
        {
            if (!isTracking)
                return;

            RunStatsEvents.WaveSurvived -= Stats.RecordWaveSurvived;
            RunStatsEvents.EnemyKilled -= Stats.RecordEnemyKilled;
            RunStatsEvents.DamageDealt -= Stats.RecordDamageDealt;
            RunStatsEvents.DamageTaken -= Stats.RecordDamageTaken;
            RunStatsEvents.RepairPerformed -= Stats.RecordRepair;
            RunStatsEvents.HealthRestored -= Stats.RecordHealthRestored;
            RunStatsEvents.GoldEarned -= Stats.RecordGoldEarned;
            RunStatsEvents.GoldSpent -= Stats.RecordGoldSpent;
            isTracking = false;
        }
    }
}

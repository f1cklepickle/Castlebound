using Castlebound.Gameplay.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Stats
{
    public class RunStatsTests
    {
        [Test]
        public void NewStats_StartAtZero()
        {
            var stats = new RunStats();

            Assert.That(stats.WavesSurvived, Is.Zero);
            Assert.That(stats.EnemiesKilled, Is.Zero);
            Assert.That(stats.DamageDealt, Is.Zero);
            Assert.That(stats.RepairsPerformed, Is.Zero);
            Assert.That(stats.HealthRestored, Is.Zero);
            Assert.That(stats.GoldEarned, Is.Zero);
            Assert.That(stats.GoldSpent, Is.Zero);
        }

        [Test]
        public void RecordMethods_AccumulateConfirmedOutcomes()
        {
            var stats = new RunStats();

            stats.RecordWaveSurvived();
            stats.RecordWaveSurvived();
            stats.RecordEnemyKilled();
            stats.RecordDamageDealt(4);
            stats.RecordDamageDealt(6);
            stats.RecordRepair();
            stats.RecordHealthRestored(3);
            stats.RecordGoldEarned(8);
            stats.RecordGoldSpent(5);

            Assert.That(stats.WavesSurvived, Is.EqualTo(2));
            Assert.That(stats.EnemiesKilled, Is.EqualTo(1));
            Assert.That(stats.DamageDealt, Is.EqualTo(10));
            Assert.That(stats.RepairsPerformed, Is.EqualTo(1));
            Assert.That(stats.HealthRestored, Is.EqualTo(3));
            Assert.That(stats.GoldEarned, Is.EqualTo(8));
            Assert.That(stats.GoldSpent, Is.EqualTo(5));
        }

        [Test]
        public void RecordAmounts_IgnoreNonPositiveValues()
        {
            var stats = new RunStats();

            stats.RecordDamageDealt(-2);
            stats.RecordHealthRestored(0);
            stats.RecordGoldEarned(-1);
            stats.RecordGoldSpent(0);

            Assert.That(stats.DamageDealt, Is.Zero);
            Assert.That(stats.HealthRestored, Is.Zero);
            Assert.That(stats.GoldEarned, Is.Zero);
            Assert.That(stats.GoldSpent, Is.Zero);
        }

        [Test]
        public void StartingCurrency_IsNotReportedAsGoldEarned()
        {
            int earned = 0;
            RunStatsEvents.GoldEarned += RecordEarned;
            try
            {
                var inventory = new Castlebound.Gameplay.Inventory.InventoryState(25);
                Assert.That(inventory.Gold, Is.EqualTo(25));
                Assert.That(earned, Is.Zero);
            }
            finally
            {
                RunStatsEvents.GoldEarned -= RecordEarned;
            }

            void RecordEarned(int amount) => earned += amount;
        }

        [Test]
        public void Tracker_CollectsRuntimeOutcomeEvents()
        {
            var gameObject = new GameObject("RunStatsTracker");
            RunStatsTracker tracker = null;
            try
            {
                tracker = gameObject.AddComponent<RunStatsTracker>();
                tracker.BeginTracking();

                RunStatsEvents.RaiseWaveSurvived();
                RunStatsEvents.RaiseEnemyKilled();
                RunStatsEvents.RaiseDamageDealt(7);
                RunStatsEvents.RaiseRepairPerformed();
                RunStatsEvents.RaiseHealthRestored(2);
                RunStatsEvents.RaiseGoldEarned(9);
                RunStatsEvents.RaiseGoldSpent(4);

                Assert.That(tracker.Stats.WavesSurvived, Is.EqualTo(1));
                Assert.That(tracker.Stats.EnemiesKilled, Is.EqualTo(1));
                Assert.That(tracker.Stats.DamageDealt, Is.EqualTo(7));
                Assert.That(tracker.Stats.RepairsPerformed, Is.EqualTo(1));
                Assert.That(tracker.Stats.HealthRestored, Is.EqualTo(2));
                Assert.That(tracker.Stats.GoldEarned, Is.EqualTo(9));
                Assert.That(tracker.Stats.GoldSpent, Is.EqualTo(4));
            }
            finally
            {
                tracker?.StopTracking();
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}

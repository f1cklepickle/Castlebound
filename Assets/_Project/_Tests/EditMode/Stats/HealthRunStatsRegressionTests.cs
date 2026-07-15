using Castlebound.Gameplay.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Stats
{
    public class HealthRunStatsRegressionTests
    {
        private GameObject player;
        private int restored;

        [TearDown]
        public void TearDown()
        {
            RunStatsEvents.HealthRestored -= RecordHealthRestored;
            if (player != null)
                Object.DestroyImmediate(player);
        }

        [Test]
        public void Heal_Overheal_RaisesOnlyActualHealthRestored()
        {
            RunStatsEvents.HealthRestored += RecordHealthRestored;
            player = new GameObject("Player") { tag = "Player" };
            var health = player.AddComponent<Health>();
            health.ConfigureMaxHealth(10, true);
            health.TakeDamage(3);

            health.Heal(10);

            Assert.That(restored, Is.EqualTo(3));
        }

        private void RecordHealthRestored(int amount)
        {
            restored += amount;
        }
    }
}

using Castlebound.Gameplay.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Stats
{
    public class DamageTakenRunStatsRegressionTests
    {
        private GameObject player;
        private int damageTaken;

        [TearDown]
        public void TearDown()
        {
            RunStatsEvents.DamageTaken -= RecordDamageTaken;
            if (player != null)
                Object.DestroyImmediate(player);
        }

        [Test]
        public void LethalDamage_RaisesOnlyActualHealthLost()
        {
            RunStatsEvents.DamageTaken += RecordDamageTaken;
            player = new GameObject("Player") { tag = "Player" };
            var health = player.AddComponent<Health>();
            health.ConfigureMaxHealth(10, true);

            health.TakeDamage(50);

            Assert.That(damageTaken, Is.EqualTo(10));
        }

        private void RecordDamageTaken(int amount) => damageTaken += amount;
    }
}

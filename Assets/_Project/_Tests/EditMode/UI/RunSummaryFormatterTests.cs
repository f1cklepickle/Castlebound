using Castlebound.Gameplay.Stats;
using Castlebound.Gameplay.UI;
using NUnit.Framework;

namespace Castlebound.Tests.UI
{
    public class RunSummaryFormatterTests
    {
        [Test]
        public void Format_MapsEveryRunStatToSummary()
        {
            var stats = new RunStats();
            stats.RecordWaveSurvived();
            stats.RecordEnemyKilled();
            stats.RecordDamageDealt(2);
            stats.RecordDamageTaken(3);
            stats.RecordRepair();
            stats.RecordHealthRestored(4);
            stats.RecordGoldEarned(5);
            stats.RecordGoldSpent(6);

            string summary = RunSummaryFormatter.Format(stats);

            StringAssert.Contains("Waves Survived    <color=#78D68A><b>1</b></color>", summary);
            StringAssert.Contains("Enemies Defeated  <color=#78D68A><b>1</b></color>", summary);
            StringAssert.Contains("Damage Dealt    <color=#78D68A><b>2</b></color>", summary);
            StringAssert.Contains("Damage Taken    <color=#78D68A><b>3</b></color>", summary);
            StringAssert.Contains("Repairs Made    <color=#78D68A><b>1</b></color>", summary);
            StringAssert.Contains("Health Restored    <color=#78D68A><b>4</b></color>", summary);
            StringAssert.Contains("Gold Earned    <color=#78D68A><b>5</b></color>", summary);
            StringAssert.Contains("Gold Spent    <color=#78D68A><b>6</b></color>", summary);
        }
    }
}

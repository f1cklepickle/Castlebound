using Castlebound.Gameplay.AI;
using NUnit.Framework;

namespace Castlebound.Tests.AI
{
    public class EnemySurroundEligibilityTests
    {
        [Test]
        public void ActiveLivingPlayerTargetingMeleeEnemy_IsEligible()
        {
            Assert.IsTrue(EnemySurroundEligibility.Evaluate(
                isActiveAndEnabled: true,
                isControllerEnabled: true,
                isAlive: true,
                isRooted: false,
                targetType: EnemyTargetType.Player,
                targetsPlayer: true));
        }

        [TestCase(false, true, true, false, EnemyTargetType.Player, true)]
        [TestCase(true, false, true, false, EnemyTargetType.Player, true)]
        [TestCase(true, true, false, false, EnemyTargetType.Player, true)]
        [TestCase(true, true, true, true, EnemyTargetType.Player, true)]
        [TestCase(true, true, true, false, EnemyTargetType.Barrier, false)]
        [TestCase(true, true, true, false, EnemyTargetType.Player, false)]
        public void IneligibleState_IsExcluded(
            bool isActiveAndEnabled,
            bool isControllerEnabled,
            bool isAlive,
            bool isRooted,
            EnemyTargetType targetType,
            bool targetsPlayer)
        {
            Assert.IsFalse(EnemySurroundEligibility.Evaluate(
                isActiveAndEnabled,
                isControllerEnabled,
                isAlive,
                isRooted,
                targetType,
                targetsPlayer));
        }
    }
}

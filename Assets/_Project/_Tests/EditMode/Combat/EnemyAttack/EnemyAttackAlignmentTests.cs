using NUnit.Framework;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackAlignmentTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void SharedEligibility_RequiresAlignmentForEveryTargetType(bool targetsBarrier)
        {
            Assert.IsFalse(EnemyAttack.IsAttackEligible(
                isInHoldRange: true,
                isInReach: true,
                isAligned: false),
                targetsBarrier
                    ? "Barrier attacks must respect the facing cone."
                    : "Player attacks must respect the facing cone.");
        }

        [Test]
        public void SharedEligibility_AllowsAlignedInRangeAttack()
        {
            Assert.IsTrue(EnemyAttack.IsAttackEligible(
                isInHoldRange: true,
                isInReach: true,
                isAligned: true));
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTargetLockTests
    {
        [Test]
        public void LockedTarget_RemainsValid_WhenSameSelectedTargetIsInReach()
        {
            var target = new GameObject("Target");

            try
            {
                Assert.IsTrue(EnemyAttack.IsLockedTargetValid(
                    target.transform,
                    target.transform,
                    isInReach: true));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void LockedTarget_IsInvalid_WhenSelectionChanges()
        {
            var lockedTarget = new GameObject("LockedTarget");
            var replacementTarget = new GameObject("ReplacementTarget");

            try
            {
                Assert.IsFalse(EnemyAttack.IsLockedTargetValid(
                    lockedTarget.transform,
                    replacementTarget.transform,
                    isInReach: true));
            }
            finally
            {
                Object.DestroyImmediate(replacementTarget);
                Object.DestroyImmediate(lockedTarget);
            }
        }

        [Test]
        public void LockedTarget_IsInvalid_WhenItLeavesReach()
        {
            var target = new GameObject("Target");

            try
            {
                Assert.IsFalse(EnemyAttack.IsLockedTargetValid(
                    target.transform,
                    target.transform,
                    isInReach: false));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void CancelledWindup_DoesNotRequireCompletedAttackCooldown()
        {
            Assert.IsFalse(EnemyAttack.RequiresCompletedCooldown(attackCompleted: false));
            Assert.IsTrue(EnemyAttack.RequiresCompletedCooldown(attackCompleted: true));
        }
    }
}

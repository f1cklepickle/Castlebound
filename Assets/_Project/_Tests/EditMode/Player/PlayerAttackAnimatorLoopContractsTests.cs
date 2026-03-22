using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackAnimatorLoopContractsTests
    {
        private const string PlayerAnimatorControllerPath =
            "Assets/_Project/Art/Knight_Assets/Player.controller";

        [Test]
        public void PlayerAnimator_DefinesAttackLoopActiveBoolParameter()
        {
            var source = File.ReadAllText(PlayerAnimatorControllerPath);

            StringAssert.Contains("- m_Name: AttackLoopActive", source,
                "Player animator should define AttackLoopActive bool for loop-era attack presentation.");
        }

        [Test]
        public void PlayerAnimator_DoesNotRequireAttackTriggerTransition()
        {
            var source = File.ReadAllText(PlayerAnimatorControllerPath);

            var containsAttackTriggerCondition =
                source.Contains("m_ConditionEvent: Attack\n") ||
                source.Contains("m_ConditionEvent: Attack\r\n");
            Assert.IsFalse(containsAttackTriggerCondition,
                "Loop-era attack presentation should not depend on trigger-driven Idle->Attack transitions.");
        }

        [Test]
        public void PlayerAnimator_ExitsAttackWithoutExitTimeGate()
        {
            var source = File.ReadAllText(PlayerAnimatorControllerPath);

            StringAssert.DoesNotContain("m_HasExitTime: 1", source,
                "Loop-era attack transitions should not be constrained by exit-time gates.");
        }

        [Test]
        public void AttackClip_DoesNotOwnHitboxEvents()
        {
            var source = File.ReadAllText("Assets/_Project/Art/Knight_Assets/Knight_Attack.anim");

            StringAssert.DoesNotContain("functionName: EnableHitbox", source,
                "Hitbox window should be owned by PlayerAttackLoop, not animation events.");
            StringAssert.DoesNotContain("functionName: DisableHitbox", source,
                "Hitbox window should be owned by PlayerAttackLoop, not animation events.");
        }
    }
}

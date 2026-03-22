using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackSingleClockContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string AttackLoopPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackLoop.cs";
        private const string AttackAnimationDriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackAnimationDriver.cs";

        [Test]
        public void PlayerController_DoesNotUseAnimatorTrigger_AsCadenceAuthority()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.DoesNotContain("SetTrigger(\"Attack\")", source,
                "Cadence should be owned by PlayerAttackLoop single clock, not Animator trigger pulses.");
        }

        [Test]
        public void AttackAnimationDriver_ExposesLoopDrivenPresentationContract()
        {
            var source = File.ReadAllText(AttackAnimationDriverPath);

            StringAssert.Contains("ApplyLoopPresentation", source,
                "Animation driver should expose a loop-driven presentation method.");
            StringAssert.Contains("normalizedSwingProgress", source,
                "Presentation contract should accept normalized loop timing for deterministic visual sync.");
        }

        [Test]
        public void AttackAnimationDriver_DoesNotEmitPerSwingAttackTrigger()
        {
            var source = File.ReadAllText(AttackAnimationDriverPath);

            StringAssert.DoesNotContain("SetTrigger(", source,
                "Looped high-rate presentation should not emit per-swing attack triggers.");
        }

        [Test]
        public void AttackLoop_ExposesNormalizedSwingProgress()
        {
            var source = File.ReadAllText(AttackLoopPath);

            StringAssert.Contains("NormalizedSwingProgress", source,
                "Single-clock loop should expose normalized swing progress for presentation followers.");
        }

        [Test]
        public void PlayerController_DelegatesLoopPresentation_ToAnimationDriver()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("attackAnimationDriver.ApplyLoopPresentation", source,
                "PlayerController should delegate loop presentation to the animation driver.");
        }
    }
}

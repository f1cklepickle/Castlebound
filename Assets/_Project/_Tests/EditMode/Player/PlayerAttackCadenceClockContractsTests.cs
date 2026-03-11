using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackCadenceClockContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";

        [Test]
        public void PlayerController_UsesFixedStepClock_ForCooldownAuthority()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("attackCooldownGate.TryConsume(Time.fixedTime", source,
                "Cooldown authority should use fixed-step time when attacks are processed from FixedUpdate.");
        }

        [Test]
        public void PlayerController_DoesNotUseFrameClock_ForCooldownAuthority()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.DoesNotContain("attackCooldownGate.TryConsume(Time.time", source,
                "Using frame clock in FixedUpdate attack authority can drop high-rate attempts under frame hitching.");
        }
    }
}

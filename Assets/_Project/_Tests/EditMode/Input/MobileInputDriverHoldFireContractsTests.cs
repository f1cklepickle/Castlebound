using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class MobileInputDriverHoldFireContractsTests
    {
        private const string MobileInputDriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/MobileInputDriver.cs";

        [Test]
        public void MobileInputDriver_DoesNotUsePulseTimer_ForRightTriggerWhileFiring()
        {
            var source = File.ReadAllText(MobileInputDriverPath);

            StringAssert.DoesNotContain("_pendingFirePulse", source,
                "Android right-trigger hold should not depend on one-frame fire pulses.");
            StringAssert.DoesNotContain("_attackTimer", source,
                "Android right-trigger hold should not be cadence-gated by MobileInputDriver timers.");
            StringAssert.Contains("state.rightTrigger = aimAttackZone.IsFiring ? 1f : 0f;", source,
                "Android right-trigger should remain pressed while aim zone is firing.");
        }
    }
}

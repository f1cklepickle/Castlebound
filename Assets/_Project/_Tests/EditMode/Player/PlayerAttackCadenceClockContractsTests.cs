using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackCadenceClockContractsTests
    {
        private const string PlayerAttackLoopPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackLoop.cs";

        [Test]
        public void PlayerAttackLoop_UsesSharedDeterministicClock_ForCadenceAuthority()
        {
            var source = File.ReadAllText(PlayerAttackLoopPath);

            StringAssert.Contains("AttackClock", source,
                "Player cadence should advance through the shared deterministic clock.");
            StringAssert.Contains("step.UnusedDeltaTime", source,
                "Player cadence should carry overshoot into chained swings.");
        }

        [Test]
        public void PlayerAttackLoop_DoesNotDependOnControllerTimeSources_ForCooldownAuthority()
        {
            var source = File.ReadAllText(PlayerAttackLoopPath);

            StringAssert.DoesNotContain("Time.fixedTime", source,
                "Loop-owned cadence should not depend on controller-owned fixed time.");
            StringAssert.DoesNotContain("Time.time", source,
                "Loop-owned cadence should not depend on frame time sources.");
        }
    }
}

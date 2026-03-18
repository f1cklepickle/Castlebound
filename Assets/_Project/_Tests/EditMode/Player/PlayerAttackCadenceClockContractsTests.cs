using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackCadenceClockContractsTests
    {
        private const string PlayerAttackLoopPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackLoop.cs";

        [Test]
        public void PlayerAttackLoop_UsesInternalElapsedClock_ForCooldownAuthority()
        {
            var source = File.ReadAllText(PlayerAttackLoopPath);

            StringAssert.Contains("elapsedTime += deltaTime", source,
                "Loop-owned cooldown authority should advance from the loop runtime clock.");
            StringAssert.Contains("attackCooldownGate.TryConsume(elapsedTime", source,
                "Cooldown authority should consume against the loop's internal elapsed clock.");
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

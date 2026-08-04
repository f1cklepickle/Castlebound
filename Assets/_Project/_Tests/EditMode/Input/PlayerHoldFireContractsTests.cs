using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class PlayerHoldFireContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string PlayerAttackRuntimePath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackRuntime.cs";

        [Test]
        public void PlayerController_UsesDedicatedFireInputController()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("PlayerFireInputController", source,
                "PlayerController should delegate hold-fire behavior to PlayerFireInputController.");
            StringAssert.DoesNotContain("AddComponent<PlayerFireInputController>", source,
                "PlayerController should not create fire input components at runtime.");
        }

        [Test]
        public void PlayerController_DelegatesPressedState_ToFireInputController_AndTicksAttackLoop()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            var forwardsPressedState =
                source.Contains("fireInputController.OnFirePressedStateChanged(value.isPressed)") ||
                source.Contains("fireInputController?.OnFirePressedStateChanged(value.isPressed)");
            Assert.IsTrue(forwardsPressedState,
                "PlayerController should forward pressed state updates to PlayerFireInputController.");

            var ticksAttackLoop = source.Contains("attackRuntime.Tick(");
            Assert.IsTrue(ticksAttackLoop,
                "PlayerController should tick the extracted attack runtime from FixedUpdate.");
        }

        [Test]
        public void PlayerController_ClearsHeldFire_WhenInputIsLocked()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            var clearsHeldFire =
                source.Contains("fireInputController.ClearHeldFire()") ||
                source.Contains("fireInputController?.ClearHeldFire()");
            Assert.IsTrue(clearsHeldFire,
                "PlayerController should clear held-fire intent when input is locked.");
        }

        [Test]
        public void PlayerController_KeepsAttackAuthorityDelegated_ToAttackLoop()
        {
            var source = File.ReadAllText(PlayerControllerPath);
            var runtimeSource = File.ReadAllText(PlayerAttackRuntimePath);

            StringAssert.Contains("attackRuntime.Tick(", source,
                "PlayerController should delegate held-fire cadence progression to the attack runtime.");
            StringAssert.Contains("attackLoop?.Tick(", runtimeSource,
                "The attack runtime should delegate deterministic cadence to PlayerAttackLoop.");
            StringAssert.DoesNotContain("attackCooldownGate.TryConsume", source,
                "PlayerController should no longer own direct cooldown consumption.");
        }
    }
}

using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class PlayerHoldFireContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";

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
        public void PlayerController_DelegatesPressedState_AndTick_ToFireInputController()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            var forwardsPressedState =
                source.Contains("fireInputController.OnFirePressedStateChanged(value.isPressed)") ||
                source.Contains("fireInputController?.OnFirePressedStateChanged(value.isPressed)");
            Assert.IsTrue(forwardsPressedState,
                "PlayerController should forward pressed state updates to PlayerFireInputController.");

            var ticksFireInput =
                source.Contains("fireInputController.Tick()") ||
                source.Contains("fireInputController?.Tick()");
            Assert.IsTrue(ticksFireInput,
                "PlayerController should tick PlayerFireInputController from FixedUpdate.");
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
        public void PlayerController_KeepsCooldownGateAsAttackAuthority()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("attackCooldownGate.TryConsume", source,
                "Attack cadence should remain authoritative through PlayerAttackCooldownGate.");
        }
    }
}

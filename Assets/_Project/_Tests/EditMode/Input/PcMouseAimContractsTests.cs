using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class PcMouseAimContractsTests
    {
        private const string InputActionsPath = "Assets/_Project/Settings/Input/PlayerControls.inputactions";
        private const string PlayerControllerPath = "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string AimResolverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAimInputResolver.cs";

        [Test]
        public void FireAction_HasMouseLeftButtonBinding()
        {
            var json = File.ReadAllText(InputActionsPath);

            StringAssert.Contains("\"action\": \"Fire\"", json,
                "PlayerControls should define a Fire action.");
            StringAssert.Contains("<Mouse>/leftButton", json,
                "Fire must be bound to mouse left button on PC.");
        }

        [Test]
        public void LookAction_HasMousePositionBinding()
        {
            var json = File.ReadAllText(InputActionsPath);

            StringAssert.Contains("\"action\": \"Look\"", json,
                "PlayerControls should define a Look action.");
            StringAssert.Contains("<Mouse>/position", json,
                "Look must be bound to mouse position so facing can follow cursor on PC.");
        }

        [Test]
        public void PlayerController_DelegatesAimResolution_ToDedicatedComponent()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("PlayerAimInputResolver", source,
                "PlayerController should delegate aim responsibility to PlayerAimInputResolver.");
            StringAssert.Contains("aimInputResolver.Resolve", source,
                "PlayerController should use PlayerAimInputResolver to compute facing input.");
        }

        [Test]
        public void AimResolver_UsesMouseScreenPositionToDriveFacing()
        {
            var source = File.ReadAllText(AimResolverPath);

            StringAssert.Contains("Mouse.current.position", source,
                "PlayerAimInputResolver should read mouse screen position for PC aiming.");
            StringAssert.Contains("ScreenToWorldPoint", source,
                "PlayerAimInputResolver should convert mouse screen position to world-space facing.");
        }

        [Test]
        public void AimResolver_PrioritizesStickLookInput_OverMouseAim()
        {
            var source = File.ReadAllText(AimResolverPath);

            StringAssert.Contains("stickAimInput.sqrMagnitude > stickDeadZone", source,
                "PlayerAimInputResolver should prefer stick/virtual-stick look vectors when present.");
            StringAssert.Contains("return stickAimInput;", source,
                "Stick/virtual-stick look should not be overridden by mouse aiming.");
        }
    }
}

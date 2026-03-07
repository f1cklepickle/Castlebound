using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class PlayerFacingPolicyContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string FacingPolicyResolverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerFacingPolicyResolver.cs";

        [Test]
        public void PlayerController_DelegatesFacingSourceSelection_ToFacingPolicyResolver()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("PlayerFacingPolicyResolver", source,
                "PlayerController should delegate facing-source policy to PlayerFacingPolicyResolver.");
            StringAssert.Contains("facingPolicyResolver.ResolveFacing", source,
                "PlayerController should request final facing from the policy resolver.");
        }

        [Test]
        public void FacingPolicyResolver_Exists_WithThresholdAndSmoothingTunables()
        {
            Assert.That(File.Exists(FacingPolicyResolverPath), Is.True,
                "Facing policy component should exist as a dedicated resolver.");

            var source = File.ReadAllText(FacingPolicyResolverPath);

            StringAssert.Contains("[SerializeField] private float aimEnterThreshold", source,
                "Aim-enter threshold should be tunable to avoid chatter.");
            StringAssert.Contains("[SerializeField] private float aimExitThreshold", source,
                "Aim-exit threshold should be tunable for hysteresis.");
            StringAssert.Contains("[SerializeField] private float movementMeaningfulThreshold", source,
                "Movement threshold should gate movement-facing behavior.");
            StringAssert.Contains("[SerializeField] private float returnToMovementFacingSpeed", source,
                "Return speed should be tunable for smooth transition back to movement-facing.");
        }

        [Test]
        public void FacingPolicyResolver_ExposesExplicitAimIntent_AndSmoothReturnContract()
        {
            Assert.That(File.Exists(FacingPolicyResolverPath), Is.True,
                "Facing policy component should exist before evaluating contract details.");

            var source = File.ReadAllText(FacingPolicyResolverPath);

            StringAssert.Contains("ResolveFacing(", source,
                "Facing policy resolver should expose a single facing resolution entrypoint.");
            StringAssert.Contains("aimIntentActive", source,
                "Facing policy should only apply aim override when explicit aim intent is active.");
            StringAssert.Contains("return resolvedAimInput.normalized", source,
                "When aim intent is active, policy should select aim as the facing source.");
            StringAssert.DoesNotContain("Mathf.MoveTowardsAngle", source,
                "Facing policy should not own angle interpolation; rotation smoothing belongs to movement orchestration.");
        }

        [Test]
        public void FacingPolicyResolver_PreservesFacing_WhenMovementIsNotMeaningful()
        {
            Assert.That(File.Exists(FacingPolicyResolverPath), Is.True,
                "Facing policy component should exist before evaluating fallback contract.");

            var source = File.ReadAllText(FacingPolicyResolverPath);

            StringAssert.Contains("movementMeaningfulThreshold * movementMeaningfulThreshold", source,
                "Movement-facing should only apply when movement input is meaningful.");
            StringAssert.Contains("return currentFacing", source,
                "When movement is not meaningful, facing should be preserved.");
        }
    }
}

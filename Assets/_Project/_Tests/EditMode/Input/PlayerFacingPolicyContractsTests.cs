using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Input
{
    public class PlayerFacingPolicyContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string FacingPolicyResolverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerFacingPolicyResolver.cs";
        private const string FacingOrchestratorPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerFacingOrchestrator.cs";
        private const string MovementOrchestratorPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerMovementOrchestrator.cs";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

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
        public void FacingPolicyResolver_Exists_WithThresholdAndGraceTunables()
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
            StringAssert.Contains("[SerializeField, Min(0f)] private float postAimFacingGraceDuration", source,
                "Post-aim facing grace should have a single inspector-safe policy owner.");
        }

        [Test]
        public void FacingPolicyResolver_ExposesSeparateAttackAndDefenseIntentContracts()
        {
            Assert.That(File.Exists(FacingPolicyResolverPath), Is.True,
                "Facing policy component should exist before evaluating contract details.");

            var source = File.ReadAllText(FacingPolicyResolverPath);

            StringAssert.Contains("ResolveFacing(", source,
                "Facing policy resolver should expose a single facing resolution entrypoint.");
            StringAssert.Contains("attackAimIntentActive", source,
                "Facing policy should only apply aim override when explicit aim intent is active.");
            StringAssert.Contains("defenseActive", source,
                "Defense must be a distinct higher-priority facing source.");
            StringAssert.Contains("return resolvedAimInput.normalized", source,
                "When aim intent is active, policy should select aim as the facing source.");
            StringAssert.DoesNotContain("transform.rotation", source,
                "Facing policy should select directions without executing presentation rotation.");
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

        [Test]
        public void MovementOrchestrator_OwnsVelocityOnly_AndFacingOrchestratorOwnsRotation()
        {
            var movementSource = File.ReadAllText(MovementOrchestratorPath);
            var facingSource = File.ReadAllText(FacingOrchestratorPath);

            StringAssert.Contains("mover.SetMoveInput", movementSource);
            StringAssert.DoesNotContain("playerTransform", movementSource);
            StringAssert.DoesNotContain("LastFacingDirection", movementSource);
            StringAssert.Contains("LastFacingDirection", facingSource);
            StringAssert.Contains("playerTransform.rotation", facingSource);
        }

        [Test]
        public void PlayerPrefab_AuthorsPointSixSecondGraceOnFacingPolicyResolver()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Assert.NotNull(prefab);

            var resolver = prefab.GetComponent<PlayerFacingPolicyResolver>();
            var playerController = prefab.GetComponent<PlayerController>();
            Assert.NotNull(resolver);
            Assert.NotNull(playerController);

            var serializedResolver = new SerializedObject(resolver);
            Assert.That(
                serializedResolver.FindProperty("postAimFacingGraceDuration").floatValue,
                Is.EqualTo(0.6f).Within(0.0001f));

            var serializedController = new SerializedObject(playerController);
            Assert.NotNull(serializedController.FindProperty("facingOrchestrator"));

            string controllerSource = File.ReadAllText(PlayerControllerPath);
            string movementSource = File.ReadAllText(MovementOrchestratorPath);
            string facingSource = File.ReadAllText(FacingOrchestratorPath);
            StringAssert.DoesNotContain("postAimFacingGraceDuration", controllerSource);
            StringAssert.DoesNotContain("postAimFacingGraceDuration", movementSource);
            StringAssert.DoesNotContain("postAimFacingGraceDuration", facingSource);
        }
    }
}

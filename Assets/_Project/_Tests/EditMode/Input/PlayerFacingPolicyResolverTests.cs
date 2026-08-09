using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Input
{
    public class PlayerFacingPolicyResolverTests
    {
        private GameObject root;
        private PlayerFacingPolicyResolver resolver;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PlayerFacingPolicyResolver");
            resolver = root.AddComponent<PlayerFacingPolicyResolver>();
            SetPrivateFloat("aimEnterThreshold", 0.25f);
            SetPrivateFloat("aimExitThreshold", 0.20f);
            SetPrivateFloat("movementMeaningfulThreshold", 0.10f);
            SetPrivateFloat("postAimFacingGraceDuration", 0.6f);
            SetPrivateFloat("stickMagnitudeMax", 1.1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void ResolveFacing_PcWithoutAimIntent_UsesMovementDirection()
        {
            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.right), 0.001f);
        }

        [Test]
        public void ResolveFacing_PcWithAimIntent_UsesAimDirection()
        {
            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: true,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.left), 0.001f);
        }

        [Test]
        public void ResolveFacing_AndroidRightStickAboveEnter_ActivatesAimOverride()
        {
            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.down,
                rawLookInput: new Vector2(0.30f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.down), 0.001f);
        }

        [Test]
        public void ResolveFacing_AndroidRightStickBetweenEnterAndExit_KeepsAimOverride()
        {
            resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.30f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.22f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.left), 0.001f);
        }

        [Test]
        public void ResolveFacing_AndroidRightStickBelowExit_RetainsFinalAimDirection()
        {
            resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.left,
                resolvedAimInput: Vector2.down,
                rawLookInput: new Vector2(0.30f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.left,
                resolvedAimInput: Vector2.down,
                rawLookInput: new Vector2(0.19f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.down), 0.001f);
        }

        [Test]
        public void ResolveFacing_AimEndsWithoutMeaningfulMovement_PreservesCurrentFacing()
        {
            var currentFacing = new Vector2(0.3f, 0.95f).normalized;
            var result = resolver.ResolveFacing(
                currentFacing: currentFacing,
                movementInput: new Vector2(0.05f, 0f),
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, currentFacing), 0.001f);
        }

        [Test]
        public void ResolveFacing_InvalidLookMagnitudeAboveMax_ReleasesIntoGracePeriod()
        {
            resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.30f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(1.2f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.left), 0.001f);
        }

        [Test]
        public void ResolveFacing_AimReleaseWhileWalking_RetainsFinalAimForGraceDuration()
        {
            resolver.ResolveFacing(Vector2.up, Vector2.right, Vector2.left, Vector2.zero, true, false, 0.02f);

            var duringGrace = resolver.ResolveFacing(
                Vector2.left, Vector2.right, Vector2.left, Vector2.zero, false, false, 0.3f);

            Assert.Less(Vector2.Distance(duringGrace, Vector2.left), 0.001f);
        }

        [Test]
        public void ResolveFacing_GraceExpires_AllowsActiveWalkingToReclaimFacing()
        {
            resolver.ResolveFacing(Vector2.up, Vector2.right, Vector2.left, Vector2.zero, true, false, 0.02f);
            resolver.ResolveFacing(Vector2.left, Vector2.right, Vector2.left, Vector2.zero, false, false, 0.3f);

            var afterGrace = resolver.ResolveFacing(
                Vector2.left, Vector2.right, Vector2.left, Vector2.zero, false, false, 0.3f);

            Assert.Less(Vector2.Distance(afterGrace, Vector2.right), 0.001f);
        }

        [Test]
        public void ResolveFacing_DefenseBeginsDuringGrace_UsesRetainedFallbackThenLiveDefenseAim()
        {
            resolver.ResolveFacing(Vector2.up, Vector2.right, Vector2.left, Vector2.zero, true, false, 0.02f);

            var defenseStarted = resolver.ResolveFacing(
                Vector2.left, Vector2.right, Vector2.zero, Vector2.zero, false, true, 0.1f);
            var defenseAimedAfterGrace = resolver.ResolveFacing(
                defenseStarted, Vector2.down, Vector2.down, Vector2.zero, false, true, 2f);

            Assert.Less(Vector2.Distance(defenseStarted, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(defenseAimedAfterGrace, Vector2.down), 0.001f);
        }

        [Test]
        public void ResolveFacing_DefenseEnds_AllowsWalkingToReclaimFacingImmediately()
        {
            resolver.ResolveFacing(Vector2.up, Vector2.right, Vector2.left, Vector2.zero, true, false, 0.02f);
            resolver.ResolveFacing(Vector2.left, Vector2.right, Vector2.left, Vector2.zero, false, true, 0.1f);

            var defenseReleased = resolver.ResolveFacing(
                Vector2.left, Vector2.down, Vector2.left, Vector2.zero, false, false, 0.02f);

            Assert.Less(Vector2.Distance(defenseReleased, Vector2.down), 0.001f);
        }

        [Test]
        public void ResolveFacing_ActiveDefenseAim_RemainsHigherPriorityThanWalking()
        {
            var defenseStarted = resolver.ResolveFacing(
                Vector2.left, Vector2.right, Vector2.up, Vector2.zero, false, true, 0.02f);

            var defenseHeld = resolver.ResolveFacing(
                defenseStarted, Vector2.right, Vector2.down, Vector2.zero, false, true, 0.02f);

            Assert.Less(Vector2.Distance(defenseStarted, Vector2.up), 0.001f);
            Assert.Less(Vector2.Distance(defenseHeld, Vector2.down), 0.001f);
        }

        private void SetPrivateFloat(string fieldName, float value)
        {
            var field = typeof(PlayerFacingPolicyResolver).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(resolver, value);
        }
    }
}

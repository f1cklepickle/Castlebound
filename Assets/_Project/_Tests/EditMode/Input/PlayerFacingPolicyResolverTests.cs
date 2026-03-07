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
                aimIntentActive: false,
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
                aimIntentActive: true,
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
                aimIntentActive: false,
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
                aimIntentActive: false,
                deltaTime: 0.02f);

            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.22f, 0f),
                aimIntentActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.left), 0.001f);
        }

        [Test]
        public void ResolveFacing_AndroidRightStickBelowExit_ReturnsToMovementDirection()
        {
            resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.left,
                resolvedAimInput: Vector2.down,
                rawLookInput: new Vector2(0.30f, 0f),
                aimIntentActive: false,
                deltaTime: 0.02f);

            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.left,
                resolvedAimInput: Vector2.down,
                rawLookInput: new Vector2(0.19f, 0f),
                aimIntentActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.left), 0.001f);
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
                aimIntentActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, currentFacing), 0.001f);
        }

        [Test]
        public void ResolveFacing_InvalidLookMagnitudeAboveMax_DisablesStickAimIntent()
        {
            resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.30f, 0f),
                aimIntentActive: false,
                deltaTime: 0.02f);

            var result = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(1.2f, 0f),
                aimIntentActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(result, Vector2.right), 0.001f);
        }

        private void SetPrivateFloat(string fieldName, float value)
        {
            var field = typeof(PlayerFacingPolicyResolver).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(resolver, value);
        }
    }
}

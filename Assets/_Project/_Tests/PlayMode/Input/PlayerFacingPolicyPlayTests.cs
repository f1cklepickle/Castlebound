using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Input
{
    public class PlayerFacingPolicyPlayTests
    {
        [UnityTest]
        public IEnumerator Runtime_Pc_MovementFacingToAimFacingAndBack_SwitchesCorrectly()
        {
            var root = new GameObject("PlayerFacingPolicyResolver");
            var resolver = root.AddComponent<PlayerFacingPolicyResolver>();
            ConfigureResolver(resolver);

            var movementFacing = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);
            yield return null;

            var aimFacing = resolver.ResolveFacing(
                currentFacing: movementFacing,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: true,
                defenseActive: false,
                deltaTime: 0.02f);
            yield return null;

            var releasedFacing = resolver.ResolveFacing(
                currentFacing: aimFacing,
                movementInput: Vector2.right,
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(movementFacing, Vector2.right), 0.001f);
            Assert.Less(Vector2.Distance(aimFacing, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(releasedFacing, Vector2.left), 0.001f);

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Runtime_AndroidRightStickHysteresis_ReleasesIntoGracePeriod()
        {
            var root = new GameObject("PlayerFacingPolicyResolver");
            var resolver = root.AddComponent<PlayerFacingPolicyResolver>();
            ConfigureResolver(resolver);

            var entered = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.down,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.30f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);
            yield return null;

            var between = resolver.ResolveFacing(
                currentFacing: entered,
                movementInput: Vector2.down,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.22f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);
            yield return null;

            var exited = resolver.ResolveFacing(
                currentFacing: between,
                movementInput: Vector2.down,
                resolvedAimInput: Vector2.left,
                rawLookInput: new Vector2(0.19f, 0f),
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(entered, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(between, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(exited, Vector2.left), 0.001f);

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Runtime_AimEndsWithoutMovement_PreservesFacing()
        {
            var root = new GameObject("PlayerFacingPolicyResolver");
            var resolver = root.AddComponent<PlayerFacingPolicyResolver>();
            ConfigureResolver(resolver);

            var aimedFacing = resolver.ResolveFacing(
                currentFacing: Vector2.up,
                movementInput: Vector2.zero,
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: true,
                defenseActive: false,
                deltaTime: 0.02f);
            yield return null;

            var releasedFacing = resolver.ResolveFacing(
                currentFacing: aimedFacing,
                movementInput: new Vector2(0.05f, 0f),
                resolvedAimInput: Vector2.left,
                rawLookInput: Vector2.zero,
                attackAimIntentActive: false,
                defenseActive: false,
                deltaTime: 0.02f);

            Assert.Less(Vector2.Distance(aimedFacing, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(releasedFacing, aimedFacing), 0.001f);

            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator Runtime_RetreatAimReleaseAndBlock_UsesFallbackThenLiveDefenseAim()
        {
            var root = new GameObject("PlayerFacingPolicyResolver");
            var resolver = root.AddComponent<PlayerFacingPolicyResolver>();
            ConfigureResolver(resolver);

            var aimedFacing = resolver.ResolveFacing(
                Vector2.right, Vector2.right, Vector2.left, Vector2.zero, true, false, 0.02f);
            yield return null;

            var graceFacing = resolver.ResolveFacing(
                aimedFacing, Vector2.right, Vector2.left, Vector2.zero, false, false, 0.02f);
            yield return null;

            var defenseFacing = resolver.ResolveFacing(
                graceFacing, Vector2.right, Vector2.zero, Vector2.zero, false, true, 0.02f);
            yield return null;

            var heldDefenseFacing = resolver.ResolveFacing(
                defenseFacing, Vector2.down, Vector2.down, Vector2.zero, false, true, 1.5f);

            Assert.Less(Vector2.Distance(aimedFacing, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(graceFacing, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(defenseFacing, Vector2.left), 0.001f);
            Assert.Less(Vector2.Distance(heldDefenseFacing, Vector2.down), 0.001f);

            Object.Destroy(root);
        }

        private static void ConfigureResolver(PlayerFacingPolicyResolver resolver)
        {
            SetPrivateFloat(resolver, "aimEnterThreshold", 0.25f);
            SetPrivateFloat(resolver, "aimExitThreshold", 0.20f);
            SetPrivateFloat(resolver, "movementMeaningfulThreshold", 0.10f);
            SetPrivateFloat(resolver, "postAimFacingGraceDuration", 0.6f);
            SetPrivateFloat(resolver, "stickMagnitudeMax", 1.1f);
        }

        private static void SetPrivateFloat(PlayerFacingPolicyResolver resolver, string fieldName, float value)
        {
            var field = typeof(PlayerFacingPolicyResolver).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(resolver, value);
        }
    }
}

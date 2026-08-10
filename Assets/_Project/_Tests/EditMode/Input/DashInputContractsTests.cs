using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Input
{
    public class DashInputContractsTests
    {
        private const string InputActionsPath =
            "Assets/_Project/Settings/Input/PlayerControls.inputactions";
        private const string GeneratedControlsPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/PlayerControls.cs";
        private const string MobileDriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/MobileInputDriver.cs";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        [Test]
        public void PlayerControls_DashUsesRequiredDesktopAndGamepadBindings()
        {
            string actions = File.ReadAllText(InputActionsPath);
            string generated = File.ReadAllText(GeneratedControlsPath);

            StringAssert.Contains("\"name\": \"Dash\"", actions);
            StringAssert.Contains("<Keyboard>/leftCtrl", actions);
            StringAssert.Contains("<Gamepad>/leftStickPress", actions);
            StringAssert.Contains("m_Player_Dash", generated);
            StringAssert.Contains("void OnDash(InputAction.CallbackContext context)", generated);
        }

        [Test]
        public void MobileDash_UsesReleaseSampleAndVirtualLeftStickPressWithoutDedicatedButton()
        {
            string source = File.ReadAllText(MobileDriverPath);

            StringAssert.Contains("movementZone.MovementReleased", source);
            StringAssert.Contains("TryResolveMobileRelease(releaseSample", source);
            StringAssert.Contains("GamepadButton.LeftStick", source);
            StringAssert.Contains("state.leftStick = _pendingDashDirection", source);
            StringAssert.DoesNotContain("dashbutton", source.ToLowerInvariant());
        }

        [Test]
        public void PlayerPrefab_OwnsDashTuningAndNoAuthoredDistance()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            Assert.NotNull(prefab);

            var dash = prefab.GetComponent<PlayerDashController>();
            Assert.NotNull(dash);
            var serializedDash = new SerializedObject(dash);

            Assert.That(serializedDash.FindProperty("dashSpeed"), Is.Not.Null);
            Assert.That(serializedDash.FindProperty("fullDashDuration"), Is.Not.Null);
            Assert.That(serializedDash.FindProperty("rearDurationMultiplier").floatValue,
                Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(serializedDash.FindProperty("cooldown"), Is.Not.Null);
            Assert.That(serializedDash.FindProperty("invulnerabilityDuration"), Is.Not.Null);
            Assert.That(serializedDash.FindProperty("mobileOuterReleaseThreshold"), Is.Not.Null);
            Assert.That(serializedDash.FindProperty("dashDistance"), Is.Null);
            Assert.That(serializedDash.FindProperty("rearDashDistance"), Is.Null);
        }
    }
}

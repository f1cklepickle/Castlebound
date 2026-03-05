using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Input;
using System.Reflection;

namespace Castlebound.Tests.Input
{
    /// <summary>
    /// Edit Mode tests for <see cref="MobileInputDriver"/>.
    ///
    /// What is covered here:
    ///   - The component can be instantiated and references wired without exceptions.
    ///   - Null-safety: missing optional references (zones, repairButton, playerInput)
    ///     do not throw during OnEnable / Update / OnDisable.
    ///
    /// What requires PlayMode tests (not covered here):
    ///   - InputSystem.AddDevice / RemoveDevice lifecycle (needs the Input System runtime).
    ///   - InputUser.PerformPairingWithDevice actually pairing the virtual gamepad with
    ///     the PlayerInput user (needs a real PlayerInput component in a live scene).
    ///   - Per-frame QueueStateEvent writing left/right stick values from the touch zones
    ///     and those values reaching PlayerController.OnMove / OnLook / OnFire.
    ///   - Attack pulse timer: that rightTrigger pulses at baseAttackRate while IsFiring,
    ///     that the first pulse is immediate (timer reset to 0 on pointer-up), and that
    ///     Mathf.Max clamps baseAttackRate so zero/negative values do not divide by zero.
    ///
    /// For full pipeline coverage create a PlayMode integration test that:
    ///   1. Loads the gameplay scene (or a minimal stand-in).
    ///   2. Confirms InputSystem.devices contains a Gamepad named "MobileGamepad".
    ///   3. Simulates a drag on TouchMovementZone and asserts PlayerController moved.
    ///   4. Simulates a right-zone drag past the deadzone and asserts the Attack animator
    ///      trigger fires repeatedly at approximately baseAttackRate.
    /// </summary>
    public class MobileInputDriverTests
    {
        private GameObject _go;
        private MobileInputDriver _driver;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("MobileInputDriver");
            // Do NOT call AddComponent here — individual tests control whether
            // OnEnable fires so they can configure references first.
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ── Structural sanity ─────────────────────────────────────────────────

        [Test]
        public void Component_CanBeAdded_WithoutException()
        {
            Assert.DoesNotThrow(() =>
            {
                _driver = _go.AddComponent<MobileInputDriver>();
            }, "Adding MobileInputDriver to a GameObject should not throw.");
        }

        // ── Null-safety: missing serialized references ─────────────────────────
        // The driver is designed to tolerate null zones and no playerInput
        // (graceful degradation on PC where MobileInputDriver is absent from the
        // scene, and to avoid hard failures if a reference is accidentally unset).

        [Test]
        public void OnEnable_WithNoReferences_DoesNotThrow()
        {
            // Instantiating via AddComponent triggers OnEnable immediately.
            // On-device, InputSystem.AddDevice will succeed; in Edit Mode the
            // Input System is also active, so this effectively tests the happy
            // path minus the PlayerInput pairing (playerInput field is null).
            Assert.DoesNotThrow(() =>
            {
                _driver = _go.AddComponent<MobileInputDriver>();
            }, "OnEnable with all references null should not throw.");
        }

        [Test]
        public void OnDisable_AfterOnEnable_DoesNotThrow()
        {
            _driver = _go.AddComponent<MobileInputDriver>();

            Assert.DoesNotThrow(() =>
            {
                _go.SetActive(false); // triggers OnDisable
            }, "OnDisable should clean up without throwing even when playerInput is null.");
        }

        [Test]
        public void RepeatEnableDisable_DoesNotThrow()
        {
            _driver = _go.AddComponent<MobileInputDriver>();

            Assert.DoesNotThrow(() =>
            {
                _go.SetActive(false);
                _go.SetActive(true);
                _go.SetActive(false);
            }, "Repeated enable/disable cycles should not throw.");
        }

        [Test]
        public void SetAttackRate_MethodExists_AsPublicApiContract()
        {
            var method = typeof(MobileInputDriver).GetMethod(
                "SetAttackRate",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(method,
                "MobileInputDriver must expose public SetAttackRate(float) so attack cadence can be wired from weapon stats.");
        }

        [Test]
        public void SetAttackRate_ClampsToPositiveMinimum()
        {
            _driver = _go.AddComponent<MobileInputDriver>();

            var method = typeof(MobileInputDriver).GetMethod(
                "SetAttackRate",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(method, "SetAttackRate(float) must exist.");

            method.Invoke(_driver, new object[] { 0f });

            var baseRateField = typeof(MobileInputDriver).GetField(
                "baseAttackRate",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(baseRateField, "baseAttackRate field was not found.");

            var storedRate = (float)baseRateField.GetValue(_driver);
            Assert.Greater(storedRate, 0f,
                "SetAttackRate(0) should clamp to a positive minimum to avoid divide-by-zero.");
        }
    }
}

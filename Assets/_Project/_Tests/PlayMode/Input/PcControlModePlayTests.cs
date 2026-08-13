using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Castlebound.Gameplay.Combat;

namespace Castlebound.Tests.Input
{
    public class PcControlModePlayTests
    {
        private GameObject player;
        private GameObject attacker;
        private Mouse mouse;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            mouse = InputSystem.AddDevice<Mouse>();
            player = new GameObject("Player");
            player.tag = "Player";
            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerCollisionMove2D>();
            var relativeFacing = player.AddComponent<PlayerRelativeMouseFacingController>();
            relativeFacing.Configure(1f);
            player.AddComponent<PcControlModeController>();
            player.AddComponent<PlayerFireInputController>();
            player.AddComponent<PlayerAttackLoop>();
            player.AddComponent<PlayerDashController>();
            player.AddComponent<Health>().ConfigureMaxHealth(10, true);
            player.AddComponent<PlayerDefenseController>().Configure(0.15f, 0.15f, 120f, 0.6f);
            player.AddComponent<PlayerController>();

            attacker = new GameObject("Attacker");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (mouse != null && mouse.added)
                InputSystem.RemoveDevice(mouse);
            Object.Destroy(player);
            Object.Destroy(attacker);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RelativeFacingAndWorldMovement_RemainIndependent()
        {
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var controller = player.GetComponent<PlayerController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 600f));
            SetField(controller, "movementInput", Vector2.up);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(Vector2.Distance(controller.CurrentFacingDirection, Vector2.right), Is.LessThan(0.001f));
            Assert.That(
                Quaternion.Angle(player.transform.rotation, Quaternion.Euler(0f, 0f, -90f)),
                Is.LessThan(0.001f),
                "PC facing presentation must not lag behind its logical combat direction.");
            Assert.That(player.transform.position.y, Is.GreaterThan(0.01f));
            Assert.That(player.transform.position.x, Is.EqualTo(0f).Within(0.001f),
                "World-up movement must not become facing-relative movement.");
        }

        [UnityTest]
        public IEnumerator WorldRelativeDash_UsesMovementDirectionAndExistingFacingClassification()
        {
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var controller = player.GetComponent<PlayerController>();
            var dash = player.GetComponent<PlayerDashController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 0f));
            yield return new WaitForFixedUpdate();

            AssertDash(Vector2.up, Vector2.up, PlayerDashDirection.Full);
            AssertDash(Vector2.down, Vector2.down, PlayerDashDirection.Rear);
            AssertDash(Vector2.left, Vector2.left, PlayerDashDirection.Rear);
            AssertDash(Vector2.right, Vector2.right, PlayerDashDirection.Full);
            AssertDash(new Vector2(-1f, -1f), new Vector2(-1f, -1f).normalized, PlayerDashDirection.Rear);
            AssertDash(Vector2.zero, Vector2.left, PlayerDashDirection.Rear);

            void AssertDash(
                Vector2 localInput,
                Vector2 expectedDirection,
                PlayerDashDirection expectedClass)
            {
                dash.ResetState();
                SetField(controller, "movementInput", localInput);
                Assert.IsTrue(controller.TryStartDash());
                Assert.That(Vector2.Distance(dash.Direction, expectedDirection), Is.LessThan(0.001f));
                Assert.That(dash.DirectionClass, Is.EqualTo(expectedClass));
            }
        }

        [UnityTest]
        public IEnumerator DefenseAndNeutralDash_ConsumeTheProductionFacingContract()
        {
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var controller = player.GetComponent<PlayerController>();
            var defense = player.GetComponent<PlayerDefenseController>();
            var dash = player.GetComponent<PlayerDashController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 0f));
            yield return new WaitForFixedUpdate();

            attacker.transform.position = Vector2.right;
            defense.SetDefensePressed(true);
            PlayerHitResult result = defense.ReceiveHit(new PlayerHitRequest(
                1,
                attacker,
                attacker.transform.position,
                CombatDamageType.Melee));

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            defense.SetDefensePressed(false);
            defense.Tick(1f);
            Assert.IsTrue(controller.TryStartDash());
            Assert.That(Vector2.Distance(dash.Direction, Vector2.left), Is.LessThan(0.001f),
                "Neutral backward dodge must continue using the established current-facing contract.");
        }

        [UnityTest]
        public IEnumerator InputLock_UnlocksCursorAndResumeRestoresGameplayCursor()
        {
            var controller = player.GetComponent<PlayerController>();
            var cursor = player.GetComponent<PcControlModeController>();
            cursor.RefreshCursorState();
            yield return null;

            Assert.That(cursor.RequestedLockMode, Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(cursor.RequestedCursorVisible);

            controller.SetInputLocked(true);
            Assert.That(cursor.RequestedLockMode, Is.EqualTo(CursorLockMode.None));
            Assert.IsTrue(cursor.RequestedCursorVisible);

            controller.SetInputLocked(false);
            Assert.That(cursor.RequestedLockMode, Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(cursor.RequestedCursorVisible);
        }

        [UnityTest]
        public IEnumerator PointerModeTransition_DiscardsQueuedDeltaAndPreservesFacing()
        {
            var controller = player.GetComponent<PlayerController>();
            var relativeFacing = player.GetComponent<PlayerRelativeMouseFacingController>();
            var mode = player.GetComponent<PcControlModeController>();
            relativeFacing.QueueMouseDelta(new Vector2(90f, 0f));

            mode.RequestPointerMode(player);
            mode.ReleasePointerMode(player);
            yield return new WaitForFixedUpdate();

            Assert.That(Vector2.Distance(controller.CurrentFacingDirection, Vector2.up), Is.LessThan(0.001f));
            Assert.That(mode.RequestedLockMode, Is.EqualTo(CursorLockMode.Locked));
            Assert.IsFalse(mode.RequestedCursorVisible);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(instance, value);
        }
    }
}

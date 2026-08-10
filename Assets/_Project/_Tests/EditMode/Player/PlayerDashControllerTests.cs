using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Player
{
    public class PlayerDashControllerTests
    {
        private GameObject player;
        private PlayerDashController dash;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("PlayerDashControllerTests");
            dash = player.AddComponent<PlayerDashController>();
            dash.Configure(
                speed: 20f,
                fullDuration: 0.5f,
                rearMultiplier: 0.6f,
                cooldownSeconds: 1f,
                invulnerabilitySeconds: 0.4f,
                mobileReleaseThreshold: 0.9f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void TryStart_NormalizesCardinalAndDiagonalDirections()
        {
            Assert.IsTrue(dash.TryStart(new Vector2(4f, 0f), Vector2.up, true));
            Assert.That(dash.Direction, Is.EqualTo(Vector2.right));
            Assert.That(dash.CurrentVelocity.magnitude, Is.EqualTo(20f).Within(0.0001f));

            dash.ResetState();
            Assert.IsTrue(dash.TryStart(new Vector2(3f, 3f), Vector2.up, true));
            Assert.That(dash.Direction.magnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(dash.CurrentVelocity.magnitude, Is.EqualTo(20f).Within(0.0001f));
        }

        [Test]
        public void ClassifyDirection_UsesBroadRearHemisphere()
        {
            Vector2 facing = Vector2.up;

            Assert.That(PlayerDashController.ClassifyDirection(Vector2.up, facing), Is.EqualTo(PlayerDashDirection.Full));
            Assert.That(PlayerDashController.ClassifyDirection(Vector2.right, facing), Is.EqualTo(PlayerDashDirection.Full));
            Assert.That(PlayerDashController.ClassifyDirection(new Vector2(1f, 1f), facing), Is.EqualTo(PlayerDashDirection.Full));
            Assert.That(PlayerDashController.ClassifyDirection(Vector2.down, facing), Is.EqualTo(PlayerDashDirection.Rear));
            Assert.That(PlayerDashController.ClassifyDirection(new Vector2(1f, -0.01f), facing), Is.EqualTo(PlayerDashDirection.Rear));
            Assert.That(PlayerDashController.ClassifyDirection(new Vector2(-1f, -1f), facing), Is.EqualTo(PlayerDashDirection.Rear));
        }

        [Test]
        public void TryStart_WithNeutralInput_DodgesStraightBackFromFacing()
        {
            Assert.IsTrue(dash.TryStart(Vector2.zero, Vector2.right, true));

            Assert.That(dash.Direction, Is.EqualTo(Vector2.left));
            Assert.That(dash.DirectionClass, Is.EqualTo(PlayerDashDirection.Rear));
            Assert.That(dash.ActiveDuration, Is.EqualTo(0.3f).Within(0.0001f));
        }

        [Test]
        public void ForwardSideAndRear_UseSameSpeed_WhileRearEndsSooner()
        {
            Vector2[] fullDirections =
            {
                Vector2.up,
                Vector2.left,
                Vector2.right,
                new Vector2(1f, 1f)
            };

            foreach (Vector2 direction in fullDirections)
            {
                dash.ResetState();
                Assert.IsTrue(dash.TryStart(direction, Vector2.up, true));
                Assert.That(dash.CurrentVelocity.magnitude, Is.EqualTo(20f).Within(0.0001f));
                Assert.That(dash.ActiveDuration, Is.EqualTo(0.5f).Within(0.0001f));
            }

            dash.ResetState();
            Assert.IsTrue(dash.TryStart(Vector2.down, Vector2.up, true));
            Assert.That(dash.CurrentVelocity.magnitude, Is.EqualTo(20f).Within(0.0001f));
            Assert.That(dash.ActiveDuration, Is.EqualTo(0.3f).Within(0.0001f));
        }

        [Test]
        public void Distance_IsDerivedFromSpeedAndSelectedDuration()
        {
            Assert.That(dash.FullDashDistance, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(dash.RearDashDuration, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(dash.RearDashDistance, Is.EqualTo(6f).Within(0.0001f));

            string[] fieldNames = typeof(PlayerDashController)
                .GetFields(System.Reflection.BindingFlags.Instance |
                           System.Reflection.BindingFlags.NonPublic |
                           System.Reflection.BindingFlags.Public)
                .Select(field => field.Name.ToLowerInvariant())
                .ToArray();

            Assert.That(fieldNames.Any(name => name.Contains("distance")), Is.False,
                "Dash distance must remain derived rather than independently authored.");
            Assert.That(fieldNames.Any(name => name.Contains("weight") || name.Contains("shove")), Is.False,
                "Issue #263 must not introduce enemy displacement configuration.");
        }

        [Test]
        public void DefaultRearMultiplier_IsPointSix()
        {
            var defaults = new GameObject("DefaultDash");
            try
            {
                var defaultDash = defaults.AddComponent<PlayerDashController>();
                Assert.That(defaultDash.RearDurationMultiplier, Is.EqualTo(0.6f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(defaults);
            }
        }

        [Test]
        public void Direction_IsSnapshotted_AndCannotBeSteered()
        {
            Assert.IsTrue(dash.TryStart(Vector2.right, Vector2.up, true));
            Vector2 snapshot = dash.Direction;

            dash.Tick(0.1f);

            Assert.That(dash.Direction, Is.EqualTo(snapshot));
            Assert.That(dash.CurrentVelocity, Is.EqualTo(Vector2.right * 20f));
        }

        [Test]
        public void Lifecycle_StartsCooldown_RejectsChaining_AndDoesNotBuffer()
        {
            Assert.IsTrue(dash.TryStart(Vector2.up, Vector2.up, true));
            Assert.IsTrue(dash.IsDashing);
            Assert.IsFalse(dash.IsReady);
            Assert.That(dash.CooldownRemaining, Is.EqualTo(1f).Within(0.0001f));

            Assert.IsFalse(dash.TryStart(Vector2.left, Vector2.up, true));
            dash.Tick(0.5f);
            Assert.IsFalse(dash.IsDashing);
            Assert.That(dash.Direction, Is.EqualTo(Vector2.zero));

            dash.Tick(0.49f);
            Assert.IsFalse(dash.IsReady);
            dash.Tick(0.01f);
            Assert.IsTrue(dash.IsReady);
            Assert.IsFalse(dash.IsDashing, "Rejected dash input must not be buffered.");
        }

        [Test]
        public void RejectedState_DoesNotStartDashCooldownOrInvulnerability()
        {
            Assert.IsFalse(dash.TryStart(Vector2.up, Vector2.up, false));

            Assert.IsFalse(dash.IsDashing);
            Assert.IsFalse(dash.IsInvulnerable);
            Assert.IsTrue(dash.IsReady);
        }

        [Test]
        public void InvulnerabilityTimer_IsIndependent_AndCanOutlastRearMovement()
        {
            Assert.IsTrue(dash.TryStart(Vector2.down, Vector2.up, true));

            dash.Tick(0.3f);

            Assert.IsFalse(dash.IsDashing);
            Assert.IsTrue(dash.IsInvulnerable);
            Assert.That(dash.InvulnerabilityRemaining, Is.EqualTo(0.1f).Within(0.0001f));

            dash.Tick(0.1f);
            Assert.IsFalse(dash.IsInvulnerable);
        }

        [Test]
        public void Health_RejectsDamageOnlyDuringConfiguredDashInvulnerability()
        {
            var health = player.AddComponent<Health>();
            health.ConfigureMaxHealth(10, refill: true);
            Assert.IsTrue(dash.TryStart(Vector2.down, Vector2.up, true));

            Assert.That(health.ApplyDamage(3), Is.Zero);
            Assert.That(health.Current, Is.EqualTo(10));

            dash.Tick(0.4f);
            Assert.That(health.ApplyDamage(3), Is.EqualTo(3));
            Assert.That(health.Current, Is.EqualTo(7));
        }

        [Test]
        public void MobileReleasePolicy_EvaluatesOnlyCurrentReleaseSample()
        {
            Assert.IsTrue(dash.TryResolveMobileRelease(new Vector2(0.9f, 0.9f), out Vector2 outer));
            Assert.That(outer.magnitude, Is.EqualTo(1f).Within(0.0001f));

            Assert.IsFalse(dash.TryResolveMobileRelease(new Vector2(0.89f, 0f), out Vector2 inward));
            Assert.That(inward, Is.EqualTo(Vector2.zero));

            Assert.IsFalse(dash.TryResolveMobileRelease(Vector2.zero, out _),
                "An earlier outer throw must not arm a later inward release.");
        }
    }
}

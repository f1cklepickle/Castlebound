using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Player
{
    public class PlayerDashCombatIntegrationTests
    {
        private GameObject player;
        private PlayerController controller;
        private PlayerDashController dash;
        private PlayerFireInputController fireInput;
        private PlayerAttackLoop attackLoop;
        private PlayerDefenseController defense;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Player");
            player.tag = "Player";
            player.AddComponent<Animator>();
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<CircleCollider2D>();
            player.AddComponent<PlayerCollisionMove2D>();
            dash = player.AddComponent<PlayerDashController>();
            dash.Configure(20f, 0.5f, 0.6f, 1f, 0.4f, 0.9f);
            fireInput = player.AddComponent<PlayerFireInputController>();
            attackLoop = player.AddComponent<PlayerAttackLoop>();
            defense = player.AddComponent<PlayerDefenseController>();
            defense.Configure(0.15f, 0.15f, 120f, 0.6f);
            controller = player.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void SuccessfulDash_CancelsActiveAttackAndClearsHeldFire()
        {
            fireInput.Configure(null, () => true);
            fireInput.OnFirePressedStateChanged(true);
            attackLoop.Tick(0.01f, 1f, true);
            Assert.IsTrue(attackLoop.IsSwingActive);

            Assert.IsTrue(controller.TryStartDash());

            Assert.IsFalse(fireInput.IsFireHeld);
            Assert.IsFalse(attackLoop.IsSwingActive);
        }

        [Test]
        public void Dash_IsRejectedDuringGuardParryAndRecovery()
        {
            defense.SetDefensePressed(true);
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.ParryWindow));
            Assert.IsFalse(controller.TryStartDash());

            defense.Tick(0.2f);
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Blocking));
            Assert.IsFalse(controller.TryStartDash());

            defense.SetDefensePressed(false);
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Recovery));
            Assert.IsFalse(controller.TryStartDash());
        }

        [Test]
        public void DefenseAndRepeatedDash_AreRejectedWhileDashing()
        {
            Assert.IsTrue(controller.TryStartDash());

            defense.SetDefensePressed(true);

            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Idle));
            Assert.IsFalse(controller.TryStartDash());
            Assert.IsTrue(dash.IsDashing);
        }

        [Test]
        public void StopMovement_ClearsActiveDashWithoutLeavingVelocityState()
        {
            Assert.IsTrue(controller.TryStartDash());
            Assert.IsTrue(dash.IsDashing);

            controller.StopMovement();

            Assert.IsFalse(dash.IsDashing);
            Assert.IsFalse(dash.IsInvulnerable);
            Assert.That(dash.CurrentVelocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Repair_IsRejectedWhileDashing()
        {
            var barrier = new GameObject("Barrier");
            try
            {
                barrier.layer = 0;
                barrier.transform.position = Vector2.right;
                barrier.AddComponent<BoxCollider2D>();
                var barrierHealth = barrier.AddComponent<BarrierHealth>();
                barrierHealth.MaxHealth = 10;
                barrierHealth.CurrentHealth = 6;
                controller.RepairRange = 2f;
                controller.RepairBarrierMask = 1 << 0;
                Physics2D.SyncTransforms();

                Assert.IsTrue(controller.TryStartDash());
                Assert.IsFalse(controller.TryRepair());
                Assert.That(barrierHealth.CurrentHealth, Is.EqualTo(6));
                Assert.That(controller.RepairCooldownRemaining, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(barrier);
            }
        }
    }
}

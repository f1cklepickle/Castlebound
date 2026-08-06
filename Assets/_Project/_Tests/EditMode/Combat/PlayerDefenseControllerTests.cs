using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerDefenseControllerTests
    {
        private GameObject player;
        private GameObject attacker;
        private Health health;
        private PlayerDefenseController defense;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            health = player.AddComponent<Health>();
            health.ConfigureMaxHealth(10, refill: true);
            defense = player.AddComponent<PlayerDefenseController>();
            defense.Configure(0.15f, 0.15f, 120f, 0.6f);

            attacker = new GameObject("Attacker");
            attacker.transform.position = Vector2.right;
        }

        [TearDown]
        public void TearDown()
        {
            if (attacker != null)
                Object.DestroyImmediate(attacker);
            if (player != null)
                Object.DestroyImmediate(player);
        }

        [Test]
        public void ReceiveHit_DuringFrontalParry_PreservesHealthAndPublishesResult()
        {
            PlayerHitResult observed = default;
            defense.HitResolved += result => observed = result;
            defense.SetDefensePressed(true);

            PlayerHitResult result = defense.ReceiveHit(new PlayerHitRequest(
                4,
                attacker,
                attacker.transform.position,
                CombatDamageType.Melee));

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(result.Attacker, Is.SameAs(attacker));
            Assert.That(observed.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(health.Current, Is.EqualTo(10));
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Blocking));
        }

        [Test]
        public void ReceiveHit_FirstReceivedParryConsumesDefaultCapacity_SecondHitBlocks()
        {
            defense.SetDefensePressed(true);

            PlayerHitResult first = defense.ReceiveHit(CreateMeleeHit(attacker));
            PlayerHitResult second = defense.ReceiveHit(CreateMeleeHit(attacker));

            Assert.That(first.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(second.Outcome, Is.EqualTo(PlayerHitOutcome.Blocked));
            Assert.That(defense.RemainingParryCapacity, Is.Zero);
            Assert.That(health.Current, Is.EqualTo(10));
        }

        [Test]
        public void Configure_DuringDefense_AffectsNextActivationOnly()
        {
            defense.Configure(0.3f, 0f, 120f, 0.6f, 2);
            defense.SetDefensePressed(true);

            defense.Configure(0.01f, 0f, 120f, 0.6f, 1);
            defense.Tick(0.02f);
            PlayerHitResult first = defense.ReceiveHit(CreateMeleeHit(attacker));
            PlayerHitResult second = defense.ReceiveHit(CreateMeleeHit(attacker));

            Assert.That(first.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(second.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Blocking));

            defense.SetDefensePressed(false);
            defense.Tick(0f);
            defense.SetDefensePressed(true);
            Assert.That(defense.RemainingParryCapacity, Is.EqualTo(1));
            defense.Tick(0.02f);
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Blocking));
        }

        [Test]
        public void ReceiveHit_FromRear_AppliesResolvedDamageThroughHealth()
        {
            attacker.transform.position = Vector2.left;
            defense.SetDefensePressed(true);

            PlayerHitResult result = defense.ReceiveHit(new PlayerHitRequest(
                4,
                attacker,
                attacker.transform.position,
                CombatDamageType.Melee));

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Damaged));
            Assert.That(result.AppliedDamage, Is.EqualTo(4));
            Assert.That(health.Current, Is.EqualTo(6));
        }

        [Test]
        public void ReceiveHit_ReportsDamageActuallyAppliedByHealth()
        {
            health.ConfigureMaxHealth(2, refill: true);
            attacker.transform.position = Vector2.left;

            PlayerHitResult result = defense.ReceiveHit(new PlayerHitRequest(
                4,
                attacker,
                attacker.transform.position,
                CombatDamageType.Melee));

            Assert.That(result.RequestedDamage, Is.EqualTo(4));
            Assert.That(result.AppliedDamage, Is.EqualTo(2));
        }

        [Test]
        public void Guarding_UsesConfiguredMovementMultiplierOnlyWhileActive()
        {
            Assert.That(defense.MovementSpeedMultiplier, Is.EqualTo(1f));

            defense.SetDefensePressed(true);
            Assert.That(defense.MovementSpeedMultiplier, Is.EqualTo(0.6f).Within(0.0001f));

            defense.SetDefensePressed(false);
            Assert.That(defense.MovementSpeedMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void MissingCanceledInput_ReleasesDefenseWhenNoBoundControlRemainsPressed()
        {
            defense.Configure(0.15f, 0.15f, 120f, 0.6f, () => false);
            defense.OnDefensePressedStateChanged(true);
            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.ParryWindow));

            defense.Tick(0f);

            Assert.That(defense.State, Is.EqualTo(PlayerDefenseState.Recovery));
            Assert.IsFalse(defense.IsGuarding);
        }

        [Test]
        public void StartingDefense_CancelsHeldFireAndActiveSwing()
        {
            var attackPlayer = new GameObject("AttackPlayer");
            try
            {
                var fireInput = attackPlayer.AddComponent<PlayerFireInputController>();
                var attackLoop = attackPlayer.AddComponent<PlayerAttackLoop>();
                attackPlayer.AddComponent<PlayerController>();
                var attackDefense = attackPlayer.AddComponent<PlayerDefenseController>();
                attackDefense.Configure(0.15f, 0.15f, 120f, 0.6f);

                fireInput.Configure(null, () => true);
                fireInput.OnFirePressedStateChanged(true);
                attackLoop.Tick(0.01f, 1f, true);
                Assert.IsTrue(attackLoop.IsSwingActive);

                attackDefense.SetDefensePressed(true);

                Assert.IsFalse(fireInput.IsFireHeld);
                Assert.IsFalse(attackLoop.IsSwingActive);
            }
            finally
            {
                Object.DestroyImmediate(attackPlayer);
            }
        }

        [Test]
        public void EnemyMeleeDelivery_FrontalParry_UsesContextualReceiver()
        {
            var delivery = attacker.AddComponent<EnemyMeleeAttackDelivery>();
            PlayerHitResult observed = default;
            defense.HitResolved += result => observed = result;
            defense.SetDefensePressed(true);

            bool delivered = delivery.TryDeliver(player.transform, null, CreateDamageSnapshot(4));

            Assert.IsTrue(delivered);
            Assert.That(health.Current, Is.EqualTo(10));
            Assert.That(observed.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(observed.Attacker, Is.SameAs(attacker));
        }

        [Test]
        public void EnemyMeleeDelivery_RearAttack_AppliesCapturedDamage()
        {
            attacker.transform.position = Vector2.left;
            var delivery = attacker.AddComponent<EnemyMeleeAttackDelivery>();
            defense.SetDefensePressed(true);

            bool delivered = delivery.TryDeliver(player.transform, null, CreateDamageSnapshot(4));

            Assert.IsTrue(delivered);
            Assert.That(health.Current, Is.EqualTo(6));
        }

        private static CombatEquipmentSnapshot CreateDamageSnapshot(int damage)
        {
            return new CombatEquipmentSnapshot(
                "test-melee",
                damage,
                1f,
                0f,
                0f,
                CombatEquipmentCapability.MeleeDelivery,
                null,
                null,
                0f,
                0f,
                0f);
        }

        private static PlayerHitRequest CreateMeleeHit(GameObject source)
        {
            return new PlayerHitRequest(
                4,
                source,
                source.transform.position,
                CombatDamageType.Melee);
        }
    }
}

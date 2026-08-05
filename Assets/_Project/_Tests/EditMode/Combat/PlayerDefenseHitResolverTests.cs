using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerDefenseHitResolverTests
    {
        private const float BlockArcDegrees = 120f;
        private GameObject attacker;

        [SetUp]
        public void SetUp()
        {
            attacker = new GameObject("Attacker");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(attacker);
        }

        [Test]
        public void Resolve_FrontMeleeDuringParry_NegatesDamageAndPreservesAttacker()
        {
            var request = new PlayerHitRequest(4, attacker, Vector2.right, CombatDamageType.Melee);

            PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
                request,
                PlayerDefenseState.ParryWindow,
                Vector2.zero,
                Vector2.right,
                BlockArcDegrees);

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
            Assert.That(result.AppliedDamage, Is.Zero);
            Assert.That(result.Attacker, Is.SameAs(attacker));
            Assert.That(result.RequestedDamage, Is.EqualTo(4));
            Assert.That(result.DamageType, Is.EqualTo(CombatDamageType.Melee));
        }

        [Test]
        public void Resolve_FrontMeleeDuringBlock_NegatesDamage()
        {
            var request = new PlayerHitRequest(4, attacker, Vector2.right, CombatDamageType.Melee);

            PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
                request,
                PlayerDefenseState.Blocking,
                Vector2.zero,
                Vector2.right,
                BlockArcDegrees);

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Blocked));
            Assert.That(result.AppliedDamage, Is.Zero);
        }

        [TestCase(60f, PlayerHitOutcome.Blocked)]
        [TestCase(-60f, PlayerHitOutcome.Blocked)]
        [TestCase(60.01f, PlayerHitOutcome.Damaged)]
        [TestCase(-60.01f, PlayerHitOutcome.Damaged)]
        [TestCase(180f, PlayerHitOutcome.Damaged)]
        public void Resolve_UsesInclusiveArcEdges(float attackAngle, PlayerHitOutcome expectedOutcome)
        {
            Vector2 attackOrigin = Quaternion.Euler(0f, 0f, attackAngle) * Vector2.right;
            var request = new PlayerHitRequest(3, attacker, attackOrigin, CombatDamageType.Melee);

            PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
                request,
                PlayerDefenseState.Blocking,
                Vector2.zero,
                Vector2.right,
                BlockArcDegrees);

            Assert.That(result.Outcome, Is.EqualTo(expectedOutcome));
            Assert.That(result.AppliedDamage, Is.EqualTo(expectedOutcome == PlayerHitOutcome.Damaged ? 3 : 0));
        }

        [TestCase(PlayerDefenseState.Idle)]
        [TestCase(PlayerDefenseState.Recovery)]
        public void Resolve_WhenNotActivelyGuarding_AppliesFullDamage(PlayerDefenseState state)
        {
            var request = new PlayerHitRequest(3, attacker, Vector2.right, CombatDamageType.Melee);

            PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
                request,
                state,
                Vector2.zero,
                Vector2.right,
                BlockArcDegrees);

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Damaged));
            Assert.That(result.AppliedDamage, Is.EqualTo(3));
        }

        [Test]
        public void Resolve_ProjectileDuringParry_BypassesDefense()
        {
            var request = new PlayerHitRequest(5, attacker, Vector2.right, CombatDamageType.Projectile);

            PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
                request,
                PlayerDefenseState.ParryWindow,
                Vector2.zero,
                Vector2.right,
                BlockArcDegrees);

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Damaged));
            Assert.That(result.AppliedDamage, Is.EqualTo(5));
        }

        [TestCase(0f, 1f)]
        [TestCase(1f, 0f)]
        public void Resolve_WithoutUsableDirection_AppliesFullDamage(float facingX, float originX)
        {
            var request = new PlayerHitRequest(2, attacker, new Vector2(originX, 0f), CombatDamageType.Melee);

            PlayerHitResult result = PlayerDefenseHitResolver.Resolve(
                request,
                PlayerDefenseState.Blocking,
                new Vector2(originX, 0f),
                new Vector2(facingX, 0f),
                BlockArcDegrees);

            Assert.That(result.Outcome, Is.EqualTo(PlayerHitOutcome.Damaged));
            Assert.That(result.AppliedDamage, Is.EqualTo(2));
        }

        [Test]
        public void Request_NormalizesNegativeDamageWithoutDiscardingContext()
        {
            var request = new PlayerHitRequest(-3, attacker, Vector2.left, CombatDamageType.Melee);

            Assert.That(request.Damage, Is.Zero);
            Assert.That(request.Attacker, Is.SameAs(attacker));
            Assert.That(request.AttackOrigin, Is.EqualTo(Vector2.left));
        }
    }
}

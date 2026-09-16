using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySurroundEligibilityTests
    {
        [TestCase("enabled", true)]
        [TestCase("disabled", false)]
        [TestCase("missing", false)]
        [TestCase("none", false)]
        public void PredictiveChase_RequiresAuthoritativeSurroundEligibility(string kind, bool expected)
        {
            var player = new GameObject("Player");
            var enemy = new GameObject("MeleeEnemy");
            try
            {
                player.tag = "Player";
                enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
                enemy.AddComponent<Health>().ConfigureMaxHealth(10, true);
                var controller = enemy.AddComponent<EnemyController2D>();
                controller.Debug_SetupRefs(player.transform);
                controller.Debug_SetTargetDecision(player.transform, player.transform, EnemyTargetType.Player);
                var movement = enemy.GetComponent<EnemyLocomotion>();
                movement.SetMovementState(EnemyController2D.State.CHASE);
                if (kind != "missing")
                {
                    var eligibility = enemy.AddComponent<EnemySurroundEligibility>();
                    eligibility.AvoidanceGroup = kind == "none"
                        ? PredictiveAvoidanceGroup.None : PredictiveAvoidanceGroup.SmallMelee;
                    eligibility.enabled = kind != "disabled";
                    Assert.That(eligibility.IsEligibleFor(player.transform), Is.EqualTo(kind != "disabled"),
                        "The predictive group must not change the existing surround eligibility contract.");
                }

                Assert.That(EnemyPredictiveChase.IsEligible(controller, player.transform, movement),
                    Is.EqualTo(expected));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void ActiveLivingPlayerTargetingMeleeEnemy_IsEligible()
        {
            Assert.IsTrue(EnemySurroundEligibility.Evaluate(
                isActiveAndEnabled: true,
                isControllerEnabled: true,
                isAlive: true,
                isRooted: false,
                targetType: EnemyTargetType.Player,
                targetsPlayer: true));
        }

        [TestCase(false, true, true, false, EnemyTargetType.Player, true)]
        [TestCase(true, false, true, false, EnemyTargetType.Player, true)]
        [TestCase(true, true, false, false, EnemyTargetType.Player, true)]
        [TestCase(true, true, true, true, EnemyTargetType.Player, true)]
        [TestCase(true, true, true, false, EnemyTargetType.Barrier, false)]
        [TestCase(true, true, true, false, EnemyTargetType.Player, false)]
        public void IneligibleState_IsExcluded(
            bool isActiveAndEnabled,
            bool isControllerEnabled,
            bool isAlive,
            bool isRooted,
            EnemyTargetType targetType,
            bool targetsPlayer)
        {
            Assert.IsFalse(EnemySurroundEligibility.Evaluate(
                isActiveAndEnabled,
                isControllerEnabled,
                isAlive,
                isRooted,
                targetType,
                targetsPlayer));
        }
    }
}

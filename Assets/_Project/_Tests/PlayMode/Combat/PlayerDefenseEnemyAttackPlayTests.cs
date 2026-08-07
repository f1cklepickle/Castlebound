using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerDefenseEnemyAttackPlayTests
    {
        [UnityTest]
        public IEnumerator EnemyClockImpact_DuringFrontalParry_StaggersThenRecoversFresh()
        {
            var player = CreatePlayer();
            var enemy = CreateEnemy(player.transform);
            var defense = player.GetComponent<PlayerDefenseController>();
            PlayerHitResult observed = default;
            defense.HitResolved += result => observed = result;

            try
            {
                defense.SetDefensePressed(true);
                yield return new WaitForSeconds(0.08f);

                Assert.That(player.GetComponent<Health>().Current, Is.EqualTo(10));
                Assert.That(observed.Outcome, Is.EqualTo(PlayerHitOutcome.Parried));
                Assert.That(observed.Attacker, Is.SameAs(enemy));
                var stagger = enemy.GetComponent<EnemyStaggerReceiver>();
                Assert.That(stagger.State, Is.EqualTo(EnemyStaggerState.Staggered));
                Assert.IsFalse(enemy.GetComponent<EnemyAttack>().IsAttackActive);

                Vector2 lockedPosition = enemy.transform.position;
                Vector2 lockedFacing = enemy.GetComponent<EnemyFacing>().AimDirection;
                int targetRevision = enemy.GetComponent<EnemyTargeting>().TargetRevision;
                yield return new WaitForSeconds(0.2f);
                Assert.That(Vector2.Distance(enemy.transform.position, lockedPosition),
                    Is.LessThan(0.001f));
                Assert.That(enemy.GetComponent<EnemyFacing>().AimDirection, Is.EqualTo(lockedFacing));

                yield return new WaitForSeconds(0.85f);
                yield return new WaitForFixedUpdate();
                Assert.That(stagger.State, Is.EqualTo(EnemyStaggerState.Inactive));
                Assert.That(enemy.GetComponent<EnemyTargeting>().TargetRevision,
                    Is.GreaterThan(targetRevision));
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(player);
            }
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.position = new Vector2(0.5f, 0f);
            player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            var defense = player.AddComponent<PlayerDefenseController>();
            defense.Configure(0.15f, 0.15f, 120f, 0.6f);
            return player;
        }

        private static GameObject CreateEnemy(Transform target)
        {
            var enemy = new GameObject("Enemy");
            var body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            enemy.AddComponent<BoxCollider2D>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Debug_SetupRefs(target);

            var facing = enemy.GetComponent<EnemyFacing>();
            SetField(facing, "attackAlignmentThreshold", 180f);

            var attack = enemy.AddComponent<EnemyAttack>();
            var delivery = attack.AttackDeliverySource as EnemyMeleeAttackDelivery;
            var stagger = enemy.AddComponent<EnemyStaggerReceiver>();
            stagger.Configure(true, 1f, attack);
            SetField(attack, "staggerReceiver", stagger);
            SetField(delivery, "staggerReceiver", stagger);
            SetField(controller, "staggerReceiver", stagger);
            SetField(attack, "windupSeconds", 0.02f);
            attack.CooldownSeconds = 1f;
            attack.Damage = 4;
            return enemy;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            instance.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(instance, value);
        }
    }
}

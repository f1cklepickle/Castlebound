using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerDefenseEnemyAttackPlayTests
    {
        [UnityTest]
        public IEnumerator EnemyClockImpact_DuringFrontalParry_NegatesPlayerDamage()
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

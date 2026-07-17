using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class EnemyAttackTargetLockPlayTests
    {
        [UnityTest]
        public IEnumerator CompletedWindup_DamagesOnlyLockedSelectedTarget()
        {
            var player = CreateDamageable("Player", new Vector2(0.5f, 0f), "Player");
            var unrelated = CreateDamageable("Unrelated", new Vector2(0.5f, 0f), null);
            var enemy = CreateEnemy(player.transform, windupSeconds: 0.02f, cooldownSeconds: 0.5f);

            try
            {
                yield return new WaitForSeconds(0.08f);

                Assert.That(player.GetComponent<Health>().Current, Is.EqualTo(9),
                    "The locked selected target should receive one completed melee hit.");
                Assert.That(unrelated.GetComponent<Health>().Current, Is.EqualTo(10),
                    "An unrelated overlap recipient must not receive melee damage.");
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(unrelated);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator TargetEscapesDuringWindup_CancelsDamageAndResumesPursuit()
        {
            var player = CreateDamageable("Player", new Vector2(0.5f, 0f), "Player");
            var enemy = CreateEnemy(player.transform, windupSeconds: 0.08f, cooldownSeconds: 1f);
            var controller = enemy.GetComponent<EnemyController2D>();
            float startingX = enemy.transform.position.x;

            try
            {
                yield return new WaitForSeconds(0.02f);
                player.transform.position = new Vector2(3f, 0f);
                Physics2D.SyncTransforms();

                yield return new WaitForSeconds(0.12f);
                yield return new WaitForFixedUpdate();

                Assert.That(player.GetComponent<Health>().Current, Is.EqualTo(10),
                    "A target outside collider-aware reach at hit time must receive no damage.");
                Assert.IsTrue(controller.IsChaseRequested,
                    "A cancelled windup should request pursuit until attack reach is reacquired.");
                Assert.That(enemy.transform.position.x, Is.GreaterThan(startingX),
                    "The cancelled attacker should move toward its escaped target.");
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(player);
            }
        }

        [UnityTest]
        public IEnumerator CancelledWindup_ReacquiresRangeAndStartsEntirelyNewWindup()
        {
            var player = CreateDamageable("Player", new Vector2(0.5f, 0f), "Player");
            var enemy = CreateEnemy(player.transform, windupSeconds: 0.08f, cooldownSeconds: 1f);

            try
            {
                yield return new WaitForSeconds(0.02f);
                player.transform.position = new Vector2(3f, 0f);
                Physics2D.SyncTransforms();
                yield return new WaitForSeconds(0.1f);

                player.transform.position = new Vector2(0.5f, 0f);
                Physics2D.SyncTransforms();
                yield return new WaitForFixedUpdate();
                yield return null;

                Assert.That(player.GetComponent<Health>().Current, Is.EqualTo(10),
                    "Reacquiring range must begin a new windup rather than applying immediate damage.");

                yield return new WaitForSeconds(0.1f);

                Assert.That(player.GetComponent<Health>().Current, Is.EqualTo(9),
                    "A cancelled windup should not impose the completed cooldown before a new swing.");
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(player);
            }
        }

        private static GameObject CreateDamageable(string name, Vector2 position, string tag)
        {
            var target = new GameObject(name);
            if (!string.IsNullOrEmpty(tag))
                target.tag = tag;
            target.layer = LayerMask.NameToLayer("Player");
            target.transform.position = position;
            target.AddComponent<BoxCollider2D>();
            target.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            return target;
        }

        private static GameObject CreateEnemy(Transform target, float windupSeconds, float cooldownSeconds)
        {
            var enemy = new GameObject("Enemy");
            var body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            enemy.AddComponent<BoxCollider2D>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Debug_SetupRefs(target);

            var attack = enemy.AddComponent<EnemyAttack>();
            SetField(attack, "windupSeconds", windupSeconds);
            attack.CooldownSeconds = cooldownSeconds;
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

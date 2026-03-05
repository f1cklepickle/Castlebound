using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerAttackDamagePlayTests
    {
        [UnityTest]
        public IEnumerator AttackAnimation_DealsDamageToEnemy()
        {
            yield return LoadMainPrototype();

            var player = Object.FindObjectOfType<PlayerController>();
            Assert.NotNull(player, "Expected PlayerController in MainPrototype.");
            Assert.NotNull(player.animator, "Player animator reference must be available.");

            var enemy = SpawnEnemyAtPlayerHitbox(player);
            var health = enemy.GetComponent<Health>();
            Assert.NotNull(health, "Spawned enemy must have Health.");

            var before = health.Current;

            player.animator.SetTrigger("Attack");
            yield return WaitForDamageOrTimeout(player, enemy, health, before, 1.2f);

            Assert.Less(health.Current, before,
                "Attack animation should fire hitbox events and reduce enemy health.");

            Object.Destroy(enemy);
        }

        [UnityTest]
        public IEnumerator AttackAnimation_CanDealDamageAcrossConsecutiveSwings()
        {
            yield return LoadMainPrototype();

            var player = Object.FindObjectOfType<PlayerController>();
            Assert.NotNull(player, "Expected PlayerController in MainPrototype.");
            Assert.NotNull(player.animator, "Player animator reference must be available.");

            var firstEnemy = SpawnEnemyAtPlayerHitbox(player);
            var firstHealth = firstEnemy.GetComponent<Health>();
            var firstBefore = firstHealth.Current;

            player.animator.SetTrigger("Attack");
            yield return WaitForDamageOrTimeout(player, firstEnemy, firstHealth, firstBefore, 1.2f);

            Assert.Less(firstHealth.Current, firstBefore,
                "First swing should damage the first enemy.");
            Object.Destroy(firstEnemy);

            var secondEnemy = SpawnEnemyAtPlayerHitbox(player);
            var secondHealth = secondEnemy.GetComponent<Health>();
            var secondBefore = secondHealth.Current;

            player.animator.SetTrigger("Attack");
            yield return WaitForDamageOrTimeout(player, secondEnemy, secondHealth, secondBefore, 1.2f);

            Assert.Less(secondHealth.Current, secondBefore,
                "Second swing should still damage a new enemy (regression guard for one-and-done attack states).");
            Object.Destroy(secondEnemy);
        }

        private static IEnumerator LoadMainPrototype()
        {
            var load = SceneManager.LoadSceneAsync("MainPrototype", LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static GameObject SpawnEnemyAtPlayerHitbox(PlayerController player)
        {
            var enemy = new GameObject("TestEnemy");
            enemy.tag = "Enemy";
            enemy.layer = LayerMask.NameToLayer("Enemies");

            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;

            enemy.AddComponent<CircleCollider2D>().radius = 0.4f;
            enemy.AddComponent<Health>();

            var hitbox = player.GetComponentInChildren<Hitbox>(true);
            Assert.NotNull(hitbox, "Player must have a Hitbox child.");

            enemy.transform.position = hitbox.transform.position;
            Physics2D.SyncTransforms();
            return enemy;
        }

        private static IEnumerator WaitForDamageOrTimeout(
            PlayerController player,
            GameObject enemy,
            Health enemyHealth,
            int initialHealth,
            float timeoutSeconds)
        {
            var hitbox = player.GetComponentInChildren<Hitbox>(true);
            Assert.NotNull(hitbox, "Player must have a Hitbox child.");

            var end = Time.realtimeSinceStartup + timeoutSeconds;
            while (Time.realtimeSinceStartup < end)
            {
                if (enemy != null)
                {
                    enemy.transform.position = hitbox.transform.position;
                    Physics2D.SyncTransforms();
                }

                if (enemyHealth != null && enemyHealth.Current < initialHealth)
                    yield break;

                yield return null;
            }
        }
    }
}

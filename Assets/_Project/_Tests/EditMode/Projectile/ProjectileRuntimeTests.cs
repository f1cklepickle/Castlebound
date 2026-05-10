using System.Reflection;
using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Projectile
{
    public class ProjectileRuntimeTests
    {
        [Test]
        public void OnTriggerEnter_DamagesTargetAndRaisesPlayerHitEnemyFeedbackCue()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var projectileObject = new GameObject("Projectile");
            var projectileCollider = projectileObject.AddComponent<BoxCollider2D>();
            projectileCollider.isTrigger = true;
            projectileObject.AddComponent<Rigidbody2D>();
            var projectile = projectileObject.AddComponent<ProjectileRuntime>();
            SetPrivate(projectile, "hitFeedbackChannel", channel);

            var target = new GameObject("Enemy");
            target.layer = EnemyLayer();
            target.transform.position = new Vector3(2f, 3f, 0f);
            var targetCollider = target.AddComponent<BoxCollider2D>();
            var damageable = target.AddComponent<DummyDamageable>();

            bool raised = false;
            FeedbackCue received = default;
            channel.Raised += cue =>
            {
                raised = true;
                received = cue;
            };

            try
            {
                projectile.Launch(ProjectileLaunchContext.Directional(
                    Vector2.zero,
                    Vector2.right,
                    null,
                    1f,
                    3,
                    1f,
                    1 << EnemyLayer()));

                InvokeTrigger(projectile, targetCollider);

                Assert.That(damageable.DamageTaken, Is.EqualTo(3));
                Assert.IsTrue(raised, "Projectile hit should reuse the player-hit-enemy feedback cue for enemy hit flash.");
                Assert.That(received.Type, Is.EqualTo(FeedbackCueType.PlayerHitEnemy));
                Assert.That(received.Position, Is.EqualTo(target.transform.position));
                Assert.That(received.TargetInstanceId, Is.EqualTo(target.GetInstanceID()));
            }
            finally
            {
                if (projectileObject != null)
                {
                    Object.DestroyImmediate(projectileObject);
                }

                Object.DestroyImmediate(target);
                Object.DestroyImmediate(channel);
            }
        }

        [Test]
        public void ApplyImpactEmbed_MovesProjectileForwardAlongLaunchDirection()
        {
            var projectileObject = new GameObject("Projectile");
            var projectileCollider = projectileObject.AddComponent<BoxCollider2D>();
            projectileCollider.isTrigger = true;
            projectileObject.AddComponent<Rigidbody2D>();
            var projectile = projectileObject.AddComponent<ProjectileRuntime>();
            SetPrivate(projectile, "impactEmbedDistance", 0.2f);

            try
            {
                projectile.Launch(ProjectileLaunchContext.Directional(
                    Vector2.zero,
                    Vector2.right,
                    null,
                    1f,
                    1,
                    1f,
                    1 << EnemyLayer()));

                InvokePrivate(projectile, "ApplyImpactEmbed");

                Assert.That(projectile.transform.position.x, Is.EqualTo(0.2f).Within(0.001f));
                Assert.That(projectile.transform.position.y, Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(projectileObject);
            }
        }

        [Test]
        public void OnTriggerEnter_IgnoresCallbacksAfterProjectileImpacted()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var projectileObject = new GameObject("Projectile");
            var projectileCollider = projectileObject.AddComponent<BoxCollider2D>();
            projectileCollider.isTrigger = true;
            projectileObject.AddComponent<Rigidbody2D>();
            var projectile = projectileObject.AddComponent<ProjectileRuntime>();
            SetPrivate(projectile, "hitFeedbackChannel", channel);

            var target = new GameObject("Enemy");
            target.layer = EnemyLayer();
            var targetCollider = target.AddComponent<BoxCollider2D>();
            var damageable = target.AddComponent<DummyDamageable>();

            int raisedCount = 0;
            channel.Raised += _ => raisedCount++;

            try
            {
                projectile.Launch(ProjectileLaunchContext.Directional(
                    Vector2.zero,
                    Vector2.right,
                    null,
                    1f,
                    3,
                    1f,
                    1 << EnemyLayer()));
                SetPrivate(projectile, "impacted", true);

                InvokeTrigger(projectile, targetCollider);

                Assert.That(damageable.DamageTaken, Is.EqualTo(0));
                Assert.That(raisedCount, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(projectileObject);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(channel);
            }
        }

        private static int EnemyLayer()
        {
            var layer = LayerMask.NameToLayer("Enemies");
            Assert.That(layer, Is.GreaterThanOrEqualTo(0), "Project must define the Enemies layer.");
            return layer;
        }

        private static void InvokeTrigger(ProjectileRuntime projectile, Collider2D targetCollider)
        {
            InvokePrivate(projectile, "OnTriggerEnter2D", targetCollider);
        }

        private static void InvokePrivate(object instance, string methodName, params object[] arguments)
        {
            var method = typeof(ProjectileRuntime).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method, $"ProjectileRuntime should define private method '{methodName}'.");
            method.Invoke(instance, arguments);
        }

        private static void SetPrivate<T>(object instance, string fieldName, T value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{instance.GetType().Name} should define private field '{fieldName}'.");
            field.SetValue(instance, value);
        }

        private sealed class DummyDamageable : MonoBehaviour, IDamageable
        {
            public int DamageTaken { get; private set; }

            public void TakeDamage(int amount)
            {
                DamageTaken += amount;
            }
        }
    }
}

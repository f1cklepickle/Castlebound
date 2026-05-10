using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace Castlebound.Tests.Projectile
{
    public class ProjectileArrowPrefabContractTests
    {
        private const string ArrowPrefabPath = "Assets/_Project/Prefabs/Projectile_Arrow.prefab";

        [Test]
        public void ProjectileArrowPrefab_HasReusableProjectileContract()
        {
            var prefabRoot = PrefabTestUtil.Load(ArrowPrefabPath);

            try
            {
                Assert.NotNull(prefabRoot.GetComponent<ProjectileRuntime>(), "Projectile_Arrow must include ProjectileRuntime.");

                var collider = prefabRoot.GetComponent<Collider2D>();
                Assert.NotNull(collider, "Projectile_Arrow must include a Collider2D.");
                Assert.IsTrue(collider.isTrigger, "Projectile_Arrow collider must be a trigger.");
                Assert.IsInstanceOf<PolygonCollider2D>(collider, "Projectile_Arrow should use a polygon collider to fit the diagonal arrow sprite.");

                var body = prefabRoot.GetComponent<Rigidbody2D>();
                Assert.NotNull(body, "Projectile_Arrow must include a Rigidbody2D for reliable trigger callbacks.");
                Assert.That(body.bodyType, Is.EqualTo(RigidbodyType2D.Kinematic), "Projectile_Arrow Rigidbody2D must be kinematic.");
                Assert.That(body.gravityScale, Is.EqualTo(0f), "Projectile_Arrow Rigidbody2D must not use gravity.");

                var spriteRenderer = prefabRoot.GetComponent<SpriteRenderer>();
                Assert.NotNull(spriteRenderer, "Projectile_Arrow must include a SpriteRenderer.");
                Assert.NotNull(spriteRenderer.sprite, "Projectile_Arrow must reference the imported arrow sprite.");
                Assert.That(spriteRenderer.sortingOrder, Is.GreaterThan(3), "Projectile_Arrow should render above the base tower crossbow top.");

                var runtime = prefabRoot.GetComponent<ProjectileRuntime>();
                Assert.NotNull(GetPrivate<FeedbackEventChannel>(runtime, "hitFeedbackChannel"), "Projectile_Arrow should raise the shared enemy hit flash feedback channel.");
                Assert.That(GetPrivate<float>(runtime, "impactLingerSeconds"), Is.GreaterThan(0f), "Projectile_Arrow should linger briefly on impact so hits read visually.");
                Assert.That(GetPrivate<float>(runtime, "impactEmbedDistance"), Is.GreaterThanOrEqualTo(0.45f), "Projectile_Arrow should nudge far enough forward on impact to visibly connect with enemies.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }

        private static T GetPrivate<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{instance.GetType().Name} should define private field '{fieldName}'.");
            return (T)field.GetValue(instance);
        }
    }
}

using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
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
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }
    }
}

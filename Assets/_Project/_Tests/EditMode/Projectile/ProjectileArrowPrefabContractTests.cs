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

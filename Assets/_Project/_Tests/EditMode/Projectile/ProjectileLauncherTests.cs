using Castlebound.Gameplay.Projectile;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Projectile
{
    public class ProjectileLauncherTests
    {
        [Test]
        public void Launch_InstantiatesAtOriginAndAimsAtTargetPoint()
        {
            var owner = new GameObject("Owner");
            var prefabObject = new GameObject("ProjectilePrefab");
            prefabObject.AddComponent<CircleCollider2D>();
            prefabObject.AddComponent<Rigidbody2D>();
            var prefab = prefabObject.AddComponent<ProjectileRuntime>();
            ProjectileRuntime launched = null;

            try
            {
                var request = new ProjectileLaunchRequest(
                    prefab,
                    new Vector2(2f, 3f),
                    new Vector2(5f, 3f),
                    owner.transform,
                    6f,
                    4,
                    2f,
                    1 << 7,
                    -45f);

                launched = ProjectileLauncher.Launch(request);

                Assert.NotNull(launched);
                Assert.That((Vector2)launched.transform.position, Is.EqualTo(new Vector2(2f, 3f)));
                Assert.That(launched.transform.eulerAngles.z, Is.EqualTo(315f).Within(0.01f));
            }
            finally
            {
                if (launched != null)
                {
                    Object.DestroyImmediate(launched.gameObject);
                }

                Object.DestroyImmediate(prefabObject);
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void Launch_WithMissingPrefab_ReturnsNull()
        {
            var request = new ProjectileLaunchRequest(
                null,
                Vector2.zero,
                Vector2.right,
                null,
                1f,
                1,
                1f,
                1,
                0f);

            Assert.IsNull(ProjectileLauncher.Launch(request));
        }
    }
}

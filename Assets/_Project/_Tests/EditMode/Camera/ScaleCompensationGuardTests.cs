using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Scale
{
    public class ScaleCompensationGuardTests
    {
        [TestCase("Assets/_Project/Prefabs/Player.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy.prefab")]
        public void PrefabRootScale_IsNormalized(string prefabPath)
        {
            var prefab = PrefabTestUtil.Load(prefabPath);
            try
            {
                AssertVectorOne(prefab.transform.localScale, $"{prefabPath} root transform scale should stay normalized.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [TestCase("Assets/_Project/Prefabs/Player.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy.prefab")]
        public void SpriteRenderers_DoNotUseScaleCompensation(string prefabPath)
        {
            var prefab = PrefabTestUtil.Load(prefabPath);
            try
            {
                var sprites = prefab.GetComponentsInChildren<SpriteRenderer>(true);
                Assert.That(sprites.Length, Is.GreaterThan(0), $"Expected sprite renderers in {prefabPath}.");

                foreach (var sprite in sprites)
                {
                    AssertVectorOne(sprite.transform.localScale,
                        $"Sprite '{sprite.name}' in {prefabPath} should not use transform scale compensation.");
                }
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        private static void AssertVectorOne(Vector3 scale, string message)
        {
            Assert.That(scale.x, Is.EqualTo(1f).Within(0.001f), message);
            Assert.That(scale.y, Is.EqualTo(1f).Within(0.001f), message);
            Assert.That(scale.z, Is.EqualTo(1f).Within(0.001f), message);
        }
    }
}

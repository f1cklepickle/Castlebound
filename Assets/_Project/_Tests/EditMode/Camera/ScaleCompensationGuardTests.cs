using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Scale
{
    public class ScaleCompensationGuardTests
    {
        private const string BarrierPrefabPath = "Assets/_Project/Prefabs/Barrier_Gate.prefab";

        [TestCase("Assets/_Project/Prefabs/Player.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy.prefab")]
        [TestCase(BarrierPrefabPath)]
        public void PrefabRootScale_IsNormalized(string prefabPath)
        {
            var prefab = PrefabTestUtil.Load(prefabPath);
            try
            {
                // Temporary exception: legacy barrier art remains at 2x scale until replacement sprite lands.
                if (prefabPath == BarrierPrefabPath)
                {
                    Assert.That(prefab.transform.localScale.x, Is.EqualTo(2f).Within(0.001f),
                        $"{prefabPath} temporary exception expects root x scale of 2.");
                    Assert.That(prefab.transform.localScale.y, Is.EqualTo(3f).Within(0.001f),
                        $"{prefabPath} temporary exception expects root y scale of 3.");
                    Assert.That(prefab.transform.localScale.z, Is.EqualTo(1f).Within(0.001f),
                        $"{prefabPath} temporary exception expects root z scale of 1.");
                    return;
                }

                AssertVectorOne(prefab.transform.localScale, $"{prefabPath} root transform scale should stay normalized.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefab);
            }
        }

        [TestCase("Assets/_Project/Prefabs/Player.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy.prefab")]
        [TestCase(BarrierPrefabPath)]
        public void SpriteRenderers_DoNotUseScaleCompensation(string prefabPath)
        {
            var prefab = PrefabTestUtil.Load(prefabPath);
            try
            {
                var sprites = prefab.GetComponentsInChildren<SpriteRenderer>(true);
                Assert.That(sprites.Length, Is.GreaterThan(0), $"Expected sprite renderers in {prefabPath}.");

                foreach (var sprite in sprites)
                {
                    if (prefabPath == BarrierPrefabPath && sprite.transform == prefab.transform)
                    {
                        Assert.That(sprite.transform.localScale.x, Is.EqualTo(2f).Within(0.001f),
                            "Barrier root sprite temporary exception expects x scale of 2.");
                        Assert.That(sprite.transform.localScale.y, Is.EqualTo(3f).Within(0.001f),
                            "Barrier root sprite temporary exception expects y scale of 3.");
                        Assert.That(sprite.transform.localScale.z, Is.EqualTo(1f).Within(0.001f),
                            "Barrier root sprite temporary exception expects z scale of 1.");
                        continue;
                    }

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

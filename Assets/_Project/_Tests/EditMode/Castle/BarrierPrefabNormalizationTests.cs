using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierPrefabNormalizationTests
    {
        private const string BarrierPrefabPath = "Assets/_Project/Prefabs/Barrier_Gate.prefab";

        [Test]
        public void BarrierPrefab_RootTransformScale_IsNormalizedToOne()
        {
            var prefabRoot = PrefabTestUtil.Load(BarrierPrefabPath);

            try
            {
                Assert.That(
                    prefabRoot.transform.localScale,
                    Is.EqualTo(Vector3.one),
                    "Barrier prefab root must remain normalized at scale (1,1,1).");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }
    }
}

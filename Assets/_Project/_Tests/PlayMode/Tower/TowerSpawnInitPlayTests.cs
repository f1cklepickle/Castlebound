using System.Collections;
using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Castlebound.Tests.PlayMode.Tower
{
    public class TowerSpawnInitPlayTests
    {
        private const string TowerPrefabPath = "Assets/_Project/Prefabs/Tower.prefab";

        [UnityTest]
        public IEnumerator SpawnedTower_InitializesRuntimeAndHierarchyContract()
        {
            var tower = CreateTower();

            yield return null;

            var runtime = tower.GetComponent<TowerRuntime>();
            Assert.NotNull(runtime, "Spawned tower must include TowerRuntime.");
            Assert.That(runtime.MaxHealth, Is.GreaterThan(0), "Tower runtime must initialize with positive max health.");
            Assert.That(runtime.CurrentHealth, Is.EqualTo(runtime.MaxHealth), "Tower runtime should initialize at full health.");
            Assert.NotNull(runtime.AimPivot, "Tower runtime must resolve AimPivot.");
            Assert.NotNull(runtime.TowerVisual, "Tower runtime must resolve TowerVisual.");
            Assert.NotNull(runtime.PlatformVisual, "Tower runtime must resolve PlatformVisual.");
            Assert.AreEqual(runtime.AimPivot, runtime.TowerVisual.parent, "TowerVisual should remain parented under AimPivot.");
            Assert.AreEqual(tower.transform, runtime.PlatformVisual.parent, "PlatformVisual should remain parented under the tower root.");

            Object.Destroy(tower);
        }

        [UnityTest]
        public IEnumerator TowerPrefab_InstantiatesWithExpectedRuntimeShell()
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TowerPrefabPath);
            Assert.NotNull(prefab, "Expected Tower prefab asset to be loadable in PlayMode.");

            var tower = Object.Instantiate(prefab);

            yield return null;

            var runtime = tower.GetComponent<TowerRuntime>();
            Assert.NotNull(runtime, "Instantiated Tower prefab must include TowerRuntime.");
            Assert.NotNull(tower.GetComponent<Collider2D>(), "Instantiated Tower prefab must include a Collider2D on the root.");
            Assert.That(runtime.MaxHealth, Is.GreaterThan(0), "Tower runtime must initialize with positive max health.");
            Assert.That(runtime.CurrentHealth, Is.EqualTo(runtime.MaxHealth), "Tower runtime should initialize at full health.");
            Assert.NotNull(runtime.AimPivot, "Tower prefab instance must expose AimPivot.");
            Assert.NotNull(runtime.TowerVisual, "Tower prefab instance must expose TowerVisual.");
            Assert.NotNull(runtime.PlatformVisual, "Tower prefab instance must expose PlatformVisual.");
            Assert.AreEqual("AimPivot", runtime.AimPivot.name, "Tower runtime should bind to the prefab AimPivot child.");
            Assert.AreEqual("TowerVisual", runtime.TowerVisual.name, "Tower runtime should bind to the prefab TowerVisual child.");
            Assert.AreEqual("PlatformVisual", runtime.PlatformVisual.name, "Tower runtime should bind to the prefab PlatformVisual child.");
            Assert.AreEqual(runtime.AimPivot, runtime.TowerVisual.parent, "TowerVisual should remain parented under AimPivot.");
            Assert.AreEqual(tower.transform, runtime.PlatformVisual.parent, "PlatformVisual should remain parented under the tower root.");

            Object.Destroy(tower);
#else
            Assert.Fail("Tower prefab PlayMode smoke requires the Unity editor.");
            yield break;
#endif
        }

        private static GameObject CreateTower()
        {
            var root = new GameObject("Tower");
            root.AddComponent<BoxCollider2D>();
            root.AddComponent<TowerRuntime>();

            var aimPivot = new GameObject("AimPivot");
            aimPivot.transform.SetParent(root.transform, false);

            var towerVisual = new GameObject("TowerVisual");
            towerVisual.transform.SetParent(aimPivot.transform, false);
            towerVisual.AddComponent<SpriteRenderer>();

            var platformVisual = new GameObject("PlatformVisual");
            platformVisual.transform.SetParent(root.transform, false);
            platformVisual.AddComponent<SpriteRenderer>();

            return root;
        }
    }
}

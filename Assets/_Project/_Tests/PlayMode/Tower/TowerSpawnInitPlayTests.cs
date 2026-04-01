using System.Collections;
using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Tower
{
    public class TowerSpawnInitPlayTests
    {
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

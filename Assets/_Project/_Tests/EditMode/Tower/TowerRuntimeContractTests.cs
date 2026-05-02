using NUnit.Framework;
using Castlebound.Gameplay.Tower;
using UnityEngine;

namespace Castlebound.Tests.Tower
{
    public class TowerRuntimeContractTests
    {
        private const string TowerPrefabPath = "Assets/_Project/Prefabs/Tower.prefab";

        [Test]
        public void TowerPrefab_ProvidesNormalizedRootAndRuntimeStateContract()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                Assert.That(
                    prefabRoot.transform.localScale,
                    Is.EqualTo(Vector3.one),
                    "Tower prefab root must remain normalized at scale (1,1,1).");

                var runtime = prefabRoot.GetComponent("TowerRuntime");
                Assert.NotNull(runtime, "Tower prefab must include TowerRuntime.");
                Assert.NotNull(prefabRoot.GetComponent<Collider2D>(), "Tower prefab root must include a Collider2D.");

                var maxHealthProperty = runtime.GetType().GetProperty("MaxHealth");
                var currentHealthProperty = runtime.GetType().GetProperty("CurrentHealth");
                Assert.NotNull(maxHealthProperty, "TowerRuntime must expose MaxHealth.");
                Assert.NotNull(currentHealthProperty, "TowerRuntime must expose CurrentHealth.");

                var maxHealth = (int)maxHealthProperty.GetValue(runtime);
                var currentHealth = (int)currentHealthProperty.GetValue(runtime);

                Assert.That(maxHealth, Is.GreaterThan(0), "TowerRuntime MaxHealth must initialize above zero.");
                Assert.That(currentHealth, Is.EqualTo(maxHealth), "TowerRuntime should initialize at full health.");

                var aimPivot = FindChildRecursive(prefabRoot.transform, "AimPivot");
                Assert.NotNull(aimPivot, "Tower prefab must include an AimPivot child.");

                var towerVisual = FindChildRecursive(prefabRoot.transform, "TowerVisual");
                Assert.NotNull(towerVisual, "Tower prefab must include a TowerVisual child.");
                Assert.IsTrue(towerVisual.IsChildOf(aimPivot), "TowerVisual should be parented under AimPivot for future rotation.");

                var platformVisual = FindChildRecursive(prefabRoot.transform, "PlatformVisual");
                Assert.NotNull(platformVisual, "Tower prefab must include a PlatformVisual child.");
                Assert.AreEqual(prefabRoot.transform, platformVisual.parent, "PlatformVisual should remain directly under the tower root.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }

        [Test]
        public void TowerPrefab_SerializesRuntimeReferences_ToApprovedChildren()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                var runtime = prefabRoot.GetComponent<TowerRuntime>();
                Assert.NotNull(runtime, "Tower prefab must include TowerRuntime.");

                var aimPivot = FindChildRecursive(prefabRoot.transform, "AimPivot");
                var towerVisual = FindChildRecursive(prefabRoot.transform, "TowerVisual");
                var platformVisual = FindChildRecursive(prefabRoot.transform, "PlatformVisual");

                Assert.AreSame(aimPivot, runtime.AimPivot, "TowerRuntime AimPivot should serialize to the AimPivot child.");
                Assert.AreSame(towerVisual, runtime.TowerVisual, "TowerRuntime TowerVisual should serialize to the TowerVisual child.");
                Assert.AreSame(platformVisual, runtime.PlatformVisual, "TowerRuntime PlatformVisual should serialize to the PlatformVisual child.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }

        [Test]
        public void TowerPrefab_SerializesTargetingContract_ForBaseTower()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                var targetingController = prefabRoot.GetComponent<TowerTargetingController>();
                Assert.NotNull(targetingController, "Tower prefab must include TowerTargetingController.");

                var profile = targetingController.TargetingProfile;
                Assert.NotNull(profile, "TowerTargetingController must reference a targeting profile.");
                Assert.That(profile.MinRange, Is.GreaterThanOrEqualTo(0f), "Tower targeting min range must be non-negative.");
                Assert.That(profile.MaxRange, Is.GreaterThan(profile.MinRange), "Tower targeting max range must exceed min range.");
                Assert.That(profile.ScanInterval, Is.GreaterThan(0f), "Tower targeting scan interval must be above zero.");
                Assert.That(profile.ScanInterval, Is.LessThanOrEqualTo(0.1f), "Base arrow tower should acquire targets responsively.");
                Assert.That(profile.SelectionMode, Is.EqualTo(TowerTargetSelectionMode.Nearest), "Base tower should acquire the nearest valid enemy.");

                var enemiesLayer = LayerMask.NameToLayer("Enemies");
                Assert.That(enemiesLayer, Is.GreaterThanOrEqualTo(0), "Project must define the Enemies layer.");
                Assert.That(
                    profile.TargetLayers.value & (1 << enemiesLayer),
                    Is.Not.Zero,
                    "Base tower targeting profile must include the Enemies layer.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }

        [Test]
        public void TowerPrefab_SerializesAimContract_ForBaseArrowTower()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                var runtime = prefabRoot.GetComponent<TowerRuntime>();
                var targetingController = prefabRoot.GetComponent<TowerTargetingController>();
                var aimController = prefabRoot.GetComponent<TowerAimController>();

                Assert.NotNull(runtime, "Tower prefab must include TowerRuntime.");
                Assert.NotNull(targetingController, "Tower prefab must include TowerTargetingController.");
                Assert.NotNull(aimController, "Base arrow tower prefab must include TowerAimController.");
                Assert.IsTrue(aimController.AimEnabled, "Base arrow tower should aim at acquired targets by default.");
                Assert.AreSame(targetingController, aimController.TargetingController, "TowerAimController should read from the prefab targeting controller.");
                Assert.AreSame(runtime.AimPivot, aimController.AimPivot, "TowerAimController should rotate the TowerRuntime AimPivot.");
                Assert.That(aimController.AimMode, Is.EqualTo(TowerAimMode.Instant), "Base arrow tower should snap aim until attack presentation needs smoothing.");
                Assert.That(aimController.RotationSpeedDegrees, Is.GreaterThanOrEqualTo(0f), "Tower aim rotation speed must be non-negative.");
                Assert.IsTrue(aimController.ReturnToIdleWhenNoTarget, "Base arrow tower should return to forward aim when no target is available.");
                Assert.That(aimController.IdleLocalAngleDegrees, Is.EqualTo(0f), "Base arrow tower idle aim should match the authored forward rest angle.");
                Assert.That(aimController.IdleReturnSpeedDegrees, Is.GreaterThan(0f), "Base arrow tower idle return speed must be above zero.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}

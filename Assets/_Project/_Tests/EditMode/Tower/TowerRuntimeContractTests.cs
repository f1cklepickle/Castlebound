using NUnit.Framework;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Projectile;
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

                var firePoint = FindChildRecursive(prefabRoot.transform, "FirePoint");
                Assert.NotNull(firePoint, "Tower prefab must include a FirePoint child.");
                Assert.IsTrue(firePoint.IsChildOf(aimPivot), "FirePoint should be parented under AimPivot so projectile spawn follows tower aim.");

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
        public void TowerPrefab_UsesApprovedBaseTowerSprites_AndAlignedFirePoint()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                var towerVisual = FindChildRecursive(prefabRoot.transform, "TowerVisual");
                var platformVisual = FindChildRecursive(prefabRoot.transform, "PlatformVisual");
                var firePoint = FindChildRecursive(prefabRoot.transform, "FirePoint");

                Assert.NotNull(towerVisual, "Tower prefab must include a TowerVisual child.");
                Assert.NotNull(platformVisual, "Tower prefab must include a PlatformVisual child.");
                Assert.NotNull(firePoint, "Tower prefab must include a FirePoint child.");

                var towerRenderer = towerVisual.GetComponent<SpriteRenderer>();
                var platformRenderer = platformVisual.GetComponent<SpriteRenderer>();

                Assert.NotNull(towerRenderer, "TowerVisual must include a SpriteRenderer.");
                Assert.NotNull(platformRenderer, "PlatformVisual must include a SpriteRenderer.");
                Assert.NotNull(towerRenderer.sprite, "TowerVisual must use the authored arrow tower sprite.");
                Assert.NotNull(platformRenderer.sprite, "PlatformVisual must use the authored tower foundation sprite.");
                Assert.That(towerRenderer.sprite.name, Is.EqualTo("Tower_Arrow"), "Base tower top must use the approved arrow tower sprite.");
                Assert.That(platformRenderer.sprite.name, Is.EqualTo("Tower_Foundation"), "Base tower platform must use the approved foundation sprite.");
                Assert.That(towerVisual.localEulerAngles.z, Is.EqualTo(45f).Within(0.01f), "Tower_Arrow art should be rotated so its diagonal bow faces local up.");
                Assert.That(firePoint.localPosition.y, Is.GreaterThan(0f), "FirePoint should sit forward of the aim pivot.");
                Assert.That(platformRenderer.sortingOrder, Is.GreaterThan(1), "Tower foundation should render above the barrier wall base sorting order.");
                Assert.That(towerRenderer.sortingOrder, Is.GreaterThan(platformRenderer.sortingOrder), "Tower arrow top should render above its foundation baseplate.");
            }
            finally
            {
                PrefabTestUtil.Unload(prefabRoot);
            }
        }

        [Test]
        public void TowerPrefab_WiresReusableCrossbowFireAnimation()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                var attackController = prefabRoot.GetComponent<TowerAttackController>();
                var binder = prefabRoot.GetComponent<TowerWeaponFireAnimationBinder>();
                var towerVisual = FindChildRecursive(prefabRoot.transform, "TowerVisual");
                var animationPlayer = towerVisual != null ? towerVisual.GetComponent<WeaponFireAnimationPlayer>() : null;

                Assert.NotNull(attackController, "Tower prefab must include TowerAttackController.");
                Assert.NotNull(binder, "Tower prefab must include TowerWeaponFireAnimationBinder.");
                Assert.NotNull(animationPlayer, "TowerVisual must include reusable WeaponFireAnimationPlayer.");
                Assert.AreSame(attackController, binder.AttackController, "Tower animation binder should listen to the tower attack controller.");
                Assert.AreSame(animationPlayer, binder.AnimationPlayer, "Tower animation binder should trigger the visual fire animation player.");
                Assert.That(animationPlayer.FireFrames, Is.Not.Null);
                Assert.That(animationPlayer.FireFrames.Length, Is.EqualTo(6), "Crossbow fire sheet should provide six fire animation frames.");
                foreach (var frame in animationPlayer.FireFrames)
                {
                    Assert.NotNull(frame, "Crossbow fire animation frame references must be assigned.");
                    Assert.That(frame.name, Does.StartWith("Crossbow_Fire_"));
                }
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
                Assert.That(targetingController.MinRange, Is.GreaterThanOrEqualTo(0f), "Tower targeting min range must be non-negative.");
                Assert.That(targetingController.MaxRange, Is.GreaterThan(targetingController.MinRange), "Tower targeting max range must exceed min range.");
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

        [Test]
        public void TowerPrefab_SerializesAttackContract_ForBaseArrowTower()
        {
            var prefabRoot = PrefabTestUtil.Load(TowerPrefabPath);

            try
            {
                var targetingController = prefabRoot.GetComponent<TowerTargetingController>();
                var attackController = prefabRoot.GetComponent<TowerAttackController>();
                var firePoint = FindChildRecursive(prefabRoot.transform, "FirePoint");

                Assert.NotNull(targetingController, "Tower prefab must include TowerTargetingController.");
                Assert.NotNull(attackController, "Base arrow tower prefab must include TowerAttackController.");
                Assert.AreSame(targetingController, attackController.TargetingController, "TowerAttackController should read from the prefab targeting controller.");
                Assert.NotNull(attackController.ProjectilePrefab, "TowerAttackController must reference a projectile prefab.");
                Assert.IsInstanceOf<ProjectileRuntime>(attackController.ProjectilePrefab, "TowerAttackController projectile prefab must be a ProjectileRuntime.");
                Assert.NotNull(firePoint, "Tower prefab must include a FirePoint child.");
                Assert.AreSame(firePoint, attackController.FirePoint, "TowerAttackController should spawn projectiles from the FirePoint child.");
                Assert.That(attackController.Damage, Is.GreaterThan(0), "Base arrow tower damage must be above zero.");
                Assert.That(attackController.CooldownSeconds, Is.GreaterThan(0f), "Base arrow tower cooldown must be above zero.");
                Assert.That(attackController.ProjectileSpeed, Is.GreaterThan(0f), "Base arrow tower projectile speed must be above zero.");
                Assert.That(attackController.ProjectileLifetime, Is.GreaterThan(0f), "Base arrow tower projectile lifetime must be above zero.");
                Assert.That(attackController.ProjectileVisualAngleOffsetDegrees, Is.EqualTo(-45f), "Base arrow tower should offset the diagonal arrow sprite by -45 degrees.");

                var enemiesLayer = LayerMask.NameToLayer("Enemies");
                Assert.That(enemiesLayer, Is.GreaterThanOrEqualTo(0), "Project must define the Enemies layer.");
                Assert.That(
                    attackController.TargetLayerMask.value & (1 << enemiesLayer),
                    Is.Not.Zero,
                    "Base arrow tower attack target mask must include the Enemies layer.");
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

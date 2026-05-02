using System.Reflection;
using Castlebound.Gameplay.Projectile;
using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Tower
{
    public class TowerAttackControllerTests
    {
        [Test]
        public void TryFire_ReturnsNull_WhenNoTarget()
        {
            var fixture = CreateFixture();

            try
            {
                var projectile = fixture.AttackController.TryFire(0f);

                Assert.IsNull(projectile, "Tower should not fire without an acquired target.");
                Assert.That(fixture.FiredCount, Is.EqualTo(0), "Tower should not raise Fired without a projectile.");
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void TryFire_SpawnsProjectile_WhenTargetAvailableAndCooldownReady()
        {
            var fixture = CreateFixture();
            var target = CreateTarget("Enemy", new Vector2(2f, 0f));

            try
            {
                fixture.TargetingController.AcquireTargetNow();

                var projectile = fixture.AttackController.TryFire(0f);

                Assert.NotNull(projectile, "Tower should spawn a projectile when a target is available.");
                Assert.AreSame(projectile, fixture.LastFiredProjectile, "Fired event should include the spawned projectile.");
                Assert.That(fixture.FiredCount, Is.EqualTo(1));
                Assert.That(projectile.transform.position, Is.EqualTo(fixture.FirePoint.position), "Projectile should launch from the fire point.");
            }
            finally
            {
                Object.DestroyImmediate(target);
                fixture.Destroy();
            }
        }

        [Test]
        public void TryFire_RespectsCooldown()
        {
            var fixture = CreateFixture();
            var target = CreateTarget("Enemy", new Vector2(2f, 0f));

            try
            {
                fixture.TargetingController.AcquireTargetNow();

                var first = fixture.AttackController.TryFire(0f);
                var second = fixture.AttackController.TryFire(0.5f);
                var third = fixture.AttackController.TryFire(1f);

                Assert.NotNull(first, "Precondition: first shot should fire.");
                Assert.IsNull(second, "Tower should not fire again before cooldown completes.");
                Assert.NotNull(third, "Tower should fire again once cooldown completes.");
                Assert.That(fixture.FiredCount, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(target);
                fixture.Destroy();
            }
        }

        [Test]
        public void TryFire_LaunchesProjectileWithTowerAttackStats()
        {
            var fixture = CreateFixture();
            var target = CreateTarget("Enemy", new Vector2(0f, 3f));

            try
            {
                fixture.TargetingController.AcquireTargetNow();

                var projectile = fixture.AttackController.TryFire(0f);

                Assert.NotNull(projectile, "Precondition: tower should fire at the acquired target.");
                Assert.AreSame(fixture.Tower.transform, GetPrivate<Transform>(projectile, "owner"));
                Assert.That(GetPrivate<Vector2>(projectile, "direction"), Is.EqualTo(Vector2.up));
                Assert.That(GetPrivate<float>(projectile, "speed"), Is.EqualTo(fixture.AttackController.ProjectileSpeed));
                Assert.That(GetPrivate<int>(projectile, "damage"), Is.EqualTo(fixture.AttackController.Damage));
                Assert.That(GetPrivate<float>(projectile, "remainingLifetime"), Is.EqualTo(fixture.AttackController.ProjectileLifetime));
                Assert.That(GetPrivate<LayerMask>(projectile, "targetLayerMask").value, Is.EqualTo(fixture.AttackController.TargetLayerMask.value));
            }
            finally
            {
                Object.DestroyImmediate(target);
                fixture.Destroy();
            }
        }

        [Test]
        public void TryFire_RotatesProjectileToLaunchDirection_WithVisualOffset()
        {
            var fixture = CreateFixture();
            var target = CreateTarget("Enemy", new Vector2(0f, 3f));

            try
            {
                fixture.TargetingController.AcquireTargetNow();

                var projectile = fixture.AttackController.TryFire(0f);

                Assert.NotNull(projectile, "Precondition: tower should fire at the acquired target.");
                Assert.That(projectile.transform.eulerAngles.z, Is.EqualTo(45f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(target);
                fixture.Destroy();
            }
        }

        private static TowerAttackFixture CreateFixture()
        {
            var tower = new GameObject("Tower");
            var firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(tower.transform);
            firePoint.position = Vector3.zero;

            var profile = ScriptableObject.CreateInstance<TowerTargetingProfile>();
            profile.MinRange = 0f;
            profile.MaxRange = 5f;
            profile.TargetLayers = 1 << EnemyLayer();
            profile.SelectionMode = TowerTargetSelectionMode.Nearest;

            var targeting = tower.AddComponent<TowerTargetingController>();
            targeting.TargetingProfile = profile;

            var attack = tower.AddComponent<TowerAttackController>();
            attack.TargetingController = targeting;
            attack.ProjectilePrefab = CreateProjectilePrefab();
            attack.FirePoint = firePoint;
            attack.Damage = 4;
            attack.CooldownSeconds = 1f;
            attack.ProjectileSpeed = 7f;
            attack.ProjectileLifetime = 2.5f;
            attack.ProjectileVisualAngleOffsetDegrees = -45f;
            attack.TargetLayerMask = 1 << EnemyLayer();

            var fixture = new TowerAttackFixture(tower, firePoint, profile, attack.ProjectilePrefab, targeting, attack);
            attack.Fired += projectile =>
            {
                fixture.FiredCount++;
                fixture.LastFiredProjectile = projectile;
            };

            return fixture;
        }

        private static ProjectileRuntime CreateProjectilePrefab()
        {
            var projectileObject = new GameObject("ProjectilePrefab");
            var collider = projectileObject.AddComponent<PolygonCollider2D>();
            collider.isTrigger = true;
            var body = projectileObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            return projectileObject.AddComponent<ProjectileRuntime>();
        }

        private static GameObject CreateTarget(string name, Vector2 position)
        {
            var target = new GameObject(name);
            target.layer = EnemyLayer();
            target.transform.position = position;
            target.AddComponent<BoxCollider2D>();
            Physics2D.SyncTransforms();
            return target;
        }

        private static int EnemyLayer()
        {
            var layer = LayerMask.NameToLayer("Enemies");
            Assert.That(layer, Is.GreaterThanOrEqualTo(0), "Project must define the Enemies layer.");
            return layer;
        }

        private static T GetPrivate<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"{instance.GetType().Name} should define private field '{fieldName}'.");
            return (T)field.GetValue(instance);
        }

        private sealed class TowerAttackFixture
        {
            public TowerAttackFixture(
                GameObject tower,
                Transform firePoint,
                TowerTargetingProfile profile,
                ProjectileRuntime projectilePrefab,
                TowerTargetingController targetingController,
                TowerAttackController attackController)
            {
                Tower = tower;
                FirePoint = firePoint;
                Profile = profile;
                ProjectilePrefab = projectilePrefab;
                TargetingController = targetingController;
                AttackController = attackController;
            }

            public GameObject Tower { get; }
            public Transform FirePoint { get; }
            public TowerTargetingProfile Profile { get; }
            public ProjectileRuntime ProjectilePrefab { get; }
            public TowerTargetingController TargetingController { get; }
            public TowerAttackController AttackController { get; }
            public int FiredCount { get; set; }
            public ProjectileRuntime LastFiredProjectile { get; set; }

            public void Destroy()
            {
                foreach (var projectile in Object.FindObjectsOfType<ProjectileRuntime>())
                {
                    Object.DestroyImmediate(projectile.gameObject);
                }

                Object.DestroyImmediate(Tower);
                Object.DestroyImmediate(Profile);
            }
        }
    }
}

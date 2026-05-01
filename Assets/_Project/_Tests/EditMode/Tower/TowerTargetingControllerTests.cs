using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Tower
{
    public class TowerTargetingControllerTests
    {
        private const int TargetLayer = 0;

        private GameObject towerObject;
        private TowerTargetingController targetingController;
        private TowerTargetingProfile targetingProfile;

        [SetUp]
        public void SetUp()
        {
            towerObject = new GameObject("Tower");
            targetingController = towerObject.AddComponent<TowerTargetingController>();
            targetingProfile = ScriptableObject.CreateInstance<TowerTargetingProfile>();
            targetingProfile.MinRange = 0f;
            targetingProfile.MaxRange = 5f;
            targetingProfile.ScanInterval = 0.2f;
            targetingProfile.TargetLayers = 1 << TargetLayer;
            targetingProfile.SelectionMode = TowerTargetSelectionMode.Nearest;
            targetingController.TargetingProfile = targetingProfile;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(targetingProfile);
            Object.DestroyImmediate(towerObject);

            foreach (var gameObject in Object.FindObjectsOfType<GameObject>())
            {
                if (gameObject.name.StartsWith("Enemy"))
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        [Test]
        public void AcquireTargetNow_ReturnsNull_WhenNoEnemiesAreInRange()
        {
            var target = targetingController.AcquireTargetNow();

            Assert.IsNull(target);
            Assert.IsNull(targetingController.CurrentTarget);
        }

        [Test]
        public void AcquireTargetNow_IgnoresEnemyInsideMinimumRange()
        {
            targetingProfile.MinRange = 2f;
            targetingProfile.MaxRange = 5f;
            var closeEnemy = CreateEnemy("Enemy_Close", new Vector2(1f, 0f));
            CreateEnemy("Enemy_Valid", new Vector2(3f, 0f));
            Physics2D.SyncTransforms();

            var target = targetingController.AcquireTargetNow();

            Assert.AreNotSame(closeEnemy.transform, target);
            Assert.That(target.name, Is.EqualTo("Enemy_Valid"));
        }

        [Test]
        public void AcquireTargetNow_IgnoresEnemyOutsideMaximumRange()
        {
            targetingProfile.MaxRange = 3f;
            CreateEnemy("Enemy_Far", new Vector2(4f, 0f));
            Physics2D.SyncTransforms();

            var target = targetingController.AcquireTargetNow();

            Assert.IsNull(target);
            Assert.IsNull(targetingController.CurrentTarget);
        }

        [Test]
        public void AcquireTargetNow_SelectsNearestValidEnemy_WhenConfiguredForNearest()
        {
            targetingProfile.SelectionMode = TowerTargetSelectionMode.Nearest;
            CreateEnemy("Enemy_Farther", new Vector2(4f, 0f));
            var nearestEnemy = CreateEnemy("Enemy_Nearest", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();

            var target = targetingController.AcquireTargetNow();

            Assert.AreSame(nearestEnemy.transform, target);
            Assert.AreSame(nearestEnemy.transform, targetingController.CurrentTarget);
        }

        [Test]
        public void AcquireTargetNow_SelectsFarthestValidEnemy_WhenConfiguredForFarthest()
        {
            targetingProfile.SelectionMode = TowerTargetSelectionMode.Farthest;
            CreateEnemy("Enemy_Nearer", new Vector2(2f, 0f));
            var farthestEnemy = CreateEnemy("Enemy_Farthest", new Vector2(4f, 0f));
            Physics2D.SyncTransforms();

            var target = targetingController.AcquireTargetNow();

            Assert.AreSame(farthestEnemy.transform, target);
            Assert.AreSame(farthestEnemy.transform, targetingController.CurrentTarget);
        }

        [Test]
        public void AcquireTargetNow_ClearsCurrentTarget_WhenTargetLeavesRange()
        {
            targetingProfile.MaxRange = 3f;
            var enemy = CreateEnemy("Enemy_Target", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();

            targetingController.AcquireTargetNow();
            enemy.transform.position = new Vector2(4f, 0f);
            Physics2D.SyncTransforms();

            var target = targetingController.AcquireTargetNow();

            Assert.IsNull(target);
            Assert.IsNull(targetingController.CurrentTarget);
        }

        private static GameObject CreateEnemy(string objectName, Vector2 position)
        {
            var enemy = new GameObject(objectName)
            {
                layer = TargetLayer
            };
            enemy.transform.position = position;
            enemy.AddComponent<CircleCollider2D>();
            return enemy;
        }
    }
}

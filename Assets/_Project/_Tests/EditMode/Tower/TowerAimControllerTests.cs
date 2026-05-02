using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Tower
{
    public class TowerAimControllerTests
    {
        private const int TargetLayer = 0;

        private GameObject towerObject;
        private Transform aimPivot;
        private TowerTargetingController targetingController;
        private TowerAimController aimController;
        private TowerTargetingProfile targetingProfile;

        [SetUp]
        public void SetUp()
        {
            towerObject = new GameObject("Tower");
            aimPivot = new GameObject("AimPivot").transform;
            aimPivot.SetParent(towerObject.transform);

            targetingController = towerObject.AddComponent<TowerTargetingController>();
            targetingProfile = ScriptableObject.CreateInstance<TowerTargetingProfile>();
            targetingProfile.MaxRange = 5f;
            targetingProfile.TargetLayers = 1 << TargetLayer;
            targetingController.TargetingProfile = targetingProfile;

            aimController = towerObject.AddComponent<TowerAimController>();
            aimController.TargetingController = targetingController;
            aimController.AimPivot = aimPivot;
            aimController.AimMode = TowerAimMode.Instant;
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
        public void ApplyAimNow_RotatesAimPivotTowardCurrentTarget_WhenEnabled()
        {
            CreateEnemy("Enemy_Right", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();
            targetingController.AcquireTargetNow();

            aimController.ApplyAimNow(0f);

            Assert.That(Mathf.DeltaAngle(aimPivot.eulerAngles.z, -90f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_DoesNotRotateAimPivot_WhenAimIsDisabled()
        {
            aimController.AimEnabled = false;
            CreateEnemy("Enemy_Right", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();
            targetingController.AcquireTargetNow();

            aimController.ApplyAimNow(0f);

            Assert.That(Mathf.DeltaAngle(aimPivot.eulerAngles.z, 0f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_DoesNothingSafely_WhenNoTargetExists()
        {
            aimPivot.rotation = Quaternion.Euler(0f, 0f, 35f);

            aimController.ApplyAimNow(0f);

            Assert.That(Mathf.DeltaAngle(aimPivot.eulerAngles.z, 35f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_ReturnsTowardIdle_WhenNoTargetExistsAndIdleReturnEnabled()
        {
            aimController.ReturnToIdleWhenNoTarget = true;
            aimController.IdleLocalAngleDegrees = 0f;
            aimController.IdleReturnSpeedDegrees = 90f;
            aimPivot.localRotation = Quaternion.Euler(0f, 0f, 90f);

            aimController.ApplyAimNow(0.5f);

            Assert.That(Mathf.DeltaAngle(aimPivot.localEulerAngles.z, 45f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_DoesNotReturnToIdle_WhenIdleReturnIsDisabled()
        {
            aimController.ReturnToIdleWhenNoTarget = false;
            aimPivot.localRotation = Quaternion.Euler(0f, 0f, 90f);

            aimController.ApplyAimNow(0.5f);

            Assert.That(Mathf.DeltaAngle(aimPivot.localEulerAngles.z, 90f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_TracksMovingCurrentTarget_WithoutReacquiring()
        {
            var enemy = CreateEnemy("Enemy_Moving", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();
            targetingController.AcquireTargetNow();
            aimController.ApplyAimNow(0f);

            enemy.transform.position = new Vector2(0f, 2f);
            Physics2D.SyncTransforms();
            aimController.ApplyAimNow(0f);

            Assert.That(Mathf.DeltaAngle(aimPivot.eulerAngles.z, 0f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_AimsAtCurrentTargetInsteadOfReturningToIdle()
        {
            aimController.ReturnToIdleWhenNoTarget = true;
            aimController.IdleLocalAngleDegrees = 0f;
            aimController.IdleReturnSpeedDegrees = 90f;
            aimPivot.localRotation = Quaternion.Euler(0f, 0f, 90f);
            CreateEnemy("Enemy_Right", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();
            targetingController.AcquireTargetNow();

            aimController.ApplyAimNow(0.5f);

            Assert.That(Mathf.DeltaAngle(aimPivot.eulerAngles.z, -90f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_ReturnsTowardIdle_WhenCurrentTargetIsDestroyed()
        {
            aimController.ReturnToIdleWhenNoTarget = true;
            aimController.IdleLocalAngleDegrees = 0f;
            aimController.IdleReturnSpeedDegrees = 90f;
            aimPivot.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var enemy = CreateEnemy("Enemy_Destroyed", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();
            targetingController.AcquireTargetNow();
            Object.DestroyImmediate(enemy);

            aimController.ApplyAimNow(0.5f);

            Assert.That(Mathf.DeltaAngle(aimPivot.localEulerAngles.z, 45f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void ApplyAimNow_RotatesTowardTarget_WhenConfiguredForSmoothAim()
        {
            aimController.AimMode = TowerAimMode.RotateToward;
            aimController.RotationSpeedDegrees = 90f;
            CreateEnemy("Enemy_Right", new Vector2(2f, 0f));
            Physics2D.SyncTransforms();
            targetingController.AcquireTargetNow();

            aimController.ApplyAimNow(0.5f);

            Assert.That(Mathf.DeltaAngle(aimPivot.eulerAngles.z, -45f), Is.EqualTo(0f).Within(0.01f));
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

using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyEngagementTests
    {
        [Test]
        public void SurfaceDistance_UsesBothColliderBoundaries()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("Target");

            try
            {
                var enemyCollider = enemy.AddComponent<CircleCollider2D>();
                enemyCollider.radius = 1f;
                var targetCollider = target.AddComponent<CircleCollider2D>();
                targetCollider.radius = 1f;
                target.transform.position = Vector2.right * 3f;
                Physics2D.SyncTransforms();

                float distance = EnemyEngagement.GetSurfaceDistance(
                    enemyCollider,
                    new[] { targetCollider },
                    enemy.transform.position,
                    target.transform.position);

                Assert.That(distance, Is.InRange(0.98f, 1.001f),
                    "Surface separation should account for both collider boundaries.");
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void SurfaceDistance_UsesNearestEnabledTargetCollider()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("Target");
            var nearChild = new GameObject("Near");
            var farChild = new GameObject("Far");

            try
            {
                nearChild.transform.SetParent(target.transform);
                farChild.transform.SetParent(target.transform);
                var enemyCollider = enemy.AddComponent<CircleCollider2D>();
                enemyCollider.radius = 0.5f;
                var nearCollider = nearChild.AddComponent<BoxCollider2D>();
                nearCollider.size = Vector2.one;
                var farCollider = farChild.AddComponent<BoxCollider2D>();
                farCollider.size = Vector2.one;
                nearChild.transform.position = Vector2.right * 2f;
                farChild.transform.position = Vector2.right * 5f;
                Physics2D.SyncTransforms();

                float distance = EnemyEngagement.GetSurfaceDistance(
                    enemyCollider,
                    new[] { farCollider, nearCollider },
                    enemy.transform.position,
                    target.transform.position);

                Assert.That(distance, Is.InRange(0.98f, 1.001f),
                    "Unity may subtract its polygon contact offset from circle-to-box separation.");
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void SurfaceDistance_IgnoresCloserTriggerGeometry()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("Target");

            try
            {
                var enemyCollider = enemy.AddComponent<CircleCollider2D>();
                enemyCollider.radius = 0.5f;
                var solidCollider = target.AddComponent<BoxCollider2D>();
                solidCollider.size = Vector2.one;
                target.transform.position = Vector2.right * 2f;

                var triggerChild = new GameObject("Trigger");
                triggerChild.transform.SetParent(target.transform);
                triggerChild.transform.localPosition = Vector2.left;
                var triggerCollider = triggerChild.AddComponent<BoxCollider2D>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = Vector2.one;
                Physics2D.SyncTransforms();

                float distance = EnemyEngagement.GetSurfaceDistance(
                    enemyCollider,
                    new[] { triggerCollider, solidCollider },
                    enemy.transform.position,
                    target.transform.position);

                Assert.That(distance, Is.InRange(0.98f, 1.001f),
                    "Trigger geometry must be ignored while allowing Unity's polygon contact offset.");
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(target);
            }
        }

        [TestCase(false, 0.5f, 0.5f, 0.25f, true)]
        [TestCase(false, 0f, 5f, 0.25f, true)]
        [TestCase(false, 0.51f, 0.5f, 0.25f, false)]
        [TestCase(true, 0.74f, 0.5f, 0.25f, true)]
        [TestCase(true, 0.75f, 0.5f, 0.25f, false)]
        public void ShouldHold_UsesOneEntryDistanceAndReleaseMargin(
            bool currentlyHolding,
            float surfaceDistance,
            float engagementDistance,
            float releaseMargin,
            bool expected)
        {
            Assert.That(
                EnemyEngagement.ShouldHold(
                    currentlyHolding,
                    surfaceDistance,
                    engagementDistance,
                    releaseMargin,
                    targetBroken: false),
                Is.EqualTo(expected));
        }

        [Test]
        public void ShouldHold_BrokenTargetAlwaysReleases()
        {
            Assert.IsFalse(EnemyEngagement.ShouldHold(
                currentlyHolding: true,
                surfaceDistance: 0f,
                engagementDistance: 0.5f,
                releaseMargin: 0.25f,
                targetBroken: true));
        }
    }
}

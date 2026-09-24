using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyNavigationGoalTests
    {
        [TestCase(EnemyTargetType.Player)] [TestCase(EnemyTargetType.Barrier)]
        public void ApproachCandidates_AreDeterministicAndWithinExistingSurfaceRange(EnemyTargetType type)
        {
            using (var world = new StaticNavigationTestWorld())
            {
                var target = new GameObject("Goal", typeof(BoxCollider2D));
                try
                {
                    var collider = target.GetComponent<BoxCollider2D>(); collider.size = Vector2.one * 3f;
                    Physics2D.SyncTransforms();
                    var descriptor = new EnemyNavigationTarget(target.transform, type, new[] { collider }, 0.5f,
                        anchor: Vector2.left * 3f, hasAnchor: true);
                    var first = new List<Vector2>(); var second = new List<Vector2>();
                    EnemyNavigationGoals.Build(descriptor, Vector2.left * 5f, world, first);
                    EnemyNavigationGoals.Build(descriptor, Vector2.left * 5f, world, second);
                    CollectionAssert.AreEqual(first, second); Assert.That(first.Count, Is.GreaterThan(1));
                    int begin = type == EnemyTargetType.Player ? 1 : 0;
                    for (int i = begin; i < first.Count; i++)
                        Assert.IsTrue(EnemyNavigationGoals.InAttackBand(descriptor, first[i], world.BodyRadius));
                    if (type == EnemyTargetType.Barrier)
                    { Assert.That(first[0].x, Is.LessThan(0f)); CollectionAssert.DoesNotContain(first, Vector2.zero); }
                }
                finally { Object.DestroyImmediate(target); }
            }
        }
        [Test] public void BrokenBarrier_UsesOpeningWithoutChangingSelectedTarget()
        {
            using (var world = new StaticNavigationTestWorld())
            {
                var target = new GameObject("Opening");
                try
                {
                    var goals = new List<Vector2>();
                    var descriptor = new EnemyNavigationTarget(target.transform, EnemyTargetType.Barrier, null, 0.5f, passage: true);
                    EnemyNavigationGoals.Build(descriptor, Vector2.left * 5f, world, goals);
                    Assert.That(goals, Is.EqualTo(new[] { Vector2.zero }));
                }
                finally { Object.DestroyImmediate(target); }
            }
        }
        [Test] public void PlayerApproach_SeparatingWallIsRejectedEvenWhenBodyPositionIsClear()
        {
            Vector2 origin = new Vector2(10000f, 10000f);
            var runtime = new StaticNavigationTestWorld();
            var sampling = new StaticNavigationWorld2D(runtime.BodyRadius);
            runtime.PointQuery = sampling.SamplePoint; runtime.SightQuery = sampling.SampleSight;
            var player = new GameObject("Player"); player.tag = "Player";
            var wall = new GameObject("Separating wall", typeof(BoxCollider2D));
            try
            {
                player.transform.position = origin + Vector2.right * 0.2f;
                wall.transform.position = origin;
                wall.GetComponent<BoxCollider2D>().size = new Vector2(0.1f, 4f);
                Physics2D.SyncTransforms();
                Vector2 candidate = origin + Vector2.left * 1.1f;
                Assert.That(runtime.Point(candidate), Is.EqualTo(StaticNavigationSampleState.Clear));
                var descriptor = new EnemyNavigationTarget(player.transform, EnemyTargetType.Player, null, 0.5f);
                Assert.That(EnemyNavigationGoals.Validate(descriptor, candidate, runtime), Is.EqualTo(StaticNavigationSampleState.Blocked));
            }
            finally { Object.DestroyImmediate(player); Object.DestroyImmediate(wall); runtime.Dispose(); }
        }
    }
}

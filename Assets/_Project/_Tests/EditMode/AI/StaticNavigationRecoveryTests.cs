using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationRecoveryTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly Vector2 origin = new Vector2(500f, 500f);
        private StaticNavigationWorld2D world;
        private StaticNavigationRecoveryQueries queries;
        private CircleCollider2D body;
        private EnemyNavigationRecovery recovery;

        [SetUp] public void SetUp()
        {
            world = new StaticNavigationWorld2D(0.87684506f);
            queries = new StaticNavigationRecoveryQueries(world);
            recovery = new EnemyNavigationRecovery();
            var go = Make("Enemy body", new Vector2(1.2f, 0f)); go.tag = "Enemy";
            body = go.AddComponent<CircleCollider2D>(); body.radius = world.BodyRadius;
            Box(Vector2.zero, new Vector2(1f, 4f));
            Physics2D.SyncTransforms();
        }
        [TearDown] public void TearDown()
        {
            queries.Dispose();
            foreach (var go in objects) Object.DestroyImmediate(go);
            objects.Clear(); Physics2D.SyncTransforms();
        }
        private GameObject Make(string name, Vector2 position)
        { var go = new GameObject(name); objects.Add(go); go.transform.position = origin + position; return go; }
        private BoxCollider2D Box(Vector2 position, Vector2 size)
        { var box = Make("Static blocker", position).AddComponent<BoxCollider2D>(); box.size = size; return box; }
        private void Move(Vector2 position) { body.transform.position = origin + position; Physics2D.SyncTransforms(); }
        private Vector2 Select()
        {
            for (int i = 0; i < 64 && !recovery.HasDestination; i++)
                recovery.Compute(queries, body, 2f, 0.02f, i * 0.02f, out _);
            Assert.IsTrue(recovery.HasDestination, "Expected a local safe candidate.");
            return recovery.Destination;
        }

        [TestCase(0f)] [TestCase(-0.0005f)]
        public void ShallowContact_DoesNotStartRecovery(float gap)
        {
            Move(new Vector2(0.5f + world.BodyRadius + gap, 0f));
            Assert.That(queries.Detect(body, out _), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.IsFalse(recovery.Compute(queries, body, 2f, 0.02f, 0f, out _));
            Assert.IsFalse(recovery.IsActive);
        }
        [Test] public void GenuinePenetration_SelectsNearestOutwardCandidateDeterministically()
        {
            Assert.That(queries.Detect(body, out var normal), Is.EqualTo(StaticNavigationSampleState.Blocked));
            Assert.That(Vector2.Dot(normal, Vector2.right), Is.GreaterThan(0.99f));
            Vector2 start = body.bounds.center, selected = Select();
            Assert.That(Vector2.Distance(start, selected), Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(selected.y, Is.EqualTo(start.y).Within(0.0001f));
            Assert.That(queries.Destination(body, selected), Is.EqualTo(StaticNavigationSampleState.Clear));
            for (int i = 0; i < 48; i++)
                Assert.That(queries.Destination(body, EnemyNavigationRecovery.Candidate(start, normal, i)),
                    Is.EqualTo(StaticNavigationSampleState.Blocked), "All nearer rings are invalid.");
            recovery.Reset(); Assert.That(Select(), Is.EqualTo(selected));
        }
        [Test] public void BodyOffset_IsMeasuredAtActualColliderCenter()
        {
            body.offset = new Vector2(0.2f, 0f); Move(new Vector2(1f, 0f));
            Vector2 selected = Select();
            Assert.That(selected.x - origin.x, Is.EqualTo(1.45f).Within(0.0001f));
        }
        [Test] public void CenterInsideBox_CanExitNearestFaceWithinLocalBound()
        {
            Move(new Vector2(0.45f, 0f));
            Assert.That(queries.Detect(body, out _), Is.EqualTo(StaticNavigationSampleState.Blocked));
            Vector2 goal = Select();
            Assert.That(goal.x, Is.GreaterThan(origin.x + 1.4f));
            Assert.That(Vector2.Distance(body.bounds.center, goal), Is.LessThanOrEqualTo(1.0001f));
        }
        [Test] public void BlockedStraightExit_SelectsTangentialOutwardEscape()
        {
            Box(new Vector2(2.4f, 0f), new Vector2(0.2f, 0.2f)); Physics2D.SyncTransforms();
            Assert.That(queries.Destination(body, origin + new Vector2(1.45f, 0f)), Is.EqualTo(StaticNavigationSampleState.Blocked));
            Vector2 goal = Select();
            Assert.That(Mathf.Abs(goal.y - origin.y), Is.GreaterThan(0.2f));
            Assert.That(queries.Sweep(body, goal), Is.EqualTo(StaticNavigationSampleState.Clear));
        }
        [Test] public void CornerWithTwoInitialOverlaps_OnlyAllowsOutwardMotionFromBoth()
        {
            Box(Vector2.zero, new Vector2(4f, 1f)); Move(new Vector2(1.2f, 1.2f));
            Assert.That(queries.Sweep(body, origin + new Vector2(1.6f, 0.9f)), Is.EqualTo(StaticNavigationSampleState.Blocked));
            Vector2 goal = Select();
            Assert.That(goal.x, Is.GreaterThan(origin.x + 1.2f));
            Assert.That(goal.y, Is.GreaterThan(origin.y + 1.2f));
        }
        [Test] public void SeparateThinWall_BlocksWholeSweepEvenWithClearDestination()
        {
            queries.Dispose(); world = new StaticNavigationWorld2D(0.2f);
            queries = new StaticNavigationRecoveryQueries(world); body.radius = 0.2f;
            Box(new Vector2(0.95f, 0f), new Vector2(0.02f, 0.2f)); Move(new Vector2(0.6f, 0f));
            Vector2 goal = origin + new Vector2(1.25f, 0f);
            Assert.That(queries.Destination(body, goal), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(queries.Sweep(body, goal), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }
        [Test] public void InitialThinObstacle_CannotBeIgnoredToEscapeThroughItsOppositeFace()
        {
            queries.Dispose(); world = new StaticNavigationWorld2D(0.2f);
            queries = new StaticNavigationRecoveryQueries(world); body.radius = 0.2f;
            objects[1].GetComponent<BoxCollider2D>().size = new Vector2(0.1f, 4f);
            Move(new Vector2(0.1f, 0f));
            Vector2 opposite = origin + new Vector2(-0.4f, 0f);
            Assert.That(queries.Destination(body, opposite), Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(queries.Sweep(body, opposite), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }
        [TestCase("Player")] [TestCase("Enemy")]
        public void Recovery_DoesNotCrossActors(string tag)
        {
            var actor = Make("Other actor", new Vector2(2.4f, 0f)); actor.tag = tag;
            actor.AddComponent<CircleCollider2D>().radius = 0.1f; Physics2D.SyncTransforms();
            Assert.That(queries.Sweep(body, origin + new Vector2(1.7f, 0f)), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }
        [Test] public void EmbeddedCenter_NoSafeLocalCandidateWaitsWithoutMovingOrOscillating()
        {
            Box(Vector2.zero, new Vector2(4f, 4f)); Move(Vector2.zero);
            for (int i = 0; i < 64; i++)
            {
                Assert.IsTrue(recovery.Compute(queries, body, 2f, 0.02f, i * 0.02f, out var velocity));
                Assert.That(velocity, Is.EqualTo(Vector2.zero));
            }
            Assert.IsTrue(recovery.IsActive); Assert.IsFalse(recovery.HasDestination);
            float retryAt = recovery.RetryAt;
            Assert.That(retryAt, Is.GreaterThan(1.26f));
            recovery.Compute(queries, body, 2f, 0.02f, retryAt - 0.01f, out var waiting);
            Assert.That(waiting, Is.EqualTo(Vector2.zero)); Assert.That(recovery.CandidateIndex, Is.Zero);
            Assert.That(recovery.Constrain(queries, body, Vector2.right * 0.04f), Is.EqualTo(Vector2.zero));
        }
        [Test] public void ExhaustedQueryBudget_WaitsWithoutSkippingCandidates()
        {
            recovery.Compute(queries, body, 2f, 0.02f, 0f, out var velocity, () => false);
            Assert.That(velocity, Is.EqualTo(Vector2.zero)); Assert.That(recovery.CandidateIndex, Is.Zero);
            Assert.IsFalse(recovery.HasDestination);
        }
        [Test] public void SaturatedOverlapBuffer_IsUnknownRatherThanAnEscapePermission()
        {
            using (var small = new StaticNavigationRecoveryQueries(world, 1))
            {
                Assert.That(small.Detect(body, out _), Is.EqualTo(StaticNavigationSampleState.Unknown));
                Assert.That(small.Sweep(body, origin + new Vector2(1.6f, 0f)), Is.EqualTo(StaticNavigationSampleState.Unknown));
            }
        }
        [Test] public void CompoundOrConcaveInitialCollider_FailsClosed()
        {
            var polygon = Make("Concave blocker", new Vector2(1.2f, 0f)).AddComponent<PolygonCollider2D>();
            polygon.points = new[] { new Vector2(-0.1f, -0.2f), new Vector2(0.1f, -0.2f),
                new Vector2(0.1f, 0f), Vector2.zero, new Vector2(0f, 0.2f), new Vector2(-0.1f, 0.2f) };
            Physics2D.SyncTransforms();
            Assert.That(queries.Sweep(body, origin + new Vector2(1.6f, 0f)), Is.EqualTo(StaticNavigationSampleState.Blocked));
        }
        [Test] public void ActiveKnockback_PausesRecoveryMotion()
        {
            Assert.IsTrue(recovery.Compute(queries, body, 2f, 0.02f, 0f, out var velocity, paused: true));
            Assert.IsTrue(recovery.IsActive); Assert.That(velocity, Is.EqualTo(Vector2.zero));
            Assert.IsFalse(recovery.HasDestination);
        }
        [Test] public void RecoveryStep_RespectsSpeedRechecksActorsAndResumesOnlyAtNormalClearance()
        {
            Vector2 goal = Select();
            recovery.Compute(queries, body, 2f, 0.02f, 0.5f, out var velocity);
            Assert.That(velocity.magnitude, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(recovery.Constrain(queries, body, velocity * 0.02f).magnitude, Is.EqualTo(0.04f).Within(0.0001f));
            Assert.That(recovery.Constrain(queries, body, Vector2.left * 0.04f), Is.EqualTo(Vector2.zero));
            var actor = Make("Arriving player", new Vector2(2.1f, 0f)); actor.tag = "Player";
            actor.AddComponent<CircleCollider2D>().radius = 0.1f; Physics2D.SyncTransforms();
            Assert.That(recovery.Constrain(queries, body, velocity * 0.02f), Is.EqualTo(Vector2.zero));
            actor.SetActive(false); Move(goal - origin);
            Assert.IsFalse(recovery.Compute(queries, body, 2f, 0.02f, 0.6f, out _));
            Assert.IsFalse(recovery.IsActive);
        }
    }
}

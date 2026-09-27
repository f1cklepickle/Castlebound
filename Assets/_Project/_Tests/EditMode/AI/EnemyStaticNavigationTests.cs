using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyStaticNavigationTests
    {
        private StaticNavigationTestWorld world;
        private EnemyStaticNavigation nav;
        private GameObject target;
        private readonly Vector2 start = new Vector2(0.125f, 0.125f);
        [SetUp] public void SetUp()
        {
            world = new StaticNavigationTestWorld(); nav = new EnemyStaticNavigation(world);
            target = new GameObject("Target"); target.transform.position = new Vector2(2.125f, 0.125f);
        }
        [TearDown] public void TearDown() { nav.Dispose(); world.Dispose(); Object.DestroyImmediate(target); }
        private EnemyNavigationTarget Target => new EnemyNavigationTarget(target.transform, EnemyTargetType.Player, null, 0.5f);
        private Vector2 Step(float now = 0f, Vector2? position = null, bool suspended = false, bool knockback = false)
            => nav.Compute(Target, position ?? start, 2f, 0.1f, now, suspended, knockback);
        private void BlockTarget(float visibleBeforeX = 1f)
        {
            world.SegmentQuery = (a, b) => (a - b).sqrMagnitude < 0.0001f || b.x < visibleBeforeX
                ? StaticNavigationSampleState.Clear : StaticNavigationSampleState.Blocked;
        }
        private void Submit()
        {
            BlockTarget();
            for (int i = 0; i < 6 && world.Submissions == 0; i++) { Step(); world.Cache.SamplePending(128); }
            Assert.That(world.Submissions, Is.EqualTo(1));
        }
        [Test] public void DirectClear_DoesNotSubmitAndPreservesAuthoredSpeed()
        { Assert.That(Step().magnitude, Is.EqualTo(2f).Within(0.0001f)); Assert.That(world.Submissions, Is.Zero); }
        [Test] public void Blocked_WaitsSafelyAndNeverDuplicatesPendingRequest()
        {
            Submit(); for (int i = 0; i < 40; i++) Assert.That(Step(i), Is.EqualTo(Vector2.zero));
            Assert.That(world.Submissions, Is.EqualTo(1));
        }
        [Test] public void DirectClear_DropsRouteAndIgnoresLateResult()
        {
            Submit(); world.SegmentQuery = null; Step(); Assert.That(nav.State, Is.EqualTo(EnemyNavigationState.Direct));
            world.Complete(); Step(); Assert.That(nav.RouteCount, Is.Zero); Assert.That(world.Submissions, Is.EqualTo(1));
        }
        [Test] public void RouteAcceptance_SkipsToFurthestVisibleWaypointMonotonically()
        {
            Submit(); world.Complete();
            BlockTarget(2f);
            Assert.That(Step(1f).magnitude, Is.GreaterThan(0f));
            int index = nav.WaypointIndex; Assert.That(index, Is.GreaterThan(0));
            Step(1.1f); Assert.That(nav.WaypointIndex, Is.GreaterThanOrEqualTo(index));
        }
        [Test] public void RecoveryReset_DiscardsRouteAndRequestsFreshNavigation()
        {
            Submit(); world.Complete(); Step(1f);
            Assert.That(nav.RouteCount, Is.GreaterThan(0));
            nav.ResetAfterRecovery();
            Assert.That(nav.RouteCount, Is.Zero); Assert.That(nav.RequestId, Is.Zero);
            Step(2f); Assert.That(world.Submissions, Is.EqualTo(2));
        }
        [Test] public void TargetChange_DiscardsOldResultBeforeNewSubmission()
        {
            Submit(); target.transform.position += Vector3.right * 2f;
            Step(1f); Assert.That(world.Submissions, Is.EqualTo(1));
            world.Complete(); Step(1.1f); Assert.That(nav.RouteCount, Is.Zero);
        }
        [Test] public void DestroyedTarget_ResetsWithoutMovement()
        {
            Submit(); var descriptor = Target; Object.DestroyImmediate(target);
            Assert.That(nav.Compute(descriptor, start, 2f, 0.1f, 1f), Is.EqualTo(Vector2.zero));
            Assert.That(nav.State, Is.EqualTo(EnemyNavigationState.Suspended));
        }
        [TestCase("root")][TestCase("stagger")][TestCase("hold")]
        public void MovementLock_SuspendsAndRejectsPriorResult(string reason)
        {
            Submit(); Assert.That(Step(20f, suspended: true), Is.EqualTo(Vector2.zero));
            world.Complete(); Step(21f, suspended: true);
            Assert.That(nav.RouteCount, Is.Zero); Assert.That(world.Submissions, Is.EqualTo(1));
        }
        [Test] public void FailedRegion_UsesBackoffThenWiderBoundsWithoutGlobalFailure()
        {
            Submit(); world.Complete(false); Step(1f);
            Assert.That(nav.State, Is.EqualTo(EnemyNavigationState.RegionFailed));
            Step(2.9f); Assert.That(world.Submissions, Is.EqualTo(1));
            Step(3f); Assert.That(world.Submissions, Is.EqualTo(2)); Assert.That(world.LastAttempt, Is.EqualTo(1));
            world.Complete(false); Step(4f); Step(6f); Assert.That(world.LastAttempt, Is.EqualTo(2));
        }
        [Test] public void WorldRevision_RejectsStaleResult()
        {
            Submit(); world.Complete(); world.Cache.InvalidateWorldBounds(new Rect(start, Vector2.one), new Rect(start, Vector2.one));
            Step(1f); Assert.That(nav.RouteCount, Is.Zero);
        }
        [Test] public void StationaryFollowing_ReplansAfterGraceButNotWhileKnockbackActive()
        {
            Submit(); world.Complete();
            BlockTarget(2f);
            // Keep the goal connector valid while blocking direct pursuit from the stationary start.
            Assert.That(world.Segment(world.Cache.CellToWorld(world.LastGoal), nav.PlannedGoal),
                Is.EqualTo(StaticNavigationSampleState.Clear));
            Assert.That(Step(1f).magnitude, Is.GreaterThan(0f));
            Assert.That(nav.State, Is.EqualTo(EnemyNavigationState.Following));
            for (int i = 0; i < 10; i++) Step(1.1f + i * 0.1f);
            Assert.That(world.Submissions, Is.EqualTo(1), "Do not replan before the stall grace expires.");
            for (int i = 0; i < 30; i++) Step(2.1f + i * 0.1f, knockback: true);
            Assert.That(world.Submissions, Is.EqualTo(1), "Knockback must not count as a navigation stall.");
            for (int i = 0; i < 10; i++) Step(5.1f + i * 0.1f);
            Assert.That(world.Submissions, Is.EqualTo(1), "Knockback must reset the stall grace.");
            for (int i = 0; i < 15; i++) Step(6.1f + i * 0.1f);
            Assert.That(world.Submissions, Is.EqualTo(2), "Stationary following must eventually replan.");
        }
        [Test] public void RouteDeviation_ReplansAfterMinimumInterval()
        {
            Submit(); world.Complete(); Step(0.1f);
            Step(0.2f, start + Vector2.up * 3f); Assert.That(world.Submissions, Is.EqualTo(1));
            Step(1f, start + Vector2.up * 3f); Assert.That(world.Submissions, Is.EqualTo(2));
        }
        [Test] public void UnknownDirectClearance_WaitsWithoutInventingMovement()
        {
            world.SegmentQuery = (a, b) => StaticNavigationSampleState.Unknown;
            Assert.That(Step(), Is.EqualTo(Vector2.zero)); Assert.That(world.Submissions, Is.Zero);
        }
        [Test] public void BlockedPlayerCenter_UsesClearApproachInsideExistingReach()
        {
            var collider = target.AddComponent<CircleCollider2D>(); collider.radius = 0.5f; Physics2D.SyncTransforms();
            Vector2 center = target.transform.position;
            world.PointQuery = p => p == center ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear;
            world.SegmentQuery = (a, b) => b == center ? StaticNavigationSampleState.Blocked : StaticNavigationSampleState.Clear;
            var descriptor = new EnemyNavigationTarget(target.transform, EnemyTargetType.Player, new[] { collider }, 0.5f);
            Vector2 velocity = nav.Compute(descriptor, start, 2f, 0.02f, 0f);
            Assert.That(nav.PlannedGoal, Is.Not.EqualTo(center));
            Assert.IsTrue(EnemyNavigationGoals.WithinReach(descriptor, nav.PlannedGoal, world.BodyRadius));
            Assert.That(velocity.magnitude, Is.GreaterThan(0f)); Assert.That(world.Submissions, Is.Zero);
        }
        [Test] public void TargetTypeSwitch_InvalidatesOutstandingPlayerRoute()
        {
            Submit(); var barrier = new EnemyNavigationTarget(target.transform, EnemyTargetType.Barrier, null, 0.5f, passage: true);
            nav.Compute(barrier, start, 2f, 0.1f, 1f); world.Complete();
            nav.Compute(barrier, start, 2f, 0.1f, 1.1f); Assert.That(nav.RouteCount, Is.Zero);
        }
    }
}

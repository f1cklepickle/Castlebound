using Castlebound.Gameplay.AI;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class StaticNavigationRuntimeTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void UnavailableRuntime_DeclinesChaseWithoutChangingFallbackMovement(bool uninitialized)
        {
            var services = Object.FindObjectsOfType<StaticNavigationRuntime>();
            var paused = new System.Collections.Generic.List<StaticNavigationRuntime>();
            foreach (var service in services)
                if (service.enabled) { paused.Add(service); service.enabled = false; }
            var player = new GameObject("FallbackPlayer");
            var enemy = new GameObject("FallbackEnemy");
            GameObject unavailable = null;
            try
            {
                if (uninitialized)
                {
                    unavailable = new GameObject("UninitializedNavigation");
                    var service = unavailable.AddComponent<StaticNavigationRuntime>();
                    // Invoke only this component's setup; Unity message dispatch is not valid in EditMode.
                    typeof(StaticNavigationRuntime).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(service, null);
                    Assert.That(StaticNavigationRuntime.Instance, Is.SameAs(service));
                    Assert.IsNull(service.Cache);
                }
                else Assert.IsNull(StaticNavigationRuntime.Instance);
                player.tag = "Player";
                enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
                enemy.AddComponent<CircleCollider2D>().radius = 0.87684506f;
                enemy.AddComponent<Health>().ConfigureMaxHealth(10, true);
                enemy.AddComponent<EnemySurroundEligibility>().AvoidanceGroup = PredictiveAvoidanceGroup.SmallMelee;
                var controller = enemy.AddComponent<EnemyController2D>();
                controller.Debug_SetupRefs(player.transform);
                controller.Debug_SetTargetDecision(player.transform, player.transform, EnemyTargetType.Player);
                enemy.GetComponent<EnemyLocomotion>().SetMovementState(EnemyController2D.State.CHASE);
                var adapter = enemy.AddComponent<EnemyNavigationChase>();
                typeof(EnemyNavigationChase).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(adapter, null);
                Vector2 radial = Vector2.right * 2f, tangent = Vector2.up;

                Assert.IsFalse(adapter.Apply(2f, 0.02f, ref radial, ref tangent));
                Assert.That(radial, Is.EqualTo(Vector2.right * 2f));
                Assert.That(tangent, Is.EqualTo(Vector2.up));
                Assert.IsFalse(adapter.GuardMovement);
                Assert.IsFalse(adapter.IsRecovering);
                Assert.That(adapter.ConstrainFinalDisplacement(Vector2.right), Is.EqualTo(Vector2.right));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
                Object.DestroyImmediate(player);
                if (unavailable != null) Object.DestroyImmediate(unavailable);
                foreach (var service in paused) if (service != null) service.enabled = true;
            }
        }

        [Test]
        public void ProgressiveBounds_WidenWithoutExceedingFoundationCaps()
        {
            var world = new BoundsInt(-256, -256, 0, 512, 512, 1);
            Assert.IsTrue(StaticNavigationPlanningBounds.TryCreate(world, Vector2Int.zero, new Vector2Int(20, 0), 0, out var narrow));
            Assert.IsTrue(StaticNavigationPlanningBounds.TryCreate(world, Vector2Int.zero, new Vector2Int(20, 0), 1, out var wide));
            Assert.IsTrue(StaticNavigationPlanningBounds.TryCreate(world, Vector2Int.zero, new Vector2Int(20, 0), 2, out var largest));
            Assert.That(narrow.size.y, Is.EqualTo(9));
            Assert.That(wide.size.y, Is.EqualTo(25));
            Assert.That(largest.size.x * largest.size.y, Is.LessThanOrEqualTo(4096));
            Assert.That(largest.size.x, Is.LessThanOrEqualTo(96));
            Assert.That(largest.size.y, Is.LessThanOrEqualTo(96));
            Assert.That(largest.size.x * largest.size.y, Is.GreaterThan(wide.size.x * wide.size.y));
        }

        [Test]
        public void DistantEndpoints_AreRejectedRatherThanTruncatedOrGloballyUnreachable()
        {
            Assert.IsFalse(StaticNavigationPlanningBounds.TryCreate(new BoundsInt(-256, -256, 0, 512, 512, 1),
                Vector2Int.zero, new Vector2Int(100, 0), 2, out _));
        }

        [Test]
        public void Avoidance_StaticGuardRejectsEveryCandidateWithoutRestoringSpeed()
        {
            var solver = new EnemyPredictiveAvoidance();
            Assert.That(solver.SelectVelocity(Vector2.zero, Vector2.right * 3.5f, 0.2f,
                new EnemyAvoidanceNeighbor[0], velocity => false), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Avoidance_ClearGuardPreservesExistingSelection()
        {
            var neighbors = new[] { new EnemyAvoidanceNeighbor(Vector2.right * 1.5f, Vector2.zero, 0.2f) };
            var expected = new EnemyPredictiveAvoidance().SelectVelocity(Vector2.zero, Vector2.right * 3.5f, 0.2f, neighbors);
            Assert.That(new EnemyPredictiveAvoidance().SelectVelocity(Vector2.zero, Vector2.right * 3.5f, 0.2f,
                neighbors, velocity => true), Is.EqualTo(expected));
        }

        [Test]
        public void LiveQueries_EnforceSharedBudgetAndInvalidationDoesNotRefillIt()
        {
            var grid = new GameObject("Grid", typeof(Grid)).GetComponent<Grid>();
            var world = new StaticNavigationWorld2D(0.1f);
            var cache = new StaticNavigationWorldCache(new StaticNavigationLayout(grid, new BoundsInt(-8, -8, 0, 16, 16, 1)), world);
            using (var connector = new StaticNavigationStartConnector(cache, world))
            {
                try
                {
                    var queries = new StaticNavigationRuntimeQueries(world, connector); queries.BeginFrame(0);
                    for (int i = 0; i < 256; i++) queries.Point(new Vector2(10000f + i, 10000f));
                    queries.Invalidate();
                    Assert.That(queries.Point(Vector2.zero), Is.EqualTo(StaticNavigationSampleState.Unknown));
                    Assert.That(queries.Used, Is.EqualTo(256));
                    queries.BeginFrame(1); queries.Point(new Vector2(10000f, 10000f)); Assert.That(queries.Used, Is.EqualTo(1));
                }
                finally { Object.DestroyImmediate(grid.gameObject); }
            }
        }

        [Test]
        public void SeparationProjection_CanInvalidateClearMotion_AndFinalSweepRejectsIt()
        {
            var wall = new GameObject("Static wall", typeof(BoxCollider2D));
            try
            {
                var center = new Vector2(10000f, 10000f);
                wall.transform.position = center + Vector2.down;
                wall.GetComponent<BoxCollider2D>().size = new Vector2(10f, 2f);
                Physics2D.SyncTransforms();
                var world = new StaticNavigationWorld2D(0.1f, 0.02f);
                Vector2 start = center + Vector2.up * 0.14f;
                Vector2 requested = Vector2.right * 0.2f;
                Assert.That(world.IsSegmentClear(start, start + requested), Is.True);
                Vector2 constrained = EnemySeparationMath.ClampInwardDisplacement(Vector2.one.normalized * 0.4f,
                    0.4f, requested, 0.0001f);
                Assert.That(constrained.magnitude, Is.LessThanOrEqualTo(requested.magnitude));
                Assert.That(world.IsSegmentClear(start, start + constrained), Is.False);
            }
            finally { Object.DestroyImmediate(wall); }
        }

        [TestCase("Enemy_Goblin_Melee", PredictiveAvoidanceGroup.SmallMelee)]
        [TestCase("Enemy_Lurker", PredictiveAvoidanceGroup.Lurker)]
        public void MeleePrefab_OptsInWithoutChangingAvoidanceGroup(string name, PredictiveAvoidanceGroup expected)
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/" + name + ".prefab");
            Assert.IsNotNull(prefab.GetComponent<EnemyNavigationChase>());
            Assert.That(prefab.GetComponent<EnemySurroundEligibility>().AvoidanceGroup, Is.EqualTo(expected));
            var ranged = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Goblin_Ranged.prefab");
            Assert.IsNull(ranged.GetComponent<EnemyNavigationChase>());
        }
    }
}

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyStaticNavigationPlayTests
{
    private readonly List<GameObject> objects = new List<GameObject>();
    private readonly List<StaticNavigationRuntime> paused = new List<StaticNavigationRuntime>();
    private StaticNavigationRuntime runtime;
    private Transform player;
    private readonly Vector2 origin = new Vector2(2000f, 2000f);

    [SetUp] public void SetUp()
    {
        foreach (var service in Object.FindObjectsOfType<StaticNavigationRuntime>())
            if (service.enabled) { paused.Add(service); service.enabled = false; }
        var grid = Create("WorldGrid").AddComponent<Grid>(); grid.transform.position = origin;
        runtime = grid.gameObject.AddComponent<StaticNavigationRuntime>();
        runtime.Initialize(grid, new BoundsInt(-64, -64, 0, 128, 128, 1));
        var target = Create("NavigationPlayer"); target.tag = "Player"; target.layer = LayerMask.NameToLayer("Player");
        target.transform.position = origin + Vector2.right * 4f;
        target.AddComponent<CircleCollider2D>().radius = 0.5f;
        player = target.transform;
    }
    [TearDown] public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear();
        foreach (var service in paused) if (service != null) service.enabled = true;
        paused.Clear(); Physics2D.SyncTransforms();
    }
    private GameObject Create(string name) { var go = new GameObject(name); objects.Add(go); return go; }
    private void RequireNavigation(bool condition, string reason, EnemyController2D enemy)
    {
        if (!condition) Assert.Fail(reason + " " + DescribeNavigation(enemy));
    }
    private string DescribeNavigation(EnemyController2D enemy)
    {
        var adapter = enemy.GetComponent<EnemyNavigationChase>();
        var navigation = adapter.Navigation;
        Vector2 center = enemy.GetComponent<CircleCollider2D>().bounds.center;
        // Failure-only queries use a separate sampler so diagnostics do not consume the service budget.
        var sampler = new StaticNavigationWorld2D(runtime.BodyRadius);
        var body = enemy.GetComponent<Rigidbody2D>();
        var contacts = new ContactPoint2D[16];
        int contactCount = body.GetContacts(contacts);
        string contactDetails = "";
        for (int i = 0; i < contactCount; i++)
            contactDetails += $" [{contacts[i].collider?.name}/{contacts[i].otherCollider?.name}: " +
                $"gap={contacts[i].separation}, normal={contacts[i].normal.ToString("F6")}]";
        string route = navigation == null ? "not initialized" :
            $"state={navigation.State}, route={navigation.RouteCount}, waypoint={navigation.WaypointIndex}, " +
            $"request={navigation.RequestId}, attempt={navigation.PlanningAttempt}, " +
            $"goal={navigation.PlannedGoal - origin}, steering={navigation.SteeringPoint - origin}, " +
            $"steeringClear={sampler.IsSegmentClear(center, navigation.SteeringPoint)}";
        return $"position={center - origin}, target={enemy.Target?.name}, targetType={enemy.CurrentTargetType}, " +
            $"movement={enemy.GetComponent<EnemyLocomotion>().CurrentState}, " +
            $"surfaceGap={enemy.GetComponent<EnemyEngagement>().SurfaceDistanceTo(enemy.Target)}, " +
            $"pointClear={sampler.IsPointClear(center)}, {route}, submitted={runtime.SubmittedRequests}, " +
            $"sampling={runtime.SamplingQueriesLastTick}, liveQueries={runtime.LiveQueriesThisFrame}, " +
            $"revision={runtime.Cache.Revision}, expansions={runtime.Scheduler.DebugTotalExpansions}, " +
            $"preferred={adapter.DebugPreferredVelocity.ToString("F6")}, avoidance={adapter.DebugAvoidanceVelocity.ToString("F6")}, " +
            $"requestedStep={adapter.DebugRequestedDisplacement.ToString("F6")}, allowedStep={adapter.DebugAllowedDisplacement.ToString("F6")}, " +
            $"guard={adapter.DebugGuardState}, guardCenter={(adapter.DebugGuardCenter - origin).ToString("F6")}, " +
            $"guardTime={adapter.DebugMovementTime}, fixedTime={Time.fixedTime}, " +
            $"bodyVelocity={body.velocity.ToString("F6")}, " +
            $"freshStepClear={sampler.IsSegmentClear(center, center + adapter.DebugRequestedDisplacement)}, " +
            $"contacts={contactCount}{contactDetails}";
    }
    private BoxCollider2D Wall(Vector2 position, Vector2 size)
    {
        var go = Create("Vault-like solid"); go.transform.position = origin + position;
        go.layer = LayerMask.NameToLayer("Walls"); var box = go.AddComponent<BoxCollider2D>(); box.size = size;
        Physics2D.SyncTransforms(); return box;
    }
    private EnemyController2D Enemy(Vector2 position, PredictiveAvoidanceGroup group = PredictiveAvoidanceGroup.SmallMelee)
    {
        var go = Create("NavigationMelee"); go.SetActive(false); go.tag = "Enemy"; go.layer = LayerMask.NameToLayer("Enemies");
        go.transform.position = origin + position;
        var body = go.AddComponent<Rigidbody2D>(); body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeRotation;
        var circle = go.AddComponent<CircleCollider2D>(); circle.radius = 0.87684506f;
        circle.excludeLayers = 1 << go.layer; circle.layerOverridePriority = 2;
        var sensorObject = new GameObject("EnemySeparation"); sensorObject.layer = go.layer; sensorObject.transform.SetParent(go.transform, false);
        var sensor = sensorObject.AddComponent<CircleCollider2D>(); sensor.radius = 0.2f; sensor.isTrigger = true;
        sensor.includeLayers = 1 << go.layer; sensor.excludeLayers = ~(1 << go.layer);
        sensorObject.AddComponent<EnemySeparationCollider>();
        go.AddComponent<Health>(); go.AddComponent<EnemyRootReceiver>(); go.AddComponent<EnemyKnockbackReceiver>();
        go.AddComponent<EnemyStaggerReceiver>();
        go.AddComponent<EnemySurroundEligibility>().AvoidanceGroup = group;
        go.AddComponent<EnemyNavigationChase>();
        var enemy = go.AddComponent<EnemyController2D>();
        go.AddComponent<EnemyRegionState>();
        enemy.Speed = 2f; go.SetActive(true);
        enemy.Debug_SetBarrierTargeting(false); enemy.Debug_SetupRefs(player);
        enemy.Debug_SetTargetDecision(player, player, EnemyTargetType.Player);
        Physics2D.SyncTransforms(); return enemy;
    }

    [UnityTest] public IEnumerator PenetrationRecovery_VaultExitResetsRouteAndResumesAuthoredSpeed()
    {
        var wall = Wall(Vector2.zero, new Vector2(1.1f, 2f));
        var enemy = Enemy(Vector2.left * 4f);
        var adapter = enemy.GetComponent<EnemyNavigationChase>();
        var body = enemy.GetComponent<Rigidbody2D>();
        var primary = enemy.GetComponent<CircleCollider2D>();
        float deadline = Time.time + 12f;
        while (Time.time < deadline && (adapter.Navigation == null || adapter.Navigation.RouteCount == 0))
            yield return new WaitForFixedUpdate();
        RequireNavigation(adapter.Navigation != null && adapter.Navigation.RouteCount > 0, "Expected an initial route.", enemy);
        // Inject a bad starting state only in the fixture. Runtime recovery never assigns body.position.
        body.position = origin + Vector2.left * 1.2f;
        enemy.transform.position = body.position; Physics2D.SyncTransforms();
        Vector2 radial = Vector2.zero, tangent = Vector2.zero;
        adapter.Apply(2f, Time.fixedDeltaTime, ref radial, ref tangent);
        Assert.IsTrue(adapter.IsRecovering);
        Assert.That(adapter.Navigation.RouteCount, Is.Zero);
        Assert.That(adapter.Navigation.RequestId, Is.Zero);
        float previousGap = Physics2D.Distance(primary, wall).distance;
        bool cleared = false, resumedSpeed = false;
        var sampler = new StaticNavigationWorld2D(runtime.BodyRadius);
        Vector2 previous = body.position;
        for (int i = 0; i < 1200 && !enemy.IsInHoldRange(); i++)
        {
            yield return new WaitForFixedUpdate();
            float gap = Physics2D.Distance(primary, wall).distance;
            Assert.That(gap, Is.GreaterThanOrEqualTo(Mathf.Min(previousGap, -0.002f) - 0.002f),
                "Recovery must not deepen penetration or reenter the Vault.");
            Assert.That(adapter.DebugAllowedDisplacement.magnitude, Is.LessThanOrEqualTo(2f * Time.fixedDeltaTime + 0.0001f));
            if (cleared)
            {
                float distance = Vector2.Distance(body.position, previous);
                Assert.That(distance, Is.LessThanOrEqualTo(2f * Time.fixedDeltaTime + 0.005f));
                resumedSpeed |= !adapter.IsRecovering && distance >= 1.9f * Time.fixedDeltaTime;
            }
            cleared |= sampler.IsPointClear(primary.bounds.center) && !adapter.IsRecovering;
            previousGap = gap; previous = body.position;
        }
        RequireNavigation(cleared && resumedSpeed && enemy.IsInHoldRange(), "Recovery must clear the Vault and resume normal pursuit.", enemy);
    }

    [UnityTest] public IEnumerator VaultDetour_ReachesPlayerWithoutWallCuttingOrSpeedBoost()
    {
        var wall = Wall(Vector2.zero, new Vector2(1.1f, 2f)); var enemy = Enemy(Vector2.left * 4f);
        var body = enemy.GetComponent<Rigidbody2D>(); var collider = enemy.GetComponent<CircleCollider2D>();
        bool detoured = false; Vector2 previous = body.position;
        for (int i = 0; i < 900 && !enemy.IsInHoldRange(); i++)
        {
            yield return new WaitForFixedUpdate();
            Assert.That(Vector2.Distance(previous, body.position), Is.LessThanOrEqualTo(2f * Time.fixedDeltaTime + 0.005f));
            Assert.That(Physics2D.Distance(collider, wall).distance, Is.GreaterThan(-0.002f));
            detoured |= Mathf.Abs(body.position.y - origin.y) > 1.5f;
            Assert.That(runtime.SamplingQueriesLastTick, Is.LessThanOrEqualTo(128));
            Assert.That(runtime.LiveQueriesThisFrame, Is.LessThanOrEqualTo(256));
            previous = body.position;
        }
        RequireNavigation(detoured, "Enemy never detoured around the wall.", enemy);
        RequireNavigation(enemy.IsInHoldRange(), "Enemy did not reach engagement range.", enemy);
        Assert.That(runtime.SubmittedRequests, Is.GreaterThan(0));
    }

    [UnityTest] public IEnumerator ReopenedLine_DropsRouteAndResumesDirectPursuit()
    {
        var wall = Wall(Vector2.zero, new Vector2(1.1f, 2f)); var enemy = Enemy(Vector2.left * 4f);
        var adapter = enemy.GetComponent<EnemyNavigationChase>();
        // The first narrow region cannot fit this detour; allow the timed wider-region retry.
        float deadline = Time.time + 12f;
        while (Time.time < deadline && (adapter.Navigation == null || adapter.Navigation.RouteCount == 0))
            yield return new WaitForFixedUpdate();
        RequireNavigation(adapter.Navigation != null && adapter.Navigation.RouteCount > 0,
            "No route before reopening the line.", enemy);
        Bounds bounds = wall.bounds; wall.enabled = false;
        runtime.Invalidate(new Rect(bounds.min, bounds.size), new Rect(bounds.min, bounds.size));
        yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
        Assert.That(adapter.Navigation.State, Is.EqualTo(EnemyNavigationState.Direct));
        Assert.That(adapter.Navigation.RouteCount, Is.Zero);
    }

    [UnityTest] public IEnumerator BarrierApproach_UsesExteriorAttackBandAndSharedScheduler()
    {
        var regionObject = Create("CastleRegion"); regionObject.transform.position = origin + Vector2.right * 6f;
        var regionCollider = regionObject.AddComponent<BoxCollider2D>(); regionCollider.size = new Vector2(10f, 12f); regionCollider.isTrigger = true;
        var region = regionObject.AddComponent<CastleRegionTracker>(); region.Debug_SetPlayerInsideForTests(true);
        var barrierCollider = Wall(Vector2.zero, new Vector2(1f, 3f));
        var barrier = barrierCollider.gameObject.AddComponent<BarrierHealth>();
        var anchor = Create("ApproachAnchor"); anchor.transform.position = origin + Vector2.left * 2f;
        barrier.gameObject.AddComponent<EnemyBarrierHoldBehavior>().Debug_SetAnchor(anchor.transform);
        Wall(new Vector2(-3f, 0f), new Vector2(1f, 2f));
        var attacker = Enemy(new Vector2(-6f, 0f));
        attacker.Debug_SetBarrierTargeting(true); attacker.Debug_SetupRefs(player, barrier.transform);
        attacker.Debug_SetTargetDecision(barrier.transform, barrier.transform, EnemyTargetType.Barrier);
        var playerChaser = Enemy(new Vector2(-6f, -3f));
        for (int i = 0; i < 1000 && !attacker.IsInHoldRange(); i++) yield return new WaitForFixedUpdate();
        Assert.That(attacker.Target, Is.SameAs(barrier.transform));
        RequireNavigation(attacker.IsInHoldRange(), "Barrier attacker did not reach engagement range.", attacker);
        Assert.That(attacker.GetComponent<EnemyEngagement>().SurfaceDistanceTo(barrier.transform), Is.LessThanOrEqualTo(0.501f));
        Assert.IsFalse(region.ContainsPosition(attacker.transform.position));
        Assert.IsNotNull(playerChaser.GetComponent<EnemyNavigationChase>().Navigation);
        Assert.That(StaticNavigationRuntime.Instance, Is.SameAs(runtime));
        Assert.That(runtime.SubmittedRequests, Is.GreaterThanOrEqualTo(2));
    }

    [UnityTest] public IEnumerator BarrierBreakRepair_InvalidatesLocalCache()
    {
        var box = Wall(Vector2.zero, Vector2.one); var barrier = box.gameObject.AddComponent<BarrierHealth>();
        runtime.Cache.TryWorldToCell(origin, out var cell);
        runtime.Cache.GetCellState(cell); yield return null; yield return null;
        Assert.That(runtime.Cache.GetCellState(cell), Is.EqualTo(StaticNavigationSampleState.Blocked));
        long before = runtime.Cache.Revision;
        barrier.TakeDamage(barrier.MaxHealth);
        Assert.That(runtime.Cache.Revision, Is.GreaterThan(before));
        runtime.Cache.GetCellState(cell); yield return null; yield return null;
        Assert.That(runtime.Cache.GetCellState(cell), Is.EqualTo(StaticNavigationSampleState.Clear));
        before = runtime.Cache.Revision; Assert.IsTrue(barrier.Repair());
        Assert.That(runtime.Cache.Revision, Is.GreaterThan(before));
        runtime.Cache.GetCellState(cell); yield return null; yield return null;
        Assert.That(runtime.Cache.GetCellState(cell), Is.EqualTo(StaticNavigationSampleState.Blocked));
    }

    [UnityTest] public IEnumerator RootAndKnockback_PreserveOwnershipAndPauseNavigation()
    {
        var enemy = Enemy(Vector2.left * 4f); var body = enemy.GetComponent<Rigidbody2D>();
        enemy.GetComponent<EnemyRootReceiver>().RootForSeconds(0.3f);
        Vector2 before = body.position;
        for (int i = 0; i < 5; i++) yield return new WaitForFixedUpdate();
        Assert.That(Vector2.Distance(before, body.position), Is.LessThan(0.001f));
        yield return new WaitForSeconds(0.35f);
        var knockback = enemy.GetComponent<EnemyKnockbackReceiver>(); knockback.AddKnockback(Vector2.up * 2f, 5f);
        before = body.position; yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
        Assert.That(body.position.y, Is.GreaterThan(before.y));
        Assert.IsTrue(knockback.IsActive);
    }

    [UnityTest] public IEnumerator SameGroupAvoidance_StillCurvesBeforeHardSeparationOnRoute()
    {
        Wall(Vector2.zero, new Vector2(1.1f, 2f));
        var front = Enemy(new Vector2(-3f, 0f)); var rear = Enemy(new Vector2(-4.4f, 0f));
        front.GetComponent<EnemyRootReceiver>().RootForSeconds(10f);
        var adapter = rear.GetComponent<EnemyNavigationChase>(); bool curved = false;
        for (int i = 0; i < 500 && !curved; i++)
        {
            yield return new WaitForFixedUpdate();
            curved = adapter.Navigation != null && adapter.Navigation.RouteCount > 0 &&
                Mathf.Abs(rear.transform.position.y - origin.y) > 0.15f;
            Assert.That(Vector2.Distance(front.transform.position, rear.transform.position), Is.GreaterThanOrEqualTo(0.39f));
        }
        Assert.IsTrue(curved);
    }

    [UnityTest] public IEnumerator MissingRuntime_UsesExistingChaseMovement()
    {
        yield return VerifyUnavailableRuntimeFallback(false);
    }

    [UnityTest] public IEnumerator UninitializedRuntime_UsesExistingChaseMovement()
    {
        yield return VerifyUnavailableRuntimeFallback(true);
    }

    private IEnumerator VerifyUnavailableRuntimeFallback(bool uninitialized)
    {
        runtime.enabled = false;
        if (uninitialized)
        {
            var unavailable = Create("UninitializedNavigation").AddComponent<StaticNavigationRuntime>();
            Assert.That(StaticNavigationRuntime.Instance, Is.SameAs(unavailable));
            Assert.IsNull(unavailable.Cache);
        }
        else Assert.IsNull(StaticNavigationRuntime.Instance);
        var enemy = Enemy(Vector2.left * 4f);
        var adapter = enemy.GetComponent<EnemyNavigationChase>();
        Vector2 before = enemy.GetComponent<Rigidbody2D>().position;
        for (int i = 0; i < 10; i++) yield return new WaitForFixedUpdate();
        Vector2 after = enemy.GetComponent<Rigidbody2D>().position;
        Assert.That(after.x - before.x, Is.GreaterThan(0.05f),
            "The controller must use its existing chase path when navigation is unavailable.");
        Assert.That(Vector2.Distance(before, after), Is.LessThanOrEqualTo(enemy.Speed * Time.fixedDeltaTime * 11f + 0.01f));
        Assert.IsFalse(adapter.GuardMovement);
        Assert.IsFalse(adapter.IsRecovering);
    }

    [UnityTest] public IEnumerator StaggerAndHold_SuspendNavigationAndAttackChaseUsesRoutes()
    {
        var enemy = Enemy(Vector2.left * 4f);
        var attack = enemy.gameObject.AddComponent<EnemyAttack>(); attack.enabled = false;
        var stagger = enemy.GetComponent<EnemyStaggerReceiver>(); stagger.Configure(true, 0.2f, attack);
        Assert.IsTrue(stagger.TryStagger()); Vector2 before = enemy.transform.position;
        for (int i = 0; i < 5; i++) yield return new WaitForFixedUpdate();
        Assert.That(Vector2.Distance(before, enemy.transform.position), Is.LessThan(0.001f));
        yield return new WaitForSeconds(0.25f);
        stagger.AcknowledgeTargetRefresh();
        Wall(Vector2.zero, new Vector2(1.1f, 2f));
        enemy.RequestChase();
        float deadline = Time.time + 5f;
        while (Time.time < deadline && runtime.SubmittedRequests == 0) yield return new WaitForFixedUpdate();
        RequireNavigation(runtime.SubmittedRequests > 0,
            "Attack reacquisition must route instead of forcing wall pursuit.", enemy);
        var movement = enemy.GetComponent<EnemyLocomotion>(); var adapter = enemy.GetComponent<EnemyNavigationChase>();
        movement.SetMovementState(EnemyController2D.State.HOLD);
        Vector2 radial = Vector2.zero, tangent = Vector2.zero;
        Assert.IsTrue(adapter.Apply(2f, 0.02f, ref radial, ref tangent));
        Assert.That(adapter.Navigation.State, Is.EqualTo(EnemyNavigationState.Suspended));
        Assert.That(radial + tangent, Is.EqualTo(Vector2.zero));
    }

    [TestCase(PredictiveAvoidanceGroup.SmallMelee, true)]
    [TestCase(PredictiveAvoidanceGroup.Lurker, false)]
    public void PreferredRouteVelocity_RespectsExistingGroupsAcrossTargets(PredictiveAvoidanceGroup frontGroup, bool avoids)
    {
        var front = Enemy(new Vector2(-2.6f, 0f), frontGroup);
        var rear = Enemy(new Vector2(-4f, 0f));
        front.Debug_SetTargetDecision(player, player, EnemyTargetType.Barrier);
        front.GetComponent<EnemyLocomotion>().SetMovementState(EnemyController2D.State.HOLD);
        Physics2D.SyncTransforms();
        Vector2 velocity = new EnemyPredictiveChase().Compute(rear, player, Vector2.right * 3.5f, v => true);
        Assert.That(Mathf.Abs(velocity.y) > 0.05f, Is.EqualTo(avoids));
        Assert.That(velocity.magnitude, Is.LessThanOrEqualTo(3.5001f));
        Vector2 awayFromPlayer = new EnemyPredictiveChase().Compute(rear, player, Vector2.down * 2f, v => true);
        Assert.That(awayFromPlayer.y, Is.LessThan(0f), "The supplied route direction must not be replaced by player pursuit.");
    }

    [UnityTest] public IEnumerator FinalDisplacementGuard_RejectsWallCrossingAndLeavesKnockbackSeparate()
    {
        Wall(Vector2.zero, Vector2.one); var enemy = Enemy(Vector2.left * 3f);
        yield return new WaitForFixedUpdate();
        var adapter = enemy.GetComponent<EnemyNavigationChase>();
        Assert.IsTrue(adapter.GuardMovement);
        Assert.That(adapter.ConstrainFinalDisplacement(Vector2.right * 6f), Is.EqualTo(Vector2.zero));
        // ExecuteMovement applies #277 first and this guard second, before adding knockback.
        enemy.GetComponent<EnemyKnockbackReceiver>().AddKnockback(Vector2.up, 1f);
        Vector2 before = enemy.GetComponent<Rigidbody2D>().position;
        enemy.GetComponent<EnemyLocomotion>().ExecuteMovement(enemy.GetComponent<Rigidbody2D>(), Vector2.right * 300f, Vector2.zero, 0.02f);
        yield return new WaitForFixedUpdate();
        Assert.That(enemy.transform.position.x, Is.LessThan(origin.x - 1.3f));
        Assert.That(enemy.transform.position.y, Is.GreaterThanOrEqualTo(before.y));
    }
}
#endif

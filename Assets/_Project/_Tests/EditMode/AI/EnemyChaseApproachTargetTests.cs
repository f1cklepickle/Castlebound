using System;
using System.Reflection;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

public class EnemyChaseApproachTargetTests
{
    private readonly EnemyChaseApproachTarget.Footprint[] blocked =
    {
        new EnemyChaseApproachTarget.Footprint(new Vector2(1.3f, 0f), 0.2f)
    };

    private static bool Resolve(EnemyChaseApproachTarget helper,
        EnemyChaseApproachTarget.Footprint[] neighbors, out Vector2 target,
        Vector2? position = null, Vector2? player = null, Vector2? bias = null,
        float surfaceDistance = 2f, int playerId = 1)
    {
        return helper.TryGetTarget(position ?? new Vector2(2f, 0f), player ?? Vector2.zero,
            playerId, 0.2f, 1.5f, surfaceDistance, 0.5f, bias ?? Vector2.up,
            neighbors, out target);
    }

    [TestCase(1.51f, true)]
    [TestCase(2.25f, true)]
    [TestCase(3f, true)]
    [TestCase(3.01f, true)]
    [TestCase(3.45f, false)]
    public void Lookahead_ExtendedCollectionStillRequiresCorridorIntersection(float blockerDistance, bool expected)
    {
        Vector2 position = new Vector2(3.5f, 0f);
        var neighbors = new[] {
            new EnemyChaseApproachTarget.Footprint(position + Vector2.left * blockerDistance, 0.2f)
        };
        Assert.That(Resolve(new EnemyChaseApproachTarget(), neighbors, out Vector2 point,
            position: position, surfaceDistance: 3.5f), Is.EqualTo(expected));
        if (expected)
            Assert.That(Vector2.Distance(position, point), Is.EqualTo(1.5f).Within(0.0001f));
    }

    [TestCase(4.49f, true)]
    [TestCase(4.5f, true)]
    [TestCase(4.51f, false)]
    public void Lookahead_LargeFootprintRespectsFourPointFiveUnitBoundary(float blockerDistance, bool expected)
    {
        // The unchanged gate caps direct travel at 3 units. A larger footprint is
        // needed to intersect that endpoint while its center tests the 4.5-unit boundary.
        Vector2 position = new Vector2(5f, 0f);
        var neighbors = new[] {
            new EnemyChaseApproachTarget.Footprint(position + Vector2.left * blockerDistance, 1.2f)
        };
        Assert.That(Resolve(new EnemyChaseApproachTarget(), neighbors, out Vector2 point,
            position: position, surfaceDistance: 3.5f), Is.EqualTo(expected));
        if (expected)
            Assert.That(Vector2.Distance(position, point), Is.EqualTo(1.5f).Within(0.0001f));
    }

    [TestCase(0.5f, 1f, 3.5f)] // Newly collected distant ally, off the corridor.
    [TestCase(7f, 0f, 3.5f)] // Newly collected rear ally.
    [TestCase(-0.5f, 0f, 3.5f)] // Within lookahead, beyond the corridor endpoint.
    [TestCase(1f, 1f, 3.5f)] // Distant and closer to Player, but off the corridor.
    [TestCase(4f, 0f, 3.5f)] // Rear ally.
    [TestCase(1f, 0f, 0.6f)] // Beyond the engagement-truncated segment.
    [TestCase(1f, 0f, 3.501f)] // Player surface-distance gate remains 3.5.
    [TestCase(1f, 0f, 0.5f)] // Engagement cutoff.
    public void Lookahead_NonObstructionOrExcludedDistanceDoesNotActivate(float x, float y, float surface)
    {
        var neighbors = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(x, y), 0.2f)
        };
        Assert.IsFalse(Resolve(new EnemyChaseApproachTarget(), neighbors, out _,
            position: new Vector2(3.5f, 0f), surfaceDistance: surface));
    }

    [Test]
    public void Lookahead_OffCorridorExtrasDoNotChangeTangentOrCommittedSide()
    {
        Vector2 position = new Vector2(3.5f, 0f);
        var obstruction = new EnemyChaseApproachTarget.Footprint(new Vector2(1f, 0f), 0.2f);
        var helper = new EnemyChaseApproachTarget();
        Assert.IsTrue(Resolve(helper, new[] { obstruction }, out Vector2 first,
            position: position, surfaceDistance: 3.5f));
        int side = helper.Side;
        var extras = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(2f, 1.5f), 0.2f),
            obstruction
        };
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), extras, out Vector2 fresh,
            position: position, surfaceDistance: 3.5f));
        Assert.That(Vector2.Distance(first, fresh), Is.LessThan(0.0001f),
            "An off-corridor distant ally must not skew tangent or side selection.");
        Array.Reverse(extras);
        Assert.IsTrue(Resolve(helper, extras, out Vector2 retained, position: position,
            surfaceDistance: 3.5f, bias: Vector2.down));
        Assert.That(helper.Side, Is.EqualTo(side));
        Assert.That(retained, Is.EqualTo(first));
        Assert.IsFalse(helper.WaypointChanged);
    }

    [Test]
    public void Lookahead_ReleaseMarginRetainsDistantRouteUntilCorridorClears()
    {
        Vector2 position = new Vector2(3.5f, 0f);
        var helper = new EnemyChaseApproachTarget();
        var obstruction = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(1f, 0f), 0.2f)
        };
        Assert.IsTrue(Resolve(helper, obstruction, out _, position: position, surfaceDistance: 3.5f));
        var edge = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(1f, 0.48f), 0.2f)
        };
        Assert.IsFalse(Resolve(new EnemyChaseApproachTarget(), edge, out _,
            position: position, surfaceDistance: 3.5f));
        Assert.IsTrue(Resolve(helper, edge, out _, position: position, surfaceDistance: 3.5f));
        var clear = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(1f, 0.6f), 0.2f)
        };
        Assert.IsFalse(Resolve(helper, clear, out _, position: position, surfaceDistance: 3.5f));
    }

    [TestCase("alive", true)]
    [TestCase("dead", false)]
    [TestCase("otherPlayer", false)]
    [TestCase("barrier", false)]
    [TestCase("disabled", false)]
    public void Lookahead_RuntimeCollectionPreservesBlockerEligibility(string condition, bool expected)
    {
        var enemy = new GameObject("EarlyApproachEnemy");
        var blocker = new GameObject("EarlyApproachBlocker");
        var player = new GameObject("Player");
        var otherPlayer = new GameObject("OtherPlayer");
        player.tag = "Player";
        var registry = (List<EnemyController2D>)typeof(EnemyController2D)
            .GetField("All", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        EnemyController2D controller = null, other = null;
        try
        {
            controller = ConfigureEnemy(enemy, player.transform, new Vector2(3.5f, 0f));
            other = ConfigureEnemy(blocker, player.transform, new Vector2(1f, 0f));
            var health = blocker.AddComponent<Health>();
            health.ConfigureMaxHealth(10, refill: true);
            if (condition == "dead") health.ConfigureMaxHealth(0, refill: true);
            if (condition == "otherPlayer")
                other.Debug_SetTargetDecision(otherPlayer.transform, otherPlayer.transform, EnemyTargetType.Player);
            if (condition == "barrier")
                other.Debug_SetTargetDecision(player.transform, player.transform, EnemyTargetType.Barrier);
            if (condition == "disabled") other.enabled = false;
            if (!registry.Contains(other)) registry.Add(other);
            Physics2D.SyncTransforms();
            Assert.That(new EnemyChaseApproachTarget().TryGetTarget(controller, player.transform,
                new Vector2(3.5f, 0f), Vector2.up, 3.5f, 0.5f, out Vector2 point), Is.EqualTo(expected));
            if (expected)
                Assert.That(Vector2.Distance(new Vector2(3.5f, 0f), point),
                    Is.EqualTo(1.5f).Within(0.0001f));
        }
        finally
        {
            registry.Remove(controller);
            registry.Remove(other);
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(blocker);
            UnityEngine.Object.DestroyImmediate(player);
            UnityEngine.Object.DestroyImmediate(otherPlayer);
        }
    }

    [Test]
    public void ClearRoute_DoesNotSupplyAnOverride()
    {
        var helper = new EnemyChaseApproachTarget();
        Assert.IsFalse(Resolve(helper, Array.Empty<EnemyChaseApproachTarget.Footprint>(), out _));
        Assert.IsFalse(helper.IsActive);
    }

    [Test]
    public void OccupiedApproach_SelectsDiagonalTargetWhoseSegmentClearsBlocker()
    {
        var helper = new EnemyChaseApproachTarget();
        Vector2 position = new Vector2(2f, 0f);
        Assert.IsTrue(Resolve(helper, blocked, out Vector2 target));
        Assert.That(target.x, Is.LessThan(position.x));
        Assert.That(target.y, Is.GreaterThan(0f));
        Vector2 travel = target - position;
        float t = Mathf.Clamp01(Vector2.Dot(blocked[0].Center - position, travel) / travel.sqrMagnitude);
        Assert.That(Vector2.Distance(position + travel * t, blocked[0].Center),
            Is.GreaterThan(0.4f), "The requested path must clear the combined physical footprints.");
    }

    [Test]
    public void BlockedRoute_KeepsSideAndTargetDespiteOpposingBiasAndNeighborOrder()
    {
        var helper = new EnemyChaseApproachTarget();
        var neighbors = new[] { blocked[0],
            new EnemyChaseApproachTarget.Footprint(new Vector2(0.9f, 0f), 0.2f) };
        Assert.IsTrue(Resolve(helper, neighbors, out Vector2 first));
        int side = helper.Side;
        Array.Reverse(neighbors);
        for (int i = 0; i < 10; i++)
        {
            Vector2 moved = new Vector2(2f, 0f) + (first - new Vector2(2f, 0f)).normalized * (i * 0.01f);
            Assert.IsTrue(Resolve(helper, neighbors, out Vector2 next, position: moved, bias: Vector2.down));
            Assert.That(helper.Side, Is.EqualTo(side));
            Assert.That(next, Is.EqualTo(first));
        }
    }

    [Test]
    public void SymmetricObstruction_OppositeStableBiasesSelectOppositeSides()
    {
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), blocked, out Vector2 north, bias: Vector2.up));
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), blocked, out Vector2 south, bias: Vector2.down));
        Assert.That(north.y, Is.GreaterThan(0f));
        Assert.That(south.y, Is.LessThan(0f));
        Assert.That(north.x, Is.EqualTo(south.x).Within(0.0001f));
    }

    [Test]
    public void ContactBlocker_TargetDoesNotRequestInwardPressure()
    {
        var contact = new[] { new EnemyChaseApproachTarget.Footprint(new Vector2(1.6f, 0f), 0.2f) };
        Vector2 position = new Vector2(2f, 0f);
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), contact, out Vector2 target));
        Assert.That(Vector2.Dot(target - position, contact[0].Center - position),
            Is.LessThanOrEqualTo(0f));
    }

    [Test]
    public void WiderCrowdingOnBothSides_RequiresLargerLateralOffset()
    {
        Resolve(new EnemyChaseApproachTarget(), blocked, out Vector2 narrow);
        var wide = new[] { blocked[0],
            new EnemyChaseApproachTarget.Footprint(new Vector2(1.3f, 0.25f), 0.2f),
            new EnemyChaseApproachTarget.Footprint(new Vector2(1.3f, -0.25f), 0.2f) };
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), wide, out Vector2 target));
        Assert.That(Mathf.Abs(target.y), Is.GreaterThan(Mathf.Abs(narrow.y)));
    }

    [Test]
    public void PlayerTranslation_MovesPointWithoutChangingAssignedOffset()
    {
        var helper = new EnemyChaseApproachTarget();
        Resolve(helper, blocked, out Vector2 first);
        Vector2 shift = new Vector2(4f, 3f);
        var translated = new[] { new EnemyChaseApproachTarget.Footprint(blocked[0].Center + shift, 0.2f) };
        Assert.IsTrue(Resolve(helper, translated, out Vector2 moved,
            position: new Vector2(2f, 0f) + shift, player: shift));
        Assert.That(Vector2.Distance(moved, first + shift), Is.LessThan(0.0001f));
    }

    [Test]
    public void RouteClears_DiscardsBypassAndAllowsOrdinaryChase()
    {
        var helper = new EnemyChaseApproachTarget();
        Assert.IsTrue(Resolve(helper, blocked, out _));
        var clear = new[] { new EnemyChaseApproachTarget.Footprint(new Vector2(1.3f, 1f), 0.2f) };
        Assert.IsFalse(Resolve(helper, clear, out _));
        Assert.IsFalse(helper.IsActive);
        Assert.That(helper.Side, Is.Zero);
        Assert.That(helper.Offset, Is.EqualTo(Vector2.zero));
    }

    [Test]
    public void ReleaseClearance_PreventsDroppingBypassAtAcquisitionBoundary()
    {
        var helper = new EnemyChaseApproachTarget();
        Assert.IsTrue(Resolve(helper, blocked, out _));
        var edge = new[] { new EnemyChaseApproachTarget.Footprint(new Vector2(1.3f, 0.48f), 0.2f) };
        Assert.IsFalse(Resolve(new EnemyChaseApproachTarget(), edge, out _));
        Assert.IsTrue(Resolve(helper, edge, out _));
    }

    [Test]
    public void FarChase_RemainsOutsideExperimentEvenWithLocalBlocker()
    {
        var far = new[] { new EnemyChaseApproachTarget.Footprint(new Vector2(11.3f, 0f), 0.2f) };
        Assert.IsFalse(Resolve(new EnemyChaseApproachTarget(), far, out _,
            position: new Vector2(12f, 0f), surfaceDistance: 12f));
    }

    [Test]
    public void FartherAllyOrAllyBeyondAttackEntry_DoesNotBlockDirectRoute()
    {
        var rear = new[] { new EnemyChaseApproachTarget.Footprint(new Vector2(2.5f, 0f), 0.2f) };
        Assert.IsFalse(Resolve(new EnemyChaseApproachTarget(), rear, out _));
        Assert.IsFalse(Resolve(new EnemyChaseApproachTarget(), blocked, out _, surfaceDistance: 0.6f));
    }

    [Test]
    public void ResetOrTargetChange_DiscardsAssignedSide()
    {
        var helper = new EnemyChaseApproachTarget();
        Resolve(helper, blocked, out _);
        helper.Reset();
        Assert.IsFalse(helper.IsActive);
        Assert.That(helper.Offset, Is.EqualTo(Vector2.zero));
        Resolve(helper, blocked, out Vector2 south, bias: Vector2.down);
        Assert.That(south.y, Is.LessThan(0f));
        Resolve(helper, blocked, out Vector2 north, bias: Vector2.up, playerId: 2);
        Assert.That(north.y, Is.GreaterThan(0f));
    }
    [TestCase("clear")]
    [TestCase("far")]
    [TestCase("hold")]
    [TestCase("ranged")]
    [TestCase("barrier")]
    [TestCase("ineligible")]
    [TestCase("disabled")]
    public void LocomotionBypassGate_PreservesExcludedStatesAndSmoothsRouteRelease(string gate)
    {
        var enemy = new GameObject("ApproachGateEnemy");
        var blocker = new GameObject("ApproachGateBlocker");
        var player = new GameObject("Player");
        player.tag = "Player";
        var registry = (List<EnemyController2D>)typeof(EnemyController2D)
            .GetField("All", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        EnemyController2D controller = null;
        EnemyController2D other = null;
        try
        {
            controller = ConfigureEnemy(enemy, player.transform, new Vector2(2f, 0f));
            other = ConfigureEnemy(blocker, player.transform, new Vector2(1.3f, 0f));
            // Explicit lifecycle registration isolates this EditMode fixture from callback dispatch.
            if (!registry.Contains(other)) registry.Add(other);
            Physics2D.SyncTransforms();
            var locomotion = enemy.GetComponent<EnemyLocomotion>();
            Vector2 radial = Vector2.left * 3f;
            Vector2 tangent = Vector2.up * 0.2f;
            Assert.IsTrue(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                true, Vector2.up, 2f, 0.5f, 3f, 0.02f, ref radial, ref tangent, out _),
                "The identical ungated fixture must first arm a real bypass.");
            Assert.IsTrue(locomotion.HasChaseApproachTarget);
            for (int i = 0; i < 40; i++)
            {
                radial = Vector2.left * 3f;
                tangent = Vector2.up * 0.2f;
                locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                    true, Vector2.up, 2f, 0.5f, 3f, 0.02f, ref radial, ref tangent, out _);
            }
            Vector2 previous = radial + tangent;
            Assert.IsFalse(locomotion.IsChaseApproachTurning);
            if (gate == "hold") locomotion.SetMovementState(EnemyController2D.State.HOLD);
            if (gate == "ranged") locomotion.Debug_SetHoldMovementPolicy(enemy.AddComponent<EnemyRangedEngagement>());
            if (gate == "barrier") controller.Debug_SetTargetDecision(player.transform, player.transform, EnemyTargetType.Barrier);
            if (gate == "disabled") locomotion.enabled = false;
            if (gate == "clear")
            {
                blocker.GetComponent<Rigidbody2D>().position = new Vector2(1.3f, 1f);
                blocker.transform.position = new Vector2(1.3f, 1f);
                Physics2D.SyncTransforms();
            }
            radial = Vector2.left * 3f;
            tangent = Vector2.up * 0.2f;
            Assert.IsFalse(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                gate != "ineligible", Vector2.up, gate == "far" ? 12f : 2f, 0.5f, 3f, 0.02f,
                ref radial, ref tangent, out _));
            if (gate == "clear" || gate == "far")
            {
                Assert.IsTrue(locomotion.IsChaseApproachTurning);
                Assert.That(Vector2.Angle(previous, radial + tangent), Is.LessThanOrEqualTo(4.501f));
                Assert.That((radial + tangent).magnitude,
                    Is.EqualTo(3f).Within(0.0001f), "Exit smoothing must cap even an oversized caller request.");
                for (int i = 0; i < 41; i++)
                {
                    radial = Vector2.left * 3f;
                    tangent = Vector2.up * 0.2f;
                    Assert.IsFalse(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                        true, Vector2.up, gate == "far" ? 12f : 2f, 0.5f, 3f, 0.02f,
                        ref radial, ref tangent, out _));
                }
            }
            Assert.That(radial, Is.EqualTo(Vector2.left * 3f));
            Assert.That(tangent, Is.EqualTo(Vector2.up * 0.2f));
            Assert.IsFalse(locomotion.IsChaseApproachTurning);
            Assert.IsFalse(locomotion.HasChaseApproachTarget);
        }
        finally
        {
            registry.Remove(controller);
            registry.Remove(other);
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(blocker);
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    [TestCase(1f)]
    [TestCase(-1f)]
    public void StronglyOutwardReplacement_IsLimitedTo100DegreesWithLateralClearing(float side)
    {
        var helper = new EnemyChaseApproachTarget();
        Vector2 position = new Vector2(2f, 0f);
        Assert.IsTrue(Resolve(helper, blocked, out _, bias: Vector2.up * side));
        int originalSide = helper.Side;
        // The committed side now faces a close diagonal ally: the old tangent is about 154 degrees.
        var crowded = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(1.9f, 0.2f * side), 0.2f)
        };
        Assert.IsTrue(Resolve(helper, crowded, out Vector2 point));
        Vector2 direction = (point - position).normalized;
        Assert.That(Vector2.Angle(Vector2.left, direction), Is.EqualTo(100f).Within(0.001f));
        Assert.That(direction.y * side, Is.GreaterThan(0.98f));
        Assert.That(Vector2.Distance(position, point), Is.EqualTo(1.5f).Within(0.0001f));
        Assert.That(helper.Side, Is.EqualTo(originalSide));
        Assert.IsTrue(helper.WaypointChanged);
    }

    [Test]
    public void OrdinaryInwardAndContactLateralCandidates_KeepExistingGeometry()
    {
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), blocked, out Vector2 point));
        float angle = Mathf.Asin(0.44f / 0.7f) + 0.01f;
        Vector2 expected = new Vector2(2f, 0f) +
            new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f;
        Assert.That(Vector2.Distance(point, expected), Is.LessThan(0.0001f));
        var contact = new[] {
            new EnemyChaseApproachTarget.Footprint(new Vector2(1.6f, 0f), 0.2f)
        };
        Assert.IsTrue(Resolve(new EnemyChaseApproachTarget(), contact, out point));
        float lateralAngle = Vector2.Angle(Vector2.left, point - new Vector2(2f, 0f));
        Assert.That(lateralAngle, Is.EqualTo(90f + 0.01f * Mathf.Rad2Deg).Within(0.001f),
            "Contact clearing may remain slightly outward; never force positive inward progress.");
    }

    [Test]
    public void RetainedPointThatBecomesOutward_IsReplacedAndSignalsTransition()
    {
        var helper = new EnemyChaseApproachTarget();
        Assert.IsTrue(Resolve(helper, blocked, out Vector2 oldPoint));
        Assert.IsTrue(Resolve(helper, blocked, out _));
        Assert.IsFalse(helper.WaypointChanged, "An unchanged route must not repeatedly re-arm smoothing.");
        Vector2 position = new Vector2(0.5f, 1f);
        var obstruction = new[] {
            new EnemyChaseApproachTarget.Footprint(position + (-position).normalized * 0.7f, 0.2f)
        };
        Assert.That(Vector2.Angle(-position, oldPoint - position), Is.GreaterThan(100f));
        Assert.IsTrue(Resolve(helper, obstruction, out Vector2 point, position: position));
        Assert.IsTrue(helper.WaypointChanged);
        Assert.That(Vector2.Angle(-position, point - position), Is.LessThanOrEqualTo(100.001f));
        helper.Reset();
        Assert.IsFalse(helper.WaypointChanged);
    }

    [TestCase(8f, 0.02f)]
    [TestCase(0.25f, 0.02f)]
    [TestCase(8f, 1f)]
    [TestCase(0f, 0.02f)]
    public void LocomotionReplacement_CapsRequestAndRearmsExistingSmoother(float speed, float dt)
    {
        var enemy = new GameObject("BypassSpeedEnemy");
        var blocker = new GameObject("BypassSpeedBlocker");
        var player = new GameObject("Player");
        player.tag = "Player";
        var registry = (List<EnemyController2D>)typeof(EnemyController2D)
            .GetField("All", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        EnemyController2D controller = null;
        EnemyController2D other = null;
        try
        {
            controller = ConfigureEnemy(enemy, player.transform, new Vector2(2f, 0f));
            other = ConfigureEnemy(blocker, player.transform, new Vector2(1.3f, 0f));
            if (!registry.Contains(other)) registry.Add(other);
            Physics2D.SyncTransforms();
            var locomotion = enemy.GetComponent<EnemyLocomotion>();
            Vector2 radial = Vector2.zero, tangent = Vector2.zero;
            for (int i = 0; i < 42; i++)
            {
                radial = Vector2.left * 100f;
                tangent = Vector2.up * 100f;
                Assert.IsTrue(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                    true, Vector2.up, 2f, 0.5f, speed, dt, ref radial, ref tangent, out _));
                Assert.That((radial + tangent).magnitude, Is.LessThanOrEqualTo(speed + 0.00001f));
            }
            Assert.IsFalse(locomotion.IsChaseApproachTurning);
            Vector2 previous = radial + tangent;
            blocker.GetComponent<Rigidbody2D>().position = new Vector2(1.9f, 0.2f);
            blocker.transform.position = new Vector2(1.9f, 0.2f);
            Physics2D.SyncTransforms();
            radial = Vector2.left * 100f;
            tangent = Vector2.up * 100f;
            Assert.IsTrue(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                true, Vector2.up, 2f, 0.5f, speed, dt, ref radial, ref tangent, out Vector2 point));
            float expectedSpeed = Mathf.Min(speed, Vector2.Distance(new Vector2(2f, 0f), point) / dt);
            Assert.That((radial + tangent).magnitude, Is.EqualTo(expectedSpeed).Within(0.00001f));
            Assert.That(tangent, Is.EqualTo(Vector2.zero), "No old spacing or waypoint vector may be stacked.");
            if (speed > 0f && dt == 0.02f)
            {
                Assert.IsTrue(locomotion.IsChaseApproachTurning);
                Assert.That(Vector2.Angle(previous, radial), Is.EqualTo(4.5f).Within(0.001f));
            }
            blocker.GetComponent<Rigidbody2D>().position = new Vector2(10f, 10f);
            blocker.transform.position = new Vector2(10f, 10f);
            Physics2D.SyncTransforms();
            radial = Vector2.left * speed;
            tangent = Vector2.zero;
            Assert.IsFalse(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                true, Vector2.up, 2f, 0.5f, speed, dt, ref radial, ref tangent, out _));
            Assert.That((radial + tangent).magnitude, Is.EqualTo(speed).Within(0.00001f));

            // After exit convergence, ordinary #299 output must pass through unchanged.
            enemy.GetComponent<EnemyApproachSpread>().Compute(Vector2.left * speed, Vector2.left,
                Vector2.up * 0.5f, true, Vector2.up, 2f, 0.5f, 0f, 0f, false, speed,
                out Vector2 spacingRadial, out Vector2 spacingTangent, meleePlayerChase: true);
            for (int i = 0; i < 42; i++)
            {
                radial = spacingRadial;
                tangent = spacingTangent;
                Assert.IsFalse(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                    true, Vector2.up, 2f, 0.5f, speed, dt, ref radial, ref tangent, out _));
                Assert.That((radial + tangent).magnitude, Is.LessThanOrEqualTo(speed + 0.00001f));
            }
            Assert.IsFalse(locomotion.IsChaseApproachTurning);
            Assert.That(radial, Is.EqualTo(spacingRadial));
            Assert.That(tangent, Is.EqualTo(spacingTangent));

            locomotion.ComputeBaseMovement(new Vector2(2f, 0f), player.transform, null,
                0.4f, 0.5f, 0.25f, 0.1f, speed, 0.15f, 0.5f, 3, 0.01f, 0.1f, 1f,
                out radial, out tangent);
            Assert.That(locomotion.CurrentState, Is.EqualTo(EnemyController2D.State.HOLD));
            Assert.IsFalse(locomotion.TryApplyChaseApproachTarget(controller, player.transform,
                true, Vector2.up, 0.4f, 0.5f, speed, dt, ref radial, ref tangent, out _));
            Assert.That(radial + tangent, Is.EqualTo(Vector2.zero),
                "Settled melee HOLD stays stationary after a bypass and replacement.");
            Assert.IsFalse(locomotion.HasChaseApproachTarget);
            Assert.IsFalse(locomotion.IsChaseApproachTurning);
        }
        finally
        {
            registry.Remove(controller);
            registry.Remove(other);
            UnityEngine.Object.DestroyImmediate(enemy);
            UnityEngine.Object.DestroyImmediate(blocker);
            UnityEngine.Object.DestroyImmediate(player);
        }
    }

    private static EnemyController2D ConfigureEnemy(GameObject enemy, Transform player, Vector2 position)
    {
        enemy.transform.position = position;
        enemy.AddComponent<Rigidbody2D>().gravityScale = 0f;
        enemy.AddComponent<CircleCollider2D>().radius = 0.2f;
        var spread = enemy.AddComponent<EnemyApproachSpread>();
        var controller = enemy.AddComponent<EnemyController2D>();
        typeof(EnemyController2D).GetField("approachSpread", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(controller, spread);
        controller.Debug_SetupRefs(player);
        controller.Debug_SetTargetDecision(player, player, EnemyTargetType.Player);
        return controller;
    }

    [Test]
    public void LocomotionDisableCallback_DiscardsApproachState()
    {
        var enemy = new GameObject("ApproachLifecycleEnemy");
        try
        {
            var locomotion = enemy.AddComponent<EnemyLocomotion>();
            var helper = (EnemyChaseApproachTarget)typeof(EnemyLocomotion)
                .GetField("chaseApproachTarget", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(locomotion);
            Assert.IsTrue(Resolve(helper, blocked, out _));
            var transition = (EnemyBypassDirectionTransition)typeof(EnemyLocomotion)
                .GetField("bypassDirectionTransition", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(locomotion);
            transition.Apply(Vector2.up, true, 0.02f, Vector2.right);
            Assert.IsTrue(locomotion.IsChaseApproachTurning);
            // EditMode does not dispatch this non-ExecuteAlways lifecycle reliably.
            typeof(EnemyLocomotion).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(locomotion, null);
            Assert.IsFalse(helper.IsActive);
            Assert.IsFalse(locomotion.IsChaseApproachTurning);
            Assert.That(helper.Side, Is.Zero);
            Assert.That(helper.Offset, Is.EqualTo(Vector2.zero));
        }
        finally { UnityEngine.Object.DestroyImmediate(enemy); }
    }

}

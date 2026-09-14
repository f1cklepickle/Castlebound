using NUnit.Framework;
using UnityEngine;

public class EnemyBypassDirectionTransitionTests
{
    [Test]
    public void Entry_RateLimitsHeadingWithoutChangingRequestedSpeed()
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.right * 3f, false, 0.02f, Vector2.zero);
        Vector2 result = transition.Apply(Vector2.up * 3f, true, 0.02f, Vector2.zero);
        Assert.That(Vector2.Angle(Vector2.right, result), Is.EqualTo(4.5f).Within(0.001f));
        Assert.That(result.magnitude, Is.EqualTo(3f).Within(0.0001f));
        Assert.IsTrue(transition.IsTurning);
    }

    [TestCase(90f)]
    [TestCase(-90f)]
    [TestCase(180f)]
    public void FixedHeading_ConvergesWithinAngularDistanceOverTurnRate(float degrees)
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.right * 8f, false, 0.02f, Vector2.zero);
        Vector2 desired = new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad)) * 8f;
        Vector2 previous = Vector2.right * 8f;
        int maximumSteps = Mathf.CeilToInt(Mathf.Abs(degrees) / (225f * 0.02f)) + 1;
        Vector2 result = previous;
        for (int i = 0; i < maximumSteps; i++)
        {
            result = transition.Apply(desired, true, 0.02f, Vector2.zero);
            Assert.That(Vector2.Angle(previous, result), Is.LessThanOrEqualTo(4.501f));
            Assert.That(result.magnitude, Is.EqualTo(8f).Within(0.0001f));
            previous = result;
        }
        Assert.IsFalse(transition.IsTurning);
        Assert.That(result, Is.EqualTo(desired));
    }

    [Test]
    public void Exit_AlsoTurnsGraduallyThenRestoresExactDirectRequest()
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.up * 3f, true, 1f, Vector2.up);
        Vector2 desired = Vector2.right * 3f;
        Vector2 result = transition.Apply(desired, false, 0.02f, Vector2.zero);
        Assert.That(Vector2.Angle(Vector2.up, result), Is.EqualTo(4.5f).Within(0.001f));
        Assert.That(result.magnitude, Is.EqualTo(3f).Within(0.0001f));
        for (int i = 0; i < 20; i++) result = transition.Apply(desired, false, 0.02f, Vector2.zero);
        Assert.IsFalse(transition.IsTurning);
        Assert.That(result, Is.EqualTo(desired));
        Vector2 newDirect = Vector2.down * 2f;
        Assert.That(transition.Apply(newDirect, false, 0.02f, Vector2.zero), Is.EqualTo(newDirect),
            "After convergence, ordinary CHASE direction changes must not be rate-limited.");
    }

    [Test]
    public void DirectChaseWithoutBypass_IsExactEvenForSuddenDirectionChanges()
    {
        var transition = new EnemyBypassDirectionTransition();
        foreach (Vector2 velocity in new[] { new Vector2(3f, 0.2f), Vector2.down * 8f, Vector2.left })
            Assert.That(transition.Apply(velocity, false, 0.02f, Vector2.zero), Is.EqualTo(velocity));
        Assert.IsFalse(transition.IsTurning);
    }

    [Test]
    public void EstablishedBypass_WithoutReplacementSignal_RemainsPassthrough()
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.up * 3f, true, 1f, Vector2.right);
        Assert.IsFalse(transition.IsTurning);
        Vector2 redirect = Vector2.left * 3f;
        Assert.That(transition.Apply(redirect, true, 0.02f, Vector2.zero), Is.EqualTo(redirect));
    }

    [TestCase(8f)]
    [TestCase(0.25f)]
    public void WaypointReplacement_RearmsTurnAndPreservesNewMagnitude(float speed)
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.up * 8f, true, 1f, Vector2.up);
        Assert.IsFalse(transition.IsTurning);
        Vector2 desired = Vector2.right * speed;
        Vector2 previous = Vector2.up * 8f;
        for (int i = 0; i < 21; i++)
        {
            Vector2 result = transition.Apply(desired, true, 0.02f, Vector2.left * 100f,
                waypointChanged: i == 0);
            Assert.That(Vector2.Angle(previous, result), Is.LessThanOrEqualTo(4.501f));
            Assert.That(result.magnitude, Is.EqualTo(speed).Within(0.0001f),
                "Replacement replaces the old velocity; it must never add either input vector.");
            if (i == 0) Assert.IsTrue(transition.IsTurning);
            previous = result;
        }
        Assert.IsFalse(transition.IsTurning);
        Assert.That(previous, Is.EqualTo(desired));
    }

    [Test]
    public void EntryReplacementAndExit_EveryTransitionPreservesAuthoredSpeed()
    {
        var transition = new EnemyBypassDirectionTransition();
        Vector2 previous = Vector2.right * 8f;
        transition.Apply(previous, false, 0.02f, Vector2.zero);
        for (int phase = 0; phase < 3; phase++)
        {
            Vector2 desired = (phase == 0 ? Vector2.up : phase == 1 ? Vector2.left : Vector2.right) * 8f;
            for (int tick = 0; tick < 42; tick++)
            {
                Vector2 result = transition.Apply(desired, phase != 2, 0.02f, Vector2.zero,
                    waypointChanged: phase == 1 && tick == 0);
                Assert.That(result.magnitude, Is.EqualTo(8f).Within(0.0001f));
                Assert.That(Vector2.Angle(previous, result), Is.LessThanOrEqualTo(4.501f));
                previous = result;
            }
            Assert.That(previous, Is.EqualTo(desired));
            Assert.IsFalse(transition.IsTurning);
        }
    }

    [Test]
    public void FirstBypass_UsesOrdinaryChaseAsItsInitialDirection()
    {
        var transition = new EnemyBypassDirectionTransition();
        Vector2 result = transition.Apply(Vector2.up * 3f, true, 0.02f, Vector2.right);
        Assert.That(Vector2.Angle(Vector2.right, result), Is.EqualTo(4.5f).Within(0.001f));
    }

    [Test]
    public void ZeroMovementOrReset_DiscardsTurningWithoutCoasting()
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.up * 3f, true, 0.02f, Vector2.right);
        Assert.That(transition.Apply(Vector2.zero, false, 0.02f, Vector2.zero), Is.EqualTo(Vector2.zero));
        Assert.IsFalse(transition.IsTurning);
        transition.Apply(Vector2.up * 3f, true, 0.02f, Vector2.right);
        transition.Reset();
        Assert.IsFalse(transition.IsTurning);
        Assert.That(transition.Apply(Vector2.down, false, 0.02f, Vector2.zero), Is.EqualTo(Vector2.down));
    }

    [Test]
    public void TargetChange_DiscardsTheOldMovementHeading()
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.right * 3f, true, 1f, Vector2.right, 1);
        Vector2 result = transition.Apply(Vector2.down * 3f, false, 0.02f, Vector2.zero, 2);
        Assert.That(result, Is.EqualTo(Vector2.down * 3f));
        Assert.IsFalse(transition.IsTurning);
    }

    [Test]
    public void DirectionOnly_PreservesExistingWaypointStepCapAndNewSpeedRequests()
    {
        var transition = new EnemyBypassDirectionTransition();
        transition.Apply(Vector2.right * 8f, false, 0.02f, Vector2.zero);
        Assert.That(transition.Apply(Vector2.up * 0.25f, true, 0.02f, Vector2.zero).magnitude,
            Is.EqualTo(0.25f).Within(0.0001f));
        Assert.That(transition.Apply(Vector2.up * 5f, true, 0.02f, Vector2.zero).magnitude,
            Is.EqualTo(5f).Within(0.0001f));
    }
}

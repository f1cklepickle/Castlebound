#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyPredictiveChasePlayTests
{
    private readonly List<GameObject> objects = new List<GameObject>();
    private readonly List<EnemyRingManager> paused = new List<EnemyRingManager>();
    private Transform player;

    [SetUp]
    public void SetUp()
    {
        foreach (var manager in Object.FindObjectsOfType<EnemyRingManager>())
        {
            if (!manager.isActiveAndEnabled) continue;
            paused.Add(manager);
            manager.enabled = false;
        }
        var go = new GameObject("PredictivePlayer");
        objects.Add(go);
        go.tag = "Player";
        go.layer = LayerMask.NameToLayer("Player");
        go.transform.position = new Vector2(1000f, 1000f);
        go.AddComponent<CircleCollider2D>().radius = 0.5f;
        player = go.transform;
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--) Object.DestroyImmediate(objects[i]);
        objects.Clear();
        foreach (var manager in paused)
            if (manager != null) manager.enabled = true;
        paused.Clear();
    }

    [TestCase("hold")]
    [TestCase("ranged")]
    [TestCase("barrier")]
    [TestCase("root")]
    [TestCase("steerElsewhere")]
    public void NonExperimentalPaths_AreExcluded(string kind)
    {
        var enemy = CreateEnemy(new Vector2(4f, 0f));
        var movement = enemy.GetComponent<EnemyLocomotion>();
        Assert.IsTrue(movement.CanUsePredictiveChase(enemy, player));
        switch (kind)
        {
            case "hold": movement.SetMovementState(EnemyController2D.State.HOLD); break;
            case "ranged":
                movement.Debug_SetHoldMovementPolicy(enemy.gameObject.AddComponent<EnemyRangedEngagement>());
                break;
            case "barrier": enemy.Debug_SetTargetDecision(player, player, EnemyTargetType.Barrier); break;
            case "root": enemy.GetComponent<EnemyRootReceiver>().RootForSeconds(2f); break;
            case "steerElsewhere":
                var blocker = new GameObject("OtherSteerTarget");
                objects.Add(blocker);
                enemy.Debug_SetTargetDecision(blocker.transform, player, EnemyTargetType.Player);
                break;
        }
        Assert.IsFalse(movement.CanUsePredictiveChase(enemy, player));
        Vector2 radial = Vector2.left, tangent = Vector2.up;
        movement.ApplyPredictiveChase(enemy, player, 2f, ref radial, ref tangent);
        Assert.That(radial, Is.EqualTo(Vector2.left));
        Assert.That(tangent, Is.EqualTo(Vector2.up));
    }

    [UnityTest]
    public IEnumerator FiveSameSideArrivals_RearEnemyReachesStationaryHold()
    {
        yield return CheckArrivals(5);
    }

    [UnityTest]
    public IEnumerator EightSameSideArrivals_CompressAndReachStationaryHold()
    {
        yield return CheckArrivals(8);
    }

    private IEnumerator CheckArrivals(int count)
    {
        var enemies = new EnemyController2D[count];
        var previous = new Vector2[count];
        var settled = new Vector2[count];
        var startedHolding = new bool[count];
        for (int i = 0; i < count; i++)
        {
            enemies[i] = CreateEnemy(new Vector2(4f + i * 0.55f, (i % 2 == 0 ? 1f : -1f) * 0.3f));
            previous[i] = enemies[i].GetComponent<Rigidbody2D>().position;
        }
        int allHoldTicks = 0;
        bool curvedBeforeContact = false;
        for (int tick = 0; tick < 800 && allHoldTicks < 10; tick++)
        {
            yield return new WaitForFixedUpdate();
            bool allHolding = true;
            for (int i = 0; i < count; i++)
            {
                var movement = enemies[i].GetComponent<EnemyLocomotion>();
                Vector2 position = enemies[i].GetComponent<Rigidbody2D>().position;
                Vector2 travel = position - previous[i];
                Assert.That(travel.magnitude, Is.LessThanOrEqualTo(2f * Time.fixedDeltaTime + 0.003f));
                Assert.IsFalse(movement.HasChaseApproachTarget, "Only one CHASE avoidance authority may run.");
                Assert.IsFalse(movement.IsChaseApproachTurning);
                Assert.That(enemies[i].Target, Is.SameAs(player));
                if (tick < 40 && Mathf.Abs(travel.y) > 0.003f)
                {
                    float nearest = float.PositiveInfinity;
                    for (int j = 0; j < count; j++)
                        if (i != j) nearest = Mathf.Min(nearest,
                            Vector2.Distance(position, enemies[j].GetComponent<Rigidbody2D>().position));
                    curvedBeforeContact |= nearest > 0.42f;
                }
                allHolding &= movement.IsInHoldRange;
                if (movement.IsInHoldRange && !startedHolding[i])
                {
                    startedHolding[i] = true;
                    settled[i] = position;
                }
                if (startedHolding[i])
                {
                    Assert.IsTrue(movement.IsInHoldRange, "A stationary Player must not restart CHASE.");
                    Assert.That(Vector2.Distance(position, settled[i]), Is.LessThan(0.003f));
                }
                for (int j = 0; j < i; j++)
                    Assert.That(Vector2.Distance(position, enemies[j].GetComponent<Rigidbody2D>().position),
                        Is.GreaterThanOrEqualTo(0.39f), "#277 remains the physical minimum.");
                previous[i] = position;
            }
            allHoldTicks = allHolding ? allHoldTicks + 1 : 0;
        }
        Assert.IsTrue(curvedBeforeContact, "Approach must change before hard contact.");
        Assert.That(allHoldTicks, Is.EqualTo(10), "Every arrival, including the rear enemy, must reach HOLD.");
    }

    [UnityTest]
    public IEnumerator ExistingMeleeHold_StaysStillInsidePreferredRadius()
    {
        var first = CreateEnemy(new Vector2(1.7f, 0f));
        var second = CreateEnemy(new Vector2(1.7f, 0.6f));
        Vector2 firstStart = first.GetComponent<Rigidbody2D>().position;
        Vector2 secondStart = second.GetComponent<Rigidbody2D>().position;
        for (int i = 0; i < 12; i++)
        {
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(first.IsInHoldRange());
            Assert.IsTrue(second.IsInHoldRange());
            Assert.That(first.GetComponent<Rigidbody2D>().position, Is.EqualTo(firstStart));
            Assert.That(second.GetComponent<Rigidbody2D>().position, Is.EqualTo(secondStart));
        }
    }

    private EnemyController2D CreateEnemy(Vector2 offset)
    {
        var go = new GameObject("PredictiveMelee");
        objects.Add(go);
        go.layer = LayerMask.NameToLayer("Enemies");
        go.transform.position = player.position + (Vector3)offset;
        var body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        var primary = go.AddComponent<CircleCollider2D>();
        primary.radius = 0.87684506f;
        primary.excludeLayers = 1 << go.layer;
        primary.layerOverridePriority = 2;
        var sensorObject = new GameObject("EnemySeparation");
        sensorObject.layer = go.layer;
        sensorObject.transform.SetParent(go.transform, false);
        var sensor = sensorObject.AddComponent<CircleCollider2D>();
        sensor.radius = 0.2f;
        sensor.isTrigger = true;
        sensor.includeLayers = 1 << go.layer;
        sensor.excludeLayers = ~(1 << go.layer);
        sensorObject.AddComponent<EnemySeparationCollider>();
        go.AddComponent<Health>().ConfigureMaxHealth(10, true);
        go.AddComponent<EnemyRootReceiver>();
        go.AddComponent<EnemySurroundEligibility>();
        go.AddComponent<EnemyApproachSpread>();
        var enemy = go.AddComponent<EnemyController2D>();
        enemy.Speed = 2f;
        enemy.Debug_SetBarrierTargeting(false);
        enemy.Debug_SetupRefs(player);
        enemy.Debug_SetTargetDecision(player, player, EnemyTargetType.Player);
        return enemy;
    }
}
#endif

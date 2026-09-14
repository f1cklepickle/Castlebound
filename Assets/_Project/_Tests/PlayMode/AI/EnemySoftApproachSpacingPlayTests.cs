#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemySoftApproachSpacingPlayTests
{
    private readonly List<GameObject> objects = new List<GameObject>();
    private readonly List<EnemyRingManager> pausedManagers = new List<EnemyRingManager>();
    private static readonly BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        foreach (var manager in Object.FindObjectsOfType<EnemyRingManager>())
        {
            if (!manager.isActiveAndEnabled) continue;
            pausedManagers.Add(manager);
            manager.enabled = false;
        }
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }
        finally
        {
            foreach (var manager in pausedManagers)
                if (manager != null) manager.enabled = true;
            pausedManagers.Clear();
        }
    }

    [UnityTest]
    public IEnumerator OuterNeighbor_InfluencesChaseWithoutExtendingNeighborEligibility()
    {
        yield return CheckFarChase(false);
    }

    [UnityTest]
    public IEnumerator RangedChase_RetainsUnboostedLocalContribution()
    {
        yield return CheckFarChase(true);
    }

    private IEnumerator CheckFarChase(bool ranged)
    {
        var player = CreatePlayer();
        var subject = CreateEnemy(player, new Vector2(18f, 0f), 8f);
        var neighbor = CreateEnemy(player, new Vector2(18f, ranged ? -1f : -1.75f), 0f);
        var movement = subject.GetComponent<EnemyLocomotion>();
        if (ranged) movement.Debug_SetHoldMovementPolicy(subject.gameObject.AddComponent<EnemyRangedEngagement>());
        var body = subject.GetComponent<Rigidbody2D>();
        for (int i = 0; i < 4; i++)
        {
            Feed(subject, neighbor);
            Vector2 before = body.position;
            Vector2 inward = ((Vector2)player.position - before).normalized;
            string field = ranged ? "_approachSeparation" : "_chaseSoftSeparation";
            Vector2 separation = (Vector2)typeof(EnemyController2D).GetField(field, PrivateInstance).GetValue(subject);
            bool close = (bool)typeof(EnemyController2D).GetField("_hasApproachNeighbors", PrivateInstance).GetValue(subject);
            Vector2 bias = (Vector2)typeof(EnemyController2D).GetField("_approachSpreadBias", PrivateInstance).GetValue(subject);
            if (!ranged) Assert.IsFalse(close);
            subject.GetComponent<EnemyApproachSpread>().Compute(inward * 8f, inward, separation, close, bias,
                18f, 0.5f, 0f, 0f, false, 8f, out Vector2 radial, out Vector2 tangent, meleePlayerChase: !ranged);
            Assert.That(tangent.sqrMagnitude, Is.GreaterThan(0f));
            Vector2 expectedTravel = (radial + tangent) * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
            Assert.That(Vector2.Distance(body.position - before, expectedTravel), Is.LessThan(0.0002f),
                "Actual far CHASE must consume the appropriate melee/ranged spacing request.");
            Assert.IsFalse(movement.HasChaseApproachTarget);
        }
    }

    [UnityTest]
    public IEnumerator DensePair_CanReachStationaryHoldCloserThanSoftInfluenceRadius()
    {
        var player = CreatePlayer();
        var first = CreateEnemy(player, new Vector2(4f, 0.3f), 2f);
        var second = CreateEnemy(player, new Vector2(4f, -0.3f), 2f);
        var firstBody = first.GetComponent<Rigidbody2D>();
        var secondBody = second.GetComponent<Rigidbody2D>();
        int settledTicks = 0;
        Vector2 firstSettled = Vector2.zero;
        Vector2 secondSettled = Vector2.zero;
        for (int i = 0; i < 250 && settledTicks < 10; i++)
        {
            Feed(first, second);
            yield return new WaitForFixedUpdate();
            Assert.That(Vector2.Distance(firstBody.position, secondBody.position), Is.GreaterThanOrEqualTo(0.395f));
            if (!first.IsInHoldRange() || !second.IsInHoldRange()) continue;
            if (settledTicks == 0)
            {
                firstSettled = firstBody.position;
                secondSettled = secondBody.position;
            }
            settledTicks++;
            Assert.That(Vector2.Distance(firstBody.position, firstSettled), Is.LessThan(0.001f));
            Assert.That(Vector2.Distance(secondBody.position, secondSettled), Is.LessThan(0.001f));
            Assert.That(Vector2.Distance(firstBody.position, secondBody.position), Is.LessThan(2f),
                "The soft radius must not become a minimum separation requirement.");
        }
        Assert.That(settledTicks, Is.EqualTo(10), "Both enemies must reach and remain in melee HOLD.");
        Assert.That(first.GetComponent<EnemySeparationCollider>().Collider.radius, Is.EqualTo(0.2f));
        Assert.That(second.GetComponent<EnemySeparationCollider>().Collider.radius, Is.EqualTo(0.2f));
    }

    private Transform CreatePlayer()
    {
        var player = new GameObject("SoftSpacingPlayer");
        objects.Add(player);
        player.tag = "Player";
        player.transform.position = new Vector2(1000f, 1000f);
        return player.transform;
    }

    private EnemyController2D CreateEnemy(Transform player, Vector2 offset, float speed)
    {
        var enemy = new GameObject("SoftSpacingEnemy");
        objects.Add(enemy);
        enemy.transform.position = player.position + (Vector3)offset;
        var body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        var circle = enemy.AddComponent<CircleCollider2D>();
        circle.radius = 0.2f;
        circle.isTrigger = true;
        enemy.AddComponent<EnemySeparationCollider>();
        enemy.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
        enemy.AddComponent<EnemyRootReceiver>();
        enemy.AddComponent<EnemySurroundEligibility>();
        enemy.AddComponent<EnemyApproachSpread>();
        var controller = enemy.AddComponent<EnemyController2D>();
        controller.Speed = speed;
        controller.Debug_SetBarrierTargeting(false);
        controller.Debug_SetupRefs(player);
        controller.Debug_SetTargetDecision(player, player, EnemyTargetType.Player);
        return controller;
    }

    private static void Feed(params EnemyController2D[] controllers)
    {
        // Use the real weighted feed with fixture-owned membership; restore scratch state before yielding.
        var field = typeof(EnemyRingManager).GetField("sEnemies", PrivateStatic);
        object previous = field.GetValue(null);
        try
        {
            field.SetValue(null, controllers);
            typeof(EnemyRingManager).GetMethod("FeedApproachSeparation", PrivateStatic)
                .Invoke(null, new object[] { controllers.Length });
        }
        finally { field.SetValue(null, previous); }
    }

}
#endif

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyChaseApproachTargetPlayTests
{
    [UnityTest]
    public IEnumerator OccupiedApproach_PredictsWithoutBypassThenHolds()
    {
        var paused = new List<EnemyRingManager>();
        var player = new GameObject("BypassPlayer");
        player.tag = "Player";
        player.transform.position = new Vector2(1000f, 1000f);
        GameObject subject = null, blocker = null;
        try
        {
            foreach (var manager in Object.FindObjectsOfType<EnemyRingManager>())
            {
                if (!manager.isActiveAndEnabled) continue;
                paused.Add(manager);
                manager.enabled = false;
            }
            blocker = CreateEnemy(player.transform, new Vector2(1.3f, 0f), 0f);
            subject = CreateEnemy(player.transform, new Vector2(2f, 0f), 2f);
            var controller = subject.GetComponent<EnemyController2D>();
            var movement = subject.GetComponent<EnemyLocomotion>();
            var body = subject.GetComponent<Rigidbody2D>();
            Vector2 start = body.position;
            yield return new WaitForFixedUpdate();
            Assert.IsFalse(movement.HasChaseApproachTarget);
            Assert.IsFalse(movement.IsChaseApproachTurning);
            // Predictive CHASE must curve before contact without activating legacy bypass.
            Vector2 travel = body.position - start;
            Assert.That(travel.magnitude / Time.fixedDeltaTime, Is.EqualTo(2f).Within(0.01f));
            Assert.That(Mathf.Abs(travel.y), Is.GreaterThan(0.001f));
            Assert.That(travel.x, Is.LessThan(0f));

            controller.RequestChase();
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(movement.CanUsePredictiveChase(controller, player.transform));
            Assert.IsFalse(movement.HasChaseApproachTarget, "Reacquisition must not stack legacy routing.");
            controller.ClearChaseRequest();
            int holdTicks = 0;
            Vector2 settled = Vector2.zero;
            for (int i = 0; i < 220 && holdTicks < 12; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(Vector2.Distance(body.position, blocker.transform.position),
                    Is.GreaterThanOrEqualTo(0.395f), "#277 minimum footprints remain authoritative.");
                Assert.That(controller.Target, Is.EqualTo(player.transform));
                Assert.That(subject.GetComponent<EnemyTargeting>().SteerTarget, Is.EqualTo(player.transform));
                if (!controller.IsInHoldRange()) continue;
                if (holdTicks == 0) settled = body.position;
                holdTicks++;
                Assert.IsFalse(movement.HasChaseApproachTarget);
                Assert.IsFalse(movement.IsChaseApproachTurning);
                Assert.That(Vector2.Distance(body.position, settled), Is.LessThan(0.001f));
                controller.SetAngularGaps((holdTicks & 1) == 0 ? 0.1f : 0.5f,
                    (holdTicks & 1) == 0 ? 0.5f : 0.1f, 2);
            }
            Assert.That(holdTicks, Is.EqualTo(12), "Arrival must produce sustained stationary HOLD.");
        }
        finally
        {
            if (subject != null) Object.DestroyImmediate(subject);
            if (blocker != null) Object.DestroyImmediate(blocker);
            Object.DestroyImmediate(player);
            foreach (var manager in paused)
                if (manager != null) manager.enabled = true;
        }
    }

    [UnityTest]
    public IEnumerator SameDirectionArrivals_PredictBeforeContact()
    {
        var paused = new List<EnemyRingManager>();
        var player = new GameObject("EarlyBypassPlayer");
        player.tag = "Player";
        player.transform.position = new Vector2(1000f, 1000f);
        GameObject upper = null, lower = null, blocker = null;
        try
        {
            foreach (var manager in Object.FindObjectsOfType<EnemyRingManager>())
            {
                if (!manager.isActiveAndEnabled) continue;
                paused.Add(manager);
                manager.enabled = false;
            }
            blocker = CreateEnemy(player.transform, new Vector2(0.9f, 0f), 0f);
            upper = CreateEnemy(player.transform, new Vector2(3.2f, 0.3f), 2f);
            lower = CreateEnemy(player.transform, new Vector2(3.2f, -0.3f), 2f);
            var upperBody = upper.GetComponent<Rigidbody2D>();
            var lowerBody = lower.GetComponent<Rigidbody2D>();
            Vector2 upperStart = upperBody.position, lowerStart = lowerBody.position;
            Vector2 upperPrevious = upperStart, lowerPrevious = lowerStart;
            for (int i = 0; i < 6; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.IsFalse(upper.GetComponent<EnemyLocomotion>().HasChaseApproachTarget);
                Assert.IsFalse(lower.GetComponent<EnemyLocomotion>().HasChaseApproachTarget);
                Assert.That(Vector2.Distance(upperBody.position, blocker.transform.position), Is.GreaterThan(1.5f));
                Assert.That(Vector2.Distance(lowerBody.position, blocker.transform.position), Is.GreaterThan(1.5f));
                Assert.That(Vector2.Distance(upperBody.position, lowerBody.position), Is.GreaterThanOrEqualTo(0.395f));
                Assert.That(Vector2.Distance(upperBody.position, upperPrevious),
                    Is.LessThanOrEqualTo(2f * Time.fixedDeltaTime + 0.001f));
                Assert.That(Vector2.Distance(lowerBody.position, lowerPrevious),
                    Is.LessThanOrEqualTo(2f * Time.fixedDeltaTime + 0.001f));
                upperPrevious = upperBody.position;
                lowerPrevious = lowerBody.position;
            }
            Assert.That(upperBody.position.y - upperStart.y, Is.GreaterThan(0.005f));
            Assert.That(lowerStart.y - lowerBody.position.y, Is.GreaterThan(0.005f));
            Assert.That(upperBody.position.y - lowerBody.position.y,
                Is.GreaterThan(upperStart.y - lowerStart.y + 0.01f),
                "Arrivals must diverge while the front blocker remains outside the old 1.5-unit range.");
        }
        finally
        {
            if (upper != null) Object.DestroyImmediate(upper);
            if (lower != null) Object.DestroyImmediate(lower);
            if (blocker != null) Object.DestroyImmediate(blocker);
            Object.DestroyImmediate(player);
            foreach (var manager in paused)
                if (manager != null) manager.enabled = true;
        }
    }

    private static GameObject CreateEnemy(Transform player, Vector2 offset, float speed)
    {
        var enemy = new GameObject("ApproachTestEnemy");
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
        enemy.AddComponent<EnemySurroundEligibility>().AvoidanceGroup = PredictiveAvoidanceGroup.SmallMelee;
        enemy.AddComponent<EnemyApproachSpread>();
        var controller = enemy.AddComponent<EnemyController2D>();
        controller.Speed = speed;
        controller.Debug_SetBarrierTargeting(false);
        controller.Debug_SetupRefs(player);
        controller.Debug_SetTargetDecision(player, player, EnemyTargetType.Player);
        return enemy;
    }
}
#endif

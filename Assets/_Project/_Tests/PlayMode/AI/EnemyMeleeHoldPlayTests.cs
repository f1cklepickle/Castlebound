#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyMeleeHoldPlayTests
{
    [UnityTest]
    public IEnumerator ControllerHandoff_MeleeHoldStaysStillDespiteChangingGaps()
    {
        var player = new GameObject("HoldPlayer");
        player.tag = "Player";
        player.transform.position = new Vector2(1000f, 1000f);
        var enemy = new GameObject("HoldEnemy");
        var paused = new List<EnemyRingManager>();
        try
        {
            foreach (var manager in Object.FindObjectsOfType<EnemyRingManager>())
            {
                if (!manager.isActiveAndEnabled) continue;
                paused.Add(manager);
                manager.enabled = false;
            }
            enemy.transform.position = player.transform.position + Vector3.right;
            var body = enemy.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            enemy.AddComponent<Health>().ConfigureMaxHealth(10, refill: true);
            enemy.AddComponent<EnemyRootReceiver>();
            enemy.AddComponent<EnemySurroundEligibility>().AvoidanceGroup = PredictiveAvoidanceGroup.SmallMelee;
            enemy.AddComponent<EnemyApproachSpread>();
            var controller = enemy.AddComponent<EnemyController2D>();
            controller.Speed = 3f;
            controller.Debug_SetBarrierTargeting(false);
            controller.Debug_SetupRefs(player.transform);
            controller.Debug_SetTargetDecision(player.transform, player.transform, EnemyTargetType.Player);
            controller.SetAngularGaps(0.1f, 0.5f, 2);
            controller.SetApproachSeparation(Vector2.up, true);
            Vector2 initial = body.position, settled = Vector2.zero;
            int holdTicks = 0;
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
                if (!controller.IsInHoldRange())
                {
                    Assert.That(holdTicks, Is.Zero, "Stationary Player must not cause HOLD to restart CHASE.");
                    continue;
                }
                if (holdTicks == 0) settled = body.position;
                holdTicks++;
                Assert.That(Vector2.Distance(body.position, settled), Is.LessThan(0.001f));
                controller.SetAngularGaps((holdTicks & 1) == 0 ? 0.1f : 0.5f,
                    (holdTicks & 1) == 0 ? 0.5f : 0.1f, 2);
            }
            Assert.That(holdTicks, Is.GreaterThan(12));
            Assert.That(Vector2.Distance(initial, settled), Is.GreaterThan(0.1f), "Must approach before settling.");
        }
        finally
        {
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(player);
            foreach (var manager in paused)
                if (manager != null) manager.enabled = true;
        }
    }

    [UnityTest]
    public IEnumerator LocomotionRequest_StillPassesThrough277MinimumFootprintClamp()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = Vector2.right * 10f;
        GameObject left = null;
        GameObject right = null;
        try
        {
            left = CreateControlledMelee(Vector2.zero);
            right = CreateControlledMelee(Vector2.right * 0.38f);
            for (int i = 0; i < 3; i++) yield return new WaitForFixedUpdate();
            var body = left.GetComponent<Rigidbody2D>();
            var locomotion = left.GetComponent<EnemyLocomotion>();
            var sensor = left.GetComponentInChildren<EnemySeparationCollider>();
            var otherSensor = right.GetComponentInChildren<EnemySeparationCollider>();
            float minimum = sensor.Collider.bounds.extents.x + otherSensor.Collider.bounds.extents.x;

            // Inject a movement request to verify execution still respects #277.
            bool clampObserved = false;
            for (int i = 0; i < 8; i++)
            {
                Vector2 inward = ((Vector2)player.transform.position - body.position).normalized;
                Vector2 ccw = new Vector2(inward.y, -inward.x);
                Vector2 tangent = ccw;
                Vector2 radial = Vector2.right * 2f;
                Vector2 requested = (radial + tangent) * Time.fixedDeltaTime;
                Vector2 constrained = sensor.ConstrainLocomotionDisplacement(requested);
                clampObserved |= (requested - constrained).sqrMagnitude > 0.000001f;
                locomotion.ExecuteMovement(body, radial, tangent, Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
                Assert.That(Vector2.Distance(sensor.Collider.bounds.center, otherSensor.Collider.bounds.center),
                    Is.GreaterThanOrEqualTo(minimum - 0.005f));
            }
            Assert.IsTrue(clampObserved, "Locomotion execution must not bypass #277.");
        }
        finally
        {
            if (left != null) Object.DestroyImmediate(left);
            if (right != null) Object.DestroyImmediate(right);
            Object.DestroyImmediate(player);
        }
    }

    private static GameObject CreateControlledMelee(Vector2 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab");
        Assert.NotNull(prefab);
        var enemy = Object.Instantiate(prefab, position, Quaternion.identity);
        enemy.GetComponent<EnemyController2D>().enabled = false;
        enemy.GetComponent<EnemyAttack>().enabled = false;
        return enemy;
    }
}
#endif

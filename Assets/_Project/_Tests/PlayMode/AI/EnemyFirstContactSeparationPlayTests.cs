using System.Collections;
using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyFirstContactSeparationPlayTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly Vector2 origin = new Vector2(2200f, 2200f);

        [TearDown]
        public void TearDown()
        {
            foreach (var item in objects) if (item != null) Object.DestroyImmediate(item);
            objects.Clear(); Physics2D.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator MixedFirstContact_UsesOneBoundedMove_WithoutFallback()
        {
            var slow = Enemy("Enemy_Lurker", Vector2.zero);
            var fast = Enemy("Enemy_Goblin_Melee", Vector2.left * 0.45f);
            for (int tick = 0; tick < 20; tick++)
            {
                Vector2 beforeSlow = slow.Body.position, beforeFast = fast.Body.position;
                Move(slow, Vector2.right * 3f); Move(fast, Vector2.right * 8f);
                yield return new WaitForFixedUpdate();
                AssertStep(slow, beforeSlow); AssertStep(fast, beforeFast);
                Assert.That(Vector2.Distance(slow.Body.position, fast.Body.position), Is.GreaterThanOrEqualTo(0.395f));
                Assert.That(slow.DebugFallbackCorrectionCount + fast.DebugFallbackCorrectionCount, Is.Zero);
                Assert.That(slow.DebugMovesLastStep, Is.EqualTo(1));
                Assert.That(fast.DebugMovesLastStep, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator ExistingOverlap_RecoversOverSeveralSpeedBoundedTicks()
        {
            var first = Enemy("Enemy_Lurker", Vector2.zero);
            var second = Enemy("Enemy_Lurker", Vector2.right * 0.1f);
            first.Owner.Speed = second.Owner.Speed = 0.5f;
            float previous = Vector2.Distance(first.Body.position, second.Body.position);
            for (int tick = 0; tick < 40; tick++)
            {
                Vector2 a = first.Body.position, b = second.Body.position;
                yield return new WaitForFixedUpdate();
                AssertStep(first, a); AssertStep(second, b);
                float distance = Vector2.Distance(first.Body.position, second.Body.position);
                Assert.That(distance, Is.GreaterThanOrEqualTo(previous - 0.001f)); previous = distance;
                if (tick == 0) Assert.That(distance, Is.LessThan(0.395f), "Recovery must not invent a one-tick speed boost.");
            }
            Assert.That(previous, Is.GreaterThanOrEqualTo(0.395f));
        }

        [UnityTest]
        public IEnumerator WallAdjacentCoincidence_UsesLegalSide_WithoutWorldPenetration()
        {
            var wall = new GameObject("RecoveryWall"); objects.Add(wall); wall.layer = LayerMask.NameToLayer("Walls");
            wall.transform.position = origin + Vector2.left * 1.09f;
            var box = wall.AddComponent<BoxCollider2D>(); box.size = new Vector2(0.4f, 4f);
            var first = Enemy("Enemy_Lurker", Vector2.zero);
            var second = Enemy("Enemy_Goblin_Melee", Vector2.zero);
            Physics2D.SyncTransforms();
            Assert.That(Physics2D.Distance(first.Body.GetComponent<CircleCollider2D>(), box).distance,
                Is.GreaterThanOrEqualTo(-0.002f), "Fixture must start outside the wall before recovery.");
            Assert.That(Physics2D.Distance(second.Body.GetComponent<CircleCollider2D>(), box).distance,
                Is.GreaterThanOrEqualTo(-0.002f), "Fixture must start outside the wall before recovery.");
            for (int tick = 0; tick < 20; tick++)
            {
                Vector2 a = first.Body.position, b = second.Body.position;
                yield return new WaitForFixedUpdate(); AssertStep(first, a); AssertStep(second, b);
                Assert.That(Physics2D.Distance(first.Body.GetComponent<CircleCollider2D>(), box).distance, Is.GreaterThanOrEqualTo(-0.002f));
                Assert.That(Physics2D.Distance(second.Body.GetComponent<CircleCollider2D>(), box).distance, Is.GreaterThanOrEqualTo(-0.002f));
            }
            Assert.That(Vector2.Distance(first.Body.position, second.Body.position), Is.GreaterThanOrEqualTo(0.395f));
        }

        [UnityTest]
        public IEnumerator HoldRootAndStagger_StayFixed_WhileAvailableNeighborRecovers()
        {
            var held = Enemy("Enemy_Goblin_Melee", Vector2.zero);
            var neighbor = Enemy("Enemy_Goblin_Melee", Vector2.right * 0.2f);
            held.Body.GetComponent<EnemyLocomotion>().SetMovementState(EnemyController2D.State.HOLD);
            Vector2 start = held.Body.position;
            for (int tick = 0; tick < 8; tick++) yield return new WaitForFixedUpdate();
            Assert.That(held.Body.position, Is.EqualTo(start));
            Assert.That(Vector2.Distance(start, neighbor.Body.position), Is.GreaterThanOrEqualTo(0.395f));
            held.Body.GetComponent<EnemyLocomotion>().SetMovementState(EnemyController2D.State.CHASE);
            var root = held.Body.GetComponent<EnemyRootReceiver>() ?? held.Body.gameObject.AddComponent<EnemyRootReceiver>();
            root.RootAt(start, 100f);
            Move(held, Vector2.right * 3f); yield return new WaitForFixedUpdate();
            Assert.That(held.Body.position, Is.EqualTo(start)); root.ClearRoot();
            Assert.IsTrue(held.Body.GetComponent<EnemyStaggerReceiver>().TryStagger());
            Move(held, Vector2.right * 3f); yield return new WaitForFixedUpdate();
            Assert.That(held.Body.position, Is.EqualTo(start));
        }

        [UnityTest]
        public IEnumerator Knockback_IsConsumedOnce_OutsideLocomotionBudget_WithoutTransfer()
        {
            var first = Enemy("Enemy_Lurker", Vector2.zero);
            var neighbor = Enemy("Enemy_Goblin_Melee", Vector2.right * 0.45f);
            var knockback = first.Body.GetComponent<EnemyKnockbackReceiver>();
            knockback.AddKnockback(Vector2.right * 8f, 5f);
            Vector2 a = first.Body.position, b = neighbor.Body.position;
            Move(first, Vector2.zero); Move(first, Vector2.zero);
            yield return new WaitForFixedUpdate();
            Assert.That(first.Body.position.x - a.x, Is.EqualTo(8f * Time.fixedDeltaTime).Within(0.001f));
            Assert.That(neighbor.Body.position, Is.EqualTo(b), "Contact must not transfer the knockback impulse.");
            Assert.That(first.DebugMovesLastStep, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TeleportedOverlap_RecoversThroughLaterOwnedSteps()
        {
            var first = Enemy("Enemy_Lurker", Vector2.zero);
            var second = Enemy("Enemy_Goblin_Melee", Vector2.right);
            yield return new WaitForFixedUpdate();
            // Explicit external writer: its teleport is not charged to locomotion.
            second.Body.position = first.Body.position;
            Physics2D.SyncTransforms();
            float previous = 0f;
            for (int tick = 0; tick < 12; tick++)
            {
                Vector2 a = first.Body.position, b = second.Body.position;
                yield return new WaitForFixedUpdate();
                AssertStep(first, a); AssertStep(second, b);
                float distance = Vector2.Distance(first.Body.position, second.Body.position);
                Assert.That(distance, Is.GreaterThanOrEqualTo(previous - 0.001f)); previous = distance;
            }
            Assert.That(previous, Is.GreaterThanOrEqualTo(0.395f));
        }

        private EnemySeparationCollider Enemy(string name, Vector2 offset)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/" + name + ".prefab");
            Assert.NotNull(prefab);
            var go = Object.Instantiate(prefab, origin + offset, Quaternion.identity); objects.Add(go);
            go.GetComponent<EnemyController2D>().enabled = false;
            var attack = go.GetComponent<EnemyAttack>(); if (attack != null) attack.enabled = false;
            Physics2D.SyncTransforms();
            return go.GetComponentInChildren<EnemySeparationCollider>();
        }
        private static void Move(EnemySeparationCollider sensor, Vector2 velocity)
            => sensor.Body.GetComponent<EnemyLocomotion>().ExecuteMovement(sensor.Body, velocity, Vector2.zero, Time.fixedDeltaTime);
        private static void AssertStep(EnemySeparationCollider sensor, Vector2 before)
            => Assert.That(Vector2.Distance(before, sensor.Body.position), Is.LessThanOrEqualTo(sensor.Owner.Speed * Time.fixedDeltaTime + 0.001f));
    }
}

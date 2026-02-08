using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.Castle
{
    public class BarrierPulseEmitterTests
    {
        [Test]
        public void EmitsOnPressureTrigger()
        {
            var barrier = new GameObject("Barrier");
            var health = barrier.AddComponent<BarrierHealth>();
            var tracker = barrier.AddComponent<Castlebound.Gameplay.Castle.BarrierPressureTracker>();
            tracker.Debug_ForceInitialize();
            var emitter = AddEmitter(barrier);
            Invoke(emitter, "Debug_ForceSubscribe");

            SetPrivateField(tracker, "breaksPerWave", 3);

            BreakBarrier(health);
            BreakBarrier(health);
            BreakBarrier(health);

            Assert.IsTrue(GetBoolProperty(emitter, "IsPulseActive"), "Pulse should activate after pressure trigger.");

            UnityEngine.Object.DestroyImmediate(barrier);
        }

        [Test]
        public void PushesEnemiesOutsideOnly()
        {
            var region = CreateRegionTracker(out var regionGO);
            var barrier = new GameObject("Barrier");
            var origin = new GameObject("Origin");
            origin.transform.SetParent(barrier.transform, false);
            origin.transform.localPosition = Vector3.zero;

            var emitter = AddEmitter(barrier);
            SetPulseTuning(emitter, origin.transform, 1.5f, 10f, 5f);
            Invoke(emitter, "Debug_StartPulse");

            var outsideEnemy = CreateEnemy(new Vector2(5f, 0f));
            var insideEnemy = CreateEnemy(Vector2.zero);

            SimulateEnter(region, insideEnemy.GetComponent<BoxCollider2D>());

            Invoke(emitter, "Debug_TickPulse", 0.8f);

            var outsideKnockback = outsideEnemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f);
            var insideKnockback = insideEnemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f);

            Assert.Greater(outsideKnockback.magnitude, 0.01f, "Outside enemy should receive push.");
            Assert.Less(insideKnockback.magnitude, 0.001f, "Inside enemy should not be pushed.");

            UnityEngine.Object.DestroyImmediate(regionGO);
            UnityEngine.Object.DestroyImmediate(barrier);
            UnityEngine.Object.DestroyImmediate(outsideEnemy);
            UnityEngine.Object.DestroyImmediate(insideEnemy);
        }

        [Test]
        public void PulseContinuesWhileWavefrontIsActive()
        {
            var region = CreateRegionTracker(out var regionGO);
            var barrier = new GameObject("Barrier");
            var origin = new GameObject("Origin");
            origin.transform.SetParent(barrier.transform, false);
            origin.transform.localPosition = Vector3.zero;

            var emitter = AddEmitter(barrier);
            SetPulseTuning(emitter, origin.transform, 2.0f, 10f, 5f);
            Invoke(emitter, "Debug_StartPulse");

            var enemy = CreateEnemy(new Vector2(5f, 0f));

            Invoke(emitter, "Debug_TickPulse", 1.0f);
            var firstPush = enemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f).magnitude;

            Invoke(emitter, "Debug_TickPulse", 0.1f);
            var secondPush = enemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f).magnitude;

            Assert.Greater(firstPush, 0.01f, "Enemy should receive push when wavefront reaches it.");
            Assert.GreaterOrEqual(secondPush, firstPush * 0.8f, "Enemy should keep receiving strong push while wavefront remains active.");

            UnityEngine.Object.DestroyImmediate(regionGO);
            UnityEngine.Object.DestroyImmediate(barrier);
            UnityEngine.Object.DestroyImmediate(enemy);
        }

        [Test]
        public void UsesInnerOriginDirection()
        {
            var region = CreateRegionTracker(out var regionGO);
            var barrier = new GameObject("Barrier");
            var origin = new GameObject("Origin");
            origin.transform.SetParent(barrier.transform, false);
            origin.transform.localPosition = Vector3.zero;

            var emitter = AddEmitter(barrier);
            SetPulseTuning(emitter, origin.transform, 1.0f, 10f, 5f);
            Invoke(emitter, "Debug_StartPulse");

            var enemy = CreateEnemy(new Vector2(5f, 0f));

            Invoke(emitter, "Debug_TickPulse", 0.6f);

            var velocity = enemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f);
            var dir = ((Vector2)enemy.transform.position - (Vector2)origin.transform.position).normalized;
            float dot = Vector2.Dot(velocity.normalized, dir);

            Assert.Greater(dot, 0.5f, "Push should be outward from the origin.");

            UnityEngine.Object.DestroyImmediate(regionGO);
            UnityEngine.Object.DestroyImmediate(barrier);
            UnityEngine.Object.DestroyImmediate(enemy);
        }

        [Test]
        public void StaysActiveAcrossConfiguredLoops()
        {
            var barrier = new GameObject("Barrier");
            var emitter = AddEmitter(barrier);
            SetPulseTuning(emitter, barrier.transform, 1f, 3f, 5f, 3);

            Invoke(emitter, "Debug_StartPulse");

            Invoke(emitter, "Debug_TickPulse", 1.1f);
            Assert.IsTrue(GetBoolProperty(emitter, "IsPulseActive"), "Pulse should remain active after first loop.");

            Invoke(emitter, "Debug_TickPulse", 1.1f);
            Assert.IsTrue(GetBoolProperty(emitter, "IsPulseActive"), "Pulse should remain active after second loop.");

            Invoke(emitter, "Debug_TickPulse", 1.1f);
            Assert.IsFalse(GetBoolProperty(emitter, "IsPulseActive"), "Pulse should stop after final configured loop.");

            UnityEngine.Object.DestroyImmediate(barrier);
        }

        [Test]
        public void RespectsPulseInsideThreshold()
        {
            var barrier = new GameObject("Barrier");
            var barrierCollider = barrier.AddComponent<BoxCollider2D>();
            barrierCollider.size = new Vector2(2f, 2f);
            barrier.AddComponent<SpriteRenderer>();

            var hold = barrier.AddComponent<EnemyBarrierHoldBehavior>();
            var anchor = new GameObject("Anchor");
            anchor.transform.position = new Vector2(2f, 0f);
            hold.Debug_SetAnchor(anchor.transform);

            var barrierHealth = barrier.AddComponent<BarrierHealth>();
            SetPrivateField(barrierHealth, "enemyPushInDistance", 0.5f);

            var origin = new GameObject("Origin");
            origin.transform.SetParent(barrier.transform, false);
            origin.transform.localPosition = Vector3.zero;

            var emitter = AddEmitter(barrier);
            SetPulseTuning(emitter, origin.transform, 1f, 3f, 5f);
            SetPrivateField(emitter, "pulseInsideThreshold", 0.5f);
            Invoke(emitter, "Debug_StartPulse");

            var pastThresholdEnemy = CreateEnemy(new Vector2(0.5f, 0f));
            var outsideEnemy = CreateEnemy(new Vector2(1.8f, 0f));

            Invoke(emitter, "Debug_TickPulse", 1f);

            var pastDisp = pastThresholdEnemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f).magnitude;
            var outsideDisp = outsideEnemy.GetComponent<EnemyKnockbackReceiver>().ConsumeDisplacement(0.1f).magnitude;

            Assert.That(pastDisp, Is.EqualTo(0f).Within(0.001f), "Enemy past barrier threshold should not be pushed.");
            Assert.Greater(outsideDisp, 0.01f, "Enemy not past threshold should be pushed.");

            UnityEngine.Object.DestroyImmediate(anchor);
            UnityEngine.Object.DestroyImmediate(pastThresholdEnemy);
            UnityEngine.Object.DestroyImmediate(outsideEnemy);
            UnityEngine.Object.DestroyImmediate(barrier);
        }

        private static Component AddEmitter(GameObject barrier)
        {
            var type = ResolveEmitterType();
            Assert.NotNull(type, "Expected Castlebound.Gameplay.Castle.BarrierPulseEmitter to exist.");
            return barrier.AddComponent(type);
        }

        private static Type ResolveEmitterType()
        {
            return Type.GetType("Castlebound.Gameplay.Castle.BarrierPulseEmitter, _Project.Gameplay");
        }

        private static void SetPulseTuning(Component emitter, Transform origin, float duration, float radius, float strength, int loops = 1)
        {
            SetPrivateField(emitter, "pulseOrigin", origin);
            SetPrivateField(emitter, "pulseDuration", duration);
            SetPrivateField(emitter, "pulseRadius", radius);
            SetPrivateField(emitter, "pulseStrength", strength);
            SetPrivateField(emitter, "pulseLoopCount", loops);
        }

        private static void BreakBarrier(BarrierHealth barrier)
        {
            barrier.MaxHealth = 1;
            barrier.CurrentHealth = 1;
            barrier.TakeDamage(1);
            barrier.Repair();
        }

        private static GameObject CreateEnemy(Vector2 position)
        {
            var enemy = new GameObject("Enemy");
            enemy.transform.position = position;
            enemy.AddComponent<Rigidbody2D>();
            enemy.AddComponent<BoxCollider2D>();
            enemy.AddComponent<EnemyController2D>();
            enemy.AddComponent<EnemyRegionState>();
            enemy.AddComponent<EnemyKnockbackReceiver>();
            return enemy;
        }

        private static CastleRegionTracker CreateRegionTracker(out GameObject regionGO)
        {
            regionGO = new GameObject("CastleRegion");
            var collider = regionGO.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(20f, 20f);
            var tracker = regionGO.AddComponent<CastleRegionTracker>();
            tracker.Debug_ForceInstanceForTests();
            return tracker;
        }

        private static void SimulateEnter(CastleRegionTracker tracker, Collider2D collider)
        {
            var enter = typeof(CastleRegionTracker).GetMethod(
                "OnTriggerEnter2D",
                BindingFlags.Instance | BindingFlags.NonPublic);
            enter.Invoke(tracker, new object[] { collider });
        }

        private static void Invoke(Component component, string methodName, params object[] args)
        {
            var method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Expected method {methodName} on {component.GetType().Name}.");
            method.Invoke(component, args);
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Expected field {name} on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static bool GetBoolProperty(Component component, string name)
        {
            var prop = component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(prop, $"Expected property {name} on {component.GetType().Name}.");
            return (bool)prop.GetValue(component);
        }
    }
}

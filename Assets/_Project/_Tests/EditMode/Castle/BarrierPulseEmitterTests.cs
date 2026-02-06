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

            var outsideVel = outsideEnemy.GetComponent<Rigidbody2D>().velocity;
            var insideVel = insideEnemy.GetComponent<Rigidbody2D>().velocity;

            Assert.Greater(outsideVel.magnitude, 0.01f, "Outside enemy should receive push.");
            Assert.Less(insideVel.magnitude, 0.001f, "Inside enemy should not be pushed.");

            UnityEngine.Object.DestroyImmediate(regionGO);
            UnityEngine.Object.DestroyImmediate(barrier);
            UnityEngine.Object.DestroyImmediate(outsideEnemy);
            UnityEngine.Object.DestroyImmediate(insideEnemy);
        }

        [Test]
        public void PulseAppliesOnce_WhenWavefrontCrosses()
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

            Invoke(emitter, "Debug_TickPulse", 0.4f);
            var velBefore = enemy.GetComponent<Rigidbody2D>().velocity.magnitude;

            Invoke(emitter, "Debug_TickPulse", 0.2f);
            var velAtCross = enemy.GetComponent<Rigidbody2D>().velocity.magnitude;

            Invoke(emitter, "Debug_TickPulse", 0.4f);
            var velAfter = enemy.GetComponent<Rigidbody2D>().velocity.magnitude;

            Assert.That(velBefore, Is.EqualTo(0f).Within(0.001f), "No push before wavefront reaches enemy.");
            Assert.Greater(velAtCross, 0.01f, "Enemy should be pushed when wavefront crosses.");
            Assert.That(velAfter, Is.EqualTo(velAtCross).Within(0.001f), "Enemy should not be pushed again after crossing.");

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

            var velocity = enemy.GetComponent<Rigidbody2D>().velocity;
            var dir = ((Vector2)enemy.transform.position - (Vector2)origin.transform.position).normalized;
            float dot = Vector2.Dot(velocity.normalized, dir);

            Assert.Greater(dot, 0.5f, "Push should be outward from the origin.");

            UnityEngine.Object.DestroyImmediate(regionGO);
            UnityEngine.Object.DestroyImmediate(barrier);
            UnityEngine.Object.DestroyImmediate(enemy);
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

        private static void SetPulseTuning(Component emitter, Transform origin, float duration, float radius, float strength)
        {
            SetPrivateField(emitter, "pulseOrigin", origin);
            SetPrivateField(emitter, "pulseDuration", duration);
            SetPrivateField(emitter, "pulseRadius", radius);
            SetPrivateField(emitter, "pulseStrength", strength);
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

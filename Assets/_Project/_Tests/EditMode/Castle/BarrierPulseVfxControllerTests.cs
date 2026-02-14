using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class BarrierPulseVfxControllerTests
    {
        [Test]
        public void FollowsEmitterRadius_OnEachTick()
        {
            var barrier = CreateBarrierWithPulse(out var emitter);
            var pulseOrigin = new GameObject("PulseOrigin");
            pulseOrigin.transform.SetParent(barrier.transform, false);
            pulseOrigin.transform.localPosition = new Vector3(1.25f, -0.75f, 0f);
            var ring = barrier.AddComponent<LineRenderer>();
            var vfx = AddVfxController(barrier);

            SetPulseTuning(emitter, duration: 2f, radius: 8f, loops: 1);
            emitter.Debug_StartPulse();
            emitter.Debug_TickPulse(1f);
            Invoke(vfx, "Update");

            float expectedRadius = emitter.CurrentRadius;
            float actualRadius = ReadFirstRingPointRadius(ring, pulseOrigin.transform.position);

            Assert.That(expectedRadius, Is.GreaterThan(0.01f));
            Assert.That(actualRadius, Is.EqualTo(expectedRadius).Within(0.15f), "Ring radius should follow emitter CurrentRadius from PulseOrigin.");

            UnityEngine.Object.DestroyImmediate(barrier);
        }

        [Test]
        public void StartsAndStops_WithEmitterPulseWindow()
        {
            var barrier = CreateBarrierWithPulse(out var emitter);
            var ring = barrier.AddComponent<LineRenderer>();
            var vfx = AddVfxController(barrier);

            SetPulseTuning(emitter, duration: 1f, radius: 6f, loops: 1);
            emitter.Debug_StartPulse();
            emitter.Debug_TickPulse(0.2f);
            Invoke(vfx, "Update");

            Assert.IsTrue(ring.enabled, "Ring should be enabled while pulse is active.");

            emitter.Debug_TickPulse(1f);
            Invoke(vfx, "Update");

            Assert.IsFalse(ring.enabled, "Ring should disable after pulse completes.");

            UnityEngine.Object.DestroyImmediate(barrier);
        }

        [Test]
        public void KeepsRingTextureModeTiled()
        {
            var barrier = CreateBarrierWithPulse(out _);
            var ring = barrier.AddComponent<LineRenderer>();
            var vfx = AddVfxController(barrier);

            Invoke(vfx, "Update");

            Assert.That(ring.textureMode, Is.EqualTo(LineTextureMode.Tile), "Ring texture should tile, not stretch.");

            UnityEngine.Object.DestroyImmediate(barrier);
        }

        private static GameObject CreateBarrierWithPulse(out BarrierPulseEmitter emitter)
        {
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BarrierHealth>();
            barrier.AddComponent<BarrierPressureTracker>();
            emitter = barrier.AddComponent<BarrierPulseEmitter>();
            return barrier;
        }

        private static Component AddVfxController(GameObject barrier)
        {
            var type = ResolveType("Castlebound.Gameplay.Castle.BarrierPulseVfxController");
            Assert.NotNull(type, "Expected Castlebound.Gameplay.Castle.BarrierPulseVfxController to exist.");
            return barrier.AddComponent(type);
        }

        private static Type ResolveType(string fullName)
        {
            return Type.GetType($"{fullName}, _Project.Gameplay");
        }

        private static void SetPulseTuning(BarrierPulseEmitter emitter, float duration, float radius, int loops)
        {
            SetPrivateField(emitter, "pulseDuration", duration);
            SetPrivateField(emitter, "pulseRadius", radius);
            SetPrivateField(emitter, "pulseLoopCount", loops);
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Expected field {name} on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static float ReadFirstRingPointRadius(LineRenderer ring, Vector3 origin)
        {
            Assert.That(ring.positionCount, Is.GreaterThan(0), "Expected ring to have generated points.");
            Vector3 p = ring.GetPosition(0);
            return Vector2.Distance(new Vector2(origin.x, origin.y), new Vector2(p.x, p.y));
        }

        private static void Invoke(Component component, string methodName, params object[] args)
        {
            var method = component.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method, $"Expected method {methodName} on {component.GetType().Name}.");
            method.Invoke(component, args);
        }
    }
}

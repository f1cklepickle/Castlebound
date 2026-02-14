using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class BarrierPulseVfxControllerConfigTests
    {
        [Test]
        public void AppliesRingAlpha()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out var ring);
            var controller = barrier.GetComponent<BarrierPulseVfxController>();

            SetPrivateField(controller, "ringAlpha", 0.75f);

            SetEmitterTuning(emitter, duration: 1f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();
            emitter.Debug_TickPulse(0.2f);
            InvokePrivate(controller, "Update");

            Assert.That(ring.startColor.r, Is.EqualTo(1f).Within(0.001f), "Ring should preserve sprite color (white multiplier).");
            Assert.That(ring.startColor.g, Is.EqualTo(1f).Within(0.001f), "Ring should preserve sprite color (white multiplier).");
            Assert.That(ring.startColor.b, Is.EqualTo(1f).Within(0.001f), "Ring should preserve sprite color (white multiplier).");
            Assert.That(ring.startColor.a, Is.EqualTo(0.75f).Within(0.02f), "Ring alpha should come from ringAlpha control.");
            Assert.That(ring.endColor.a, Is.EqualTo(0.75f).Within(0.02f), "Ring end alpha should match configured ringAlpha.");

            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void AppliesRingSortingOrder_ForVisibility()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out var ring);
            var controller = barrier.GetComponent<BarrierPulseVfxController>();

            SetPrivateField(controller, "ringSortingOrder", 10);

            SetEmitterTuning(emitter, duration: 1f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();
            emitter.Debug_TickPulse(0.2f);
            InvokePrivate(controller, "Update");

            Assert.That(ring.sortingOrder, Is.EqualTo(10), "Ring sorting order should be applied.");

            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void RingOnlyControls_AreExposed()
        {
            var type = typeof(BarrierPulseVfxController);

            Assert.NotNull(type.GetField("ringAlpha", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringAlpha control.");
            Assert.NotNull(type.GetField("ringThicknessWorldUnits", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ring thickness control.");
            Assert.NotNull(type.GetField("ringSortingOrder", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ring sorting control.");
        }

        [Test]
        public void KeepsRingTextureModeTiled_InConfig()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out var ring);
            var controller = barrier.GetComponent<BarrierPulseVfxController>();

            SetEmitterTuning(emitter, duration: 1f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();
            emitter.Debug_TickPulse(0.2f);
            InvokePrivate(controller, "Update");

            Assert.That(ring.textureMode, Is.EqualTo(LineTextureMode.Tile), "Ring should keep tiled texture mode.");

            Object.DestroyImmediate(barrier);
        }

        private static GameObject CreateBarrierWithVfx(out BarrierPulseEmitter emitter, out LineRenderer ring)
        {
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BarrierHealth>();
            barrier.AddComponent<BarrierPressureTracker>();
            emitter = barrier.AddComponent<BarrierPulseEmitter>();
            ring = barrier.AddComponent<LineRenderer>();
            barrier.AddComponent<BarrierPulseVfxController>();
            return barrier;
        }

        private static void SetEmitterTuning(BarrierPulseEmitter emitter, float duration, float radius, int loops)
        {
            var type = typeof(BarrierPulseEmitter);
            type.GetField("pulseDuration", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(emitter, duration);
            type.GetField("pulseRadius", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(emitter, radius);
            type.GetField("pulseLoopCount", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(emitter, loops);
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Expected field {name} on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method, $"Expected method {methodName} on {target.GetType().Name}.");
            method.Invoke(target, null);
        }
    }
}

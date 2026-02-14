using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierPulseEmitterVisualStateTests
    {
        [Test]
        public void ExposesVisualStateProperties_ForVfxFollower()
        {
            var barrier = new GameObject("Barrier");
            var emitter = AddEmitter(barrier);

            Assert.NotNull(
                emitter.GetType().GetProperty("CurrentRadius", BindingFlags.Instance | BindingFlags.Public),
                "Emitter should expose CurrentRadius for visual followers.");
            Assert.NotNull(
                emitter.GetType().GetProperty("PulseProgress01", BindingFlags.Instance | BindingFlags.Public),
                "Emitter should expose PulseProgress01 for visual followers.");

            UnityEngine.Object.DestroyImmediate(barrier);
        }

        [Test]
        public void UpdatesVisualState_DuringPulse()
        {
            var barrier = new GameObject("Barrier");
            var emitter = AddEmitter(barrier);

            SetPrivateField(emitter, "pulseDuration", 2f);
            SetPrivateField(emitter, "pulseRadius", 10f);
            SetPrivateField(emitter, "pulseLoopCount", 1);

            Invoke(emitter, "Debug_StartPulse");
            Invoke(emitter, "Debug_TickPulse", 1f);

            float currentRadius = GetFloatProperty(emitter, "CurrentRadius");
            float progress = GetFloatProperty(emitter, "PulseProgress01");

            Assert.That(currentRadius, Is.EqualTo(5f).Within(0.01f), "CurrentRadius should reflect midpoint expansion.");
            Assert.That(progress, Is.EqualTo(0.5f).Within(0.01f), "PulseProgress01 should reflect normalized duration progress.");

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

        private static float GetFloatProperty(Component component, string name)
        {
            var prop = component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(prop, $"Expected property {name} on {component.GetType().Name}.");
            return (float)prop.GetValue(component);
        }
    }
}

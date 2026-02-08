using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Castle
{
    public class BarrierPressureTrackerTests
    {
        [Test]
        public void TriggersOnThirdBreakInSameWave()
        {
            var waveProvider = CreateWaveProvider(1, out var waveProviderGo);
            var barrierGo = new GameObject("GateA");
            var barrier = barrierGo.AddComponent<BarrierHealth>();
            var tracker = AddTracker(barrierGo, waveProvider);

            int triggerCount = 0;
            Subscribe(tracker, _ => triggerCount++);

            BreakBarrier(barrier);
            BreakBarrier(barrier);
            BreakBarrier(barrier);

            Assert.That(triggerCount, Is.EqualTo(1), "Third break in the same wave should trigger once.");

            BreakBarrier(barrier);
            Assert.That(triggerCount, Is.EqualTo(1), "Further breaks in the same wave should not re-trigger.");

            DestroyAll(barrierGo, waveProviderGo);
        }

        [Test]
        public void ResetsOnWaveChange()
        {
            var waveProvider = CreateWaveProvider(1, out var waveProviderGo);
            var barrierGo = new GameObject("GateA");
            var barrier = barrierGo.AddComponent<BarrierHealth>();
            var tracker = AddTracker(barrierGo, waveProvider);

            int triggerCount = 0;
            Subscribe(tracker, _ => triggerCount++);

            BreakBarrier(barrier);
            BreakBarrier(barrier);
            BreakBarrier(barrier);

            SetWaveIndex(waveProvider, 2);

            BreakBarrier(barrier);
            BreakBarrier(barrier);
            BreakBarrier(barrier);

            Assert.That(triggerCount, Is.EqualTo(2), "Trigger should fire again after wave changes.");

            DestroyAll(barrierGo, waveProviderGo);
        }

        [Test]
        public void TracksBreaksPerBarrier()
        {
            var waveProvider = CreateWaveProvider(1, out var waveProviderGo);

            var barrierAGo = new GameObject("GateA");
            var barrierA = barrierAGo.AddComponent<BarrierHealth>();
            var trackerA = AddTracker(barrierAGo, waveProvider);

            var barrierBGo = new GameObject("GateB");
            var barrierB = barrierBGo.AddComponent<BarrierHealth>();
            var trackerB = AddTracker(barrierBGo, waveProvider);

            int triggerA = 0;
            int triggerB = 0;
            Subscribe(trackerA, _ => triggerA++);
            Subscribe(trackerB, _ => triggerB++);

            BreakBarrier(barrierA);
            BreakBarrier(barrierA);
            BreakBarrier(barrierA);

            Assert.That(triggerA, Is.EqualTo(1), "GateA should trigger on its third break.");
            Assert.That(triggerB, Is.EqualTo(0), "GateB should not trigger from GateA breaks.");

            BreakBarrier(barrierB);
            BreakBarrier(barrierB);
            BreakBarrier(barrierB);

            Assert.That(triggerB, Is.EqualTo(1), "GateB should trigger on its third break.");

            DestroyAll(barrierAGo, barrierBGo, waveProviderGo);
        }

        [Test]
        public void FallsBackToSceneWaveProvider_WhenFieldUnset()
        {
            var waveProvider = CreateWaveProvider(1, out var waveProviderGo);
            var barrierGo = new GameObject("GateA");
            var barrier = barrierGo.AddComponent<BarrierHealth>();
            var tracker = AddTrackerWithoutProvider(barrierGo);

            int triggerCount = 0;
            Subscribe(tracker, _ => triggerCount++);

            BreakBarrier(barrier);
            BreakBarrier(barrier);
            BreakBarrier(barrier);

            Assert.That(triggerCount, Is.EqualTo(1), "Tracker should trigger in wave 1 via scene provider fallback.");

            SetWaveIndex(waveProvider, 2);

            BreakBarrier(barrier);
            BreakBarrier(barrier);
            BreakBarrier(barrier);

            Assert.That(
                triggerCount,
                Is.EqualTo(2),
                "Tracker should reset and trigger again after fallback provider reports a new wave.");

            DestroyAll(barrierGo, waveProviderGo);
        }

        private static object AddTracker(GameObject barrierGo, object waveProvider)
        {
            var trackerType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierPressureTracker");
            Assert.NotNull(trackerType, "Expected Castlebound.Gameplay.Castle.BarrierPressureTracker.");

            var tracker = barrierGo.AddComponent(trackerType);

            var breaksField = trackerType.GetField("breaksPerWave", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(breaksField, "Expected a private 'breaksPerWave' field.");
            breaksField.SetValue(tracker, 3);

            var providerField = trackerType.GetField("waveIndexProvider", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(providerField, "Expected a private 'waveIndexProvider' field.");
            providerField.SetValue(tracker, waveProvider);

            return tracker;
        }

        private static object AddTrackerWithoutProvider(GameObject barrierGo)
        {
            var trackerType = ResolveGameplayType("Castlebound.Gameplay.Castle.BarrierPressureTracker");
            Assert.NotNull(trackerType, "Expected Castlebound.Gameplay.Castle.BarrierPressureTracker.");

            var tracker = barrierGo.AddComponent(trackerType);

            var breaksField = trackerType.GetField("breaksPerWave", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(breaksField, "Expected a private 'breaksPerWave' field.");
            breaksField.SetValue(tracker, 3);

            return tracker;
        }

        private static object CreateWaveProvider(int waveIndex, out GameObject waveProviderGo)
        {
            var providerType = ResolveGameplayType("Castlebound.Gameplay.Spawning.WaveIndexProviderComponent");
            Assert.NotNull(providerType, "Expected Castlebound.Gameplay.Spawning.WaveIndexProviderComponent.");

            waveProviderGo = new GameObject("WaveIndexProvider");
            var provider = waveProviderGo.AddComponent(providerType);
            SetWaveIndex(provider, waveIndex);
            return provider;
        }

        private static void SetWaveIndex(object provider, int waveIndex)
        {
            var providerType = provider.GetType();
            var prop = providerType.GetProperty("CurrentWaveIndex", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(prop, "Expected a public CurrentWaveIndex property.");
            prop.SetValue(provider, waveIndex);
        }

        private static void Subscribe(object tracker, Action<string> handler)
        {
            var trackerType = tracker.GetType();
            var evt = trackerType.GetEvent("OnPressureTriggered", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(evt, "Expected public event OnPressureTriggered.");

            var del = Delegate.CreateDelegate(evt.EventHandlerType, handler.Target, handler.Method);
            evt.AddEventHandler(tracker, del);
        }

        private static void BreakBarrier(BarrierHealth barrier)
        {
            barrier.MaxHealth = 1;
            barrier.CurrentHealth = 1;
            barrier.TakeDamage(1);
            barrier.Repair();
        }

        private static Type ResolveGameplayType(string fullName)
        {
            return Type.GetType($"{fullName}, _Project.Gameplay");
        }

        private static void DestroyAll(params GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
        }
    }
}

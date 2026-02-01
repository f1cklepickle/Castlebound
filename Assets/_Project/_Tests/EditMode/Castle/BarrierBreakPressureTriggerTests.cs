using System;
using System.Reflection;
using NUnit.Framework;

namespace Castlebound.Tests.Castle
{
    public class BarrierBreakPressureTriggerTests
    {
        [Test]
        public void ThirdBreakInSameWave_TriggersOnceForBarrier()
        {
            var trigger = CreateTrigger();

            Assert.IsFalse(RegisterBreak(trigger, "GateA", 1), "First break should not trigger.");
            Assert.IsFalse(RegisterBreak(trigger, "GateA", 1), "Second break should not trigger.");
            Assert.IsTrue(RegisterBreak(trigger, "GateA", 1), "Third break should trigger.");
            Assert.IsFalse(RegisterBreak(trigger, "GateA", 1), "Further breaks in same wave should not re-trigger.");
        }

        [Test]
        public void BreaksAreTrackedPerBarrier()
        {
            var trigger = CreateTrigger();

            RegisterBreak(trigger, "GateA", 1);
            RegisterBreak(trigger, "GateB", 1);

            Assert.IsFalse(RegisterBreak(trigger, "GateA", 1), "Second break for GateA should not trigger.");
            Assert.IsTrue(RegisterBreak(trigger, "GateA", 1), "Third break for GateA should trigger regardless of GateB breaks.");
        }

        [Test]
        public void NewWaveResetsPerBarrierCounts()
        {
            var trigger = CreateTrigger();

            RegisterBreak(trigger, "GateA", 1);
            RegisterBreak(trigger, "GateA", 1);

            Assert.IsFalse(RegisterBreak(trigger, "GateA", 2), "First break of new wave should not trigger.");
            RegisterBreak(trigger, "GateA", 2);
            Assert.IsTrue(RegisterBreak(trigger, "GateA", 2), "Third break in new wave should trigger.");
        }

        private static object CreateTrigger()
        {
            var triggerType = Type.GetType("Castlebound.Gameplay.Castle.BarrierBreakPressureTrigger, _Project.Gameplay");
            Assert.NotNull(triggerType, "Expected Castlebound.Gameplay.Castle.BarrierBreakPressureTrigger.");

            var ctor = triggerType.GetConstructor(new[] { typeof(int) });
            Assert.NotNull(ctor, "Expected a constructor BarrierBreakPressureTrigger(int breaksPerWave).");

            return ctor.Invoke(new object[] { 3 });
        }

        private static bool RegisterBreak(object trigger, string barrierId, int waveIndex)
        {
            var method = trigger.GetType().GetMethod("RegisterBreak", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method, "Expected RegisterBreak(string barrierId, int waveIndex) method.");

            return (bool)method.Invoke(trigger, new object[] { barrierId, waveIndex });
        }
    }
}

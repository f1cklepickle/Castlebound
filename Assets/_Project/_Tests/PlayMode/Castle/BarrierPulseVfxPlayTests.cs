using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.PlayMode.Castle
{
    public class BarrierPulseVfxPlayTests
    {
        [UnityTest]
        public IEnumerator CueActivates_WhenPulseStarts()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out _, out var ring);
            SetEmitterTuning(emitter, duration: 0.5f, radius: 3f, loops: 1);

            emitter.Debug_StartPulse();
            yield return null;

            Assert.IsTrue(ring.enabled, "Ring should enable when pulse starts.");

            Object.Destroy(barrier);
        }

        [UnityTest]
        public IEnumerator CueFadesAfterPulseCompletes()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out _, out var ring);
            SetEmitterTuning(emitter, duration: 0.1f, radius: 2f, loops: 1);

            emitter.Debug_StartPulse();
            yield return new WaitForSeconds(0.25f);

            Assert.IsFalse(emitter.IsPulseActive, "Emitter should complete pulse.");
            Assert.IsFalse(ring.enabled, "Ring should disable after pulse completes.");

            Object.Destroy(barrier);
        }

        [UnityTest]
        public IEnumerator CueTracksEmitterRadius_AtRuntime()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out _, out var ring);
            SetEmitterTuning(emitter, duration: 0.5f, radius: 4f, loops: 1);

            emitter.Debug_StartPulse();
            yield return new WaitForSeconds(0.2f);

            float expected = emitter.CurrentRadius;
            float ringRadius = ReadFirstRingPointRadius(ring, barrier.transform.position);

            Assert.That(ringRadius, Is.EqualTo(expected).Within(0.2f), "Ring should track emitter radius.");

            Object.Destroy(barrier);
        }

        [UnityTest]
        public IEnumerator RingFlipbookAdvances_AtRuntime()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out _, out var ring);
            SetEmitterTuning(emitter, duration: 0.6f, radius: 3f, loops: 1);

            emitter.Debug_StartPulse();
            yield return null;

            var material = ring.sharedMaterial;
            Assert.NotNull(material, "Ring material should exist while pulse is active.");
            float startOffset = material.mainTextureOffset.x;

            yield return new WaitForSeconds(0.15f);

            float laterOffset = material.mainTextureOffset.x;
            Assert.That(laterOffset, Is.Not.EqualTo(startOffset), "Ring flipbook frame offset should advance over time.");

            Object.Destroy(barrier);
        }

        [UnityTest]
        public IEnumerator MissingVisualComponents_NoRuntimeErrors()
        {
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BarrierHealth>();
            barrier.AddComponent<BarrierPressureTracker>();
            var emitter = barrier.AddComponent<BarrierPulseEmitter>();
            barrier.AddComponent<BarrierPulseVfxController>();

            SetEmitterTuning(emitter, duration: 0.2f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();
            yield return new WaitForSeconds(0.1f);

            Assert.Pass("No exceptions should be thrown with missing visual components.");
        }

        [UnityTest]
        public IEnumerator ThirdBreakFlow_StillWorksWithVfxAttached()
        {
            var barrier = CreateBarrierWithVfx(out var emitter, out var health, out var ring);
            SetEmitterTuning(emitter, duration: 0.3f, radius: 3f, loops: 1);

            BreakBarrier(health);
            BreakBarrier(health);
            BreakBarrier(health);
            yield return null;

            Assert.IsTrue(emitter.IsPulseActive, "Pulse should activate on third break.");
            Assert.IsTrue(ring.enabled, "Ring should be active after third-break trigger.");

            Object.Destroy(barrier);
        }

        private static GameObject CreateBarrierWithVfx(
            out BarrierPulseEmitter emitter,
            out BarrierHealth health,
            out LineRenderer ring)
        {
            var barrier = new GameObject("Barrier");
            health = barrier.AddComponent<BarrierHealth>();
            barrier.AddComponent<BarrierPressureTracker>();
            emitter = barrier.AddComponent<BarrierPulseEmitter>();
            ring = barrier.AddComponent<LineRenderer>();
            var vfx = barrier.AddComponent<BarrierPulseVfxController>();
            SetPrivateField(vfx, "ringStripSprite", MakeTestSprite());
            return barrier;
        }

        private static void BreakBarrier(BarrierHealth health)
        {
            health.MaxHealth = 1;
            health.CurrentHealth = 1;
            health.TakeDamage(1);
            health.Repair();
        }

        private static void SetEmitterTuning(BarrierPulseEmitter emitter, float duration, float radius, int loops)
        {
            var type = typeof(BarrierPulseEmitter);
            type.GetField("pulseDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(emitter, duration);
            type.GetField("pulseRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(emitter, radius);
            type.GetField("pulseLoopCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(emitter, loops);
        }

        private static float ReadFirstRingPointRadius(LineRenderer ring, Vector3 origin)
        {
            Assert.That(ring.positionCount, Is.GreaterThan(0), "Expected ring points while active.");
            var point = ring.GetPosition(0);
            return Vector2.Distance(new Vector2(point.x, point.y), new Vector2(origin.x, origin.y));
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static Sprite MakeTestSprite()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
        }
    }
}

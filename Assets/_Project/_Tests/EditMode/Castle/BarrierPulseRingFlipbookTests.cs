using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class BarrierPulseRingFlipbookTests
    {
        [Test]
        public void ExposesRingFlipbookControls()
        {
            var type = typeof(BarrierPulseVfxController);

            Assert.NotNull(type.GetField("ringFlipbookEnabled", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringFlipbookEnabled control.");
            Assert.NotNull(type.GetField("ringFlipbookTilesX", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringFlipbookTilesX control.");
            Assert.NotNull(type.GetField("ringFlipbookTilesY", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringFlipbookTilesY control.");
            Assert.NotNull(type.GetField("ringFlipbookFps", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringFlipbookFps control.");
        }

        [Test]
        public void ExposesRingSeamDriftControls()
        {
            var type = typeof(BarrierPulseVfxController);

            Assert.NotNull(type.GetField("ringSeamDriftEnabled", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringSeamDriftEnabled control.");
            Assert.NotNull(type.GetField("ringSeamDriftSpeed", BindingFlags.NonPublic | BindingFlags.Instance), "Expected ringSeamDriftSpeed control.");
        }

        [Test]
        public void AdvancesRingFlipbookFrame_OverTime()
        {
            var barrier = CreateBarrier(out var emitter, out var ring, out var vfx);
            SetPrivateField(vfx, "ringFlipbookEnabled", true);
            SetPrivateField(vfx, "ringFlipbookTilesX", 9);
            SetPrivateField(vfx, "ringFlipbookTilesY", 1);
            SetPrivateField(vfx, "ringFlipbookFps", 12f);
            SetPrivateField(vfx, "ringStripSprite", MakeTestSprite());

            SetEmitterTuning(emitter, duration: 1f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();

            InvokePrivate(vfx, "Update");
            float startOffset = ring.sharedMaterial != null ? ring.sharedMaterial.mainTextureOffset.x : 0f;

            emitter.Debug_TickPulse(0.2f);
            InvokePrivate(vfx, "Update");
            float nextOffset = ring.sharedMaterial != null ? ring.sharedMaterial.mainTextureOffset.x : 0f;

            Assert.That(nextOffset, Is.Not.EqualTo(startOffset), "Ring flipbook frame should advance over time.");

            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void LoopsRingFlipbookFrameIndex()
        {
            var barrier = CreateBarrier(out var emitter, out var ring, out var vfx);
            SetPrivateField(vfx, "ringFlipbookEnabled", true);
            SetPrivateField(vfx, "ringFlipbookTilesX", 4);
            SetPrivateField(vfx, "ringFlipbookTilesY", 1);
            SetPrivateField(vfx, "ringFlipbookFps", 8f);
            SetPrivateField(vfx, "ringStripSprite", MakeTestSprite());

            SetEmitterTuning(emitter, duration: 2f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();

            emitter.Debug_TickPulse(0.6f);
            InvokePrivate(vfx, "Update");
            float offsetA = ring.sharedMaterial != null ? ring.sharedMaterial.mainTextureOffset.x : 0f;

            emitter.Debug_TickPulse(0.6f);
            InvokePrivate(vfx, "Update");
            float offsetB = ring.sharedMaterial != null ? ring.sharedMaterial.mainTextureOffset.x : 0f;

            Assert.That(offsetA, Is.GreaterThanOrEqualTo(0f));
            Assert.That(offsetB, Is.GreaterThanOrEqualTo(0f));
            Assert.That(offsetA, Is.LessThan(1f));
            Assert.That(offsetB, Is.LessThan(1f));

            Object.DestroyImmediate(barrier);
        }

        [Test]
        public void DriftsRingSeam_WhilePulseActive()
        {
            var barrier = CreateBarrier(out var emitter, out var ring, out var vfx);
            SetPrivateField(vfx, "ringFlipbookEnabled", true);
            SetPrivateField(vfx, "ringFlipbookTilesX", 8);
            SetPrivateField(vfx, "ringFlipbookTilesY", 1);
            SetPrivateField(vfx, "ringFlipbookFps", 0f);
            SetPrivateField(vfx, "ringSeamDriftEnabled", true);
            SetPrivateField(vfx, "ringSeamDriftSpeed", 0.75f);
            SetPrivateField(vfx, "ringStripSprite", MakeTestSprite());

            SetEmitterTuning(emitter, duration: 1.5f, radius: 2f, loops: 1);
            emitter.Debug_StartPulse();

            emitter.Debug_TickPulse(0.2f);
            InvokePrivate(vfx, "Update");
            float offsetA = ring.sharedMaterial != null ? ring.sharedMaterial.mainTextureOffset.x : 0f;

            emitter.Debug_TickPulse(0.2f);
            InvokePrivate(vfx, "Update");
            float offsetB = ring.sharedMaterial != null ? ring.sharedMaterial.mainTextureOffset.x : 0f;

            Assert.That(offsetB, Is.Not.EqualTo(offsetA), "Seam drift should move UV offset while pulse is active.");

            Object.DestroyImmediate(barrier);
        }

        private static GameObject CreateBarrier(out BarrierPulseEmitter emitter, out LineRenderer ring, out BarrierPulseVfxController vfx)
        {
            var barrier = new GameObject("Barrier");
            barrier.AddComponent<BarrierHealth>();
            barrier.AddComponent<BarrierPressureTracker>();
            emitter = barrier.AddComponent<BarrierPulseEmitter>();
            ring = barrier.AddComponent<LineRenderer>();
            vfx = barrier.AddComponent<BarrierPulseVfxController>();
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

        private static Sprite MakeTestSprite()
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            return sprite;
        }
    }
}

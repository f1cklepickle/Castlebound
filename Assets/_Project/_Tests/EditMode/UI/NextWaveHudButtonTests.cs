using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.UI
{
    public class NextWaveHudButtonTests
    {
        private GameObject root;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("HudRoot", typeof(Canvas), typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void RuntimeButton_AnchorsTopCenter()
        {
            var phase = new WavePhaseTracker();
            var hud = root.AddComponent<NextWaveHudButton>();
            hud.SetPhaseTracker(phase);
            hud.Initialize();

            var rect = hud.Button.GetComponent<RectTransform>();

            Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0.5f, 1f)));
            Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0.5f, 1f)));
            Assert.That(rect.pivot, Is.EqualTo(new Vector2(0.5f, 1f)));
            Assert.That(rect.anchoredPosition.y, Is.LessThan(0f));
        }

        [Test]
        public void StartNextWave_OnlyWorksDuringPreWave()
        {
            var phase = new WavePhaseTracker();
            var hud = root.AddComponent<NextWaveHudButton>();
            hud.SetPhaseTracker(phase);

            phase.SetPhase(WavePhase.InWave);
            hud.StartNextWave();
            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.InWave));

            phase.SetPhase(WavePhase.PreWave);
            hud.StartNextWave();
            Assert.That(phase.CurrentPhase, Is.EqualTo(WavePhase.InWave));
        }

        [Test]
        public void ButtonVisibility_FollowsWavePhase()
        {
            var phase = new WavePhaseTracker();
            var hud = root.AddComponent<NextWaveHudButton>();
            hud.SetPhaseTracker(phase);
            hud.Initialize();

            Assert.IsTrue(hud.Button.gameObject.activeSelf);

            phase.SetPhase(WavePhase.InWave);
            Assert.IsFalse(hud.Button.gameObject.activeSelf);

            phase.SetPhase(WavePhase.PreWave);
            Assert.IsTrue(hud.Button.gameObject.activeSelf);
        }
    }
}

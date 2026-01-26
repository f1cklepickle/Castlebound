using Castlebound.Gameplay.Barrier;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Tests.Barrier
{
    public class BarrierTierVisualsBinderTests
    {
        [Test]
        public void UpdatesVisuals_WhenTierOrHealthChanges()
        {
            var root = new GameObject("Root");
            var binder = root.AddComponent<BarrierTierVisualsBinder>();

            var visualsGo = new GameObject("Visuals");
            var visuals = visualsGo.AddComponent<BarrierTierVisuals>();

            var border = new GameObject("Border").AddComponent<Image>();
            var fill = new GameObject("Fill").AddComponent<Image>();
            var star = new GameObject("Star").AddComponent<Image>();
            visuals.SetImages(border, fill, star);
            visuals.SetCycleConfig(new[] { Color.red, Color.green }, 2);

            var health = root.AddComponent<BarrierHealth>();
            health.MaxHealth = 100;
            health.CurrentHealth = 100;

            var controller = root.AddComponent<BarrierUpgradeController>();

            binder.SetReferences(controller, health, visuals);
            binder.Tick();

            float startFill = fill.fillAmount;

            health.CurrentHealth = 50;
            controller.SharedState = null;
            controller.State.IncrementTier();
            binder.Tick();

            Assert.That(fill.fillAmount, Is.Not.EqualTo(startFill));

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(visualsGo);
        }
    }
}

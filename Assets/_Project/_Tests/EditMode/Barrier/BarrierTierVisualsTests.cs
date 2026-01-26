using System.Collections.Generic;
using Castlebound.Gameplay.Barrier;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Tests.Barrier
{
    public class BarrierTierVisualsTests
    {
        [Test]
        public void Star_IsHiddenUntilFirstBorderCycleCompletes()
        {
            var root = new GameObject("Root");
            var visuals = root.AddComponent<BarrierTierVisuals>();

            var border = CreateImage("Border", root.transform);
            var fill = CreateImage("Fill", root.transform);
            var star = CreateImage("Star", root.transform);
            star.enabled = true;

            visuals.SetImages(border, fill, star);
            visuals.SetCycleConfig(CreateBaseColors(3), 2);

            visuals.UpdateVisuals(5, 50, 100);
            Assert.IsFalse(star.enabled, "Star stays hidden before the first border cycle completes.");

            visuals.UpdateVisuals(6, 50, 100);
            Assert.IsTrue(star.enabled, "Star becomes visible after the first border cycle completes.");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void HealthFill_ReflectsCurrentHealthRatio()
        {
            var root = new GameObject("Root");
            var visuals = root.AddComponent<BarrierTierVisuals>();

            var border = CreateImage("Border", root.transform);
            var fill = CreateImage("Fill", root.transform);
            var star = CreateImage("Star", root.transform);

            visuals.SetImages(border, fill, star);
            visuals.SetCycleConfig(CreateBaseColors(3), 2);

            visuals.UpdateVisuals(0, 25, 100);
            Assert.That(fill.fillAmount, Is.EqualTo(0.25f).Within(0.001f));

            Object.DestroyImmediate(root);
        }

        private static Image CreateImage(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            return go.AddComponent<Image>();
        }

        private static List<Color> CreateBaseColors(int count)
        {
            var colors = new List<Color>(count);
            for (int i = 0; i < count; i++)
            {
                colors.Add(Color.HSVToRGB(i / (float)count, 0.8f, 0.9f));
            }

            return colors;
        }
    }
}

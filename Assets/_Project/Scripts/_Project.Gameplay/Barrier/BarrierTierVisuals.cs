using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.Barrier
{
    public class BarrierTierVisuals : MonoBehaviour
    {
        [SerializeField] private Image borderImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image starImage;
        [SerializeField] private List<Color> baseColors = new List<Color>();
        [SerializeField] private int shadesPerColor = 10;

        private BarrierTierColorCycle cycle;

        public void SetImages(Image border, Image fill, Image star)
        {
            borderImage = border;
            fillImage = fill;
            starImage = star;
        }

        public void SetCycleConfig(IReadOnlyList<Color> colors, int shades)
        {
            baseColors = colors != null ? new List<Color>(colors) : new List<Color>();
            shadesPerColor = Mathf.Max(1, shades);
            cycle = new BarrierTierColorCycle(baseColors, shadesPerColor);
        }

        public void UpdateVisuals(int tier, int currentHealth, int maxHealth)
        {
            if (cycle == null)
            {
                cycle = new BarrierTierColorCycle(baseColors, shadesPerColor);
            }

            var result = cycle.Evaluate(tier);

            if (borderImage != null)
            {
                borderImage.color = ResolveColor(result.BorderBaseIndex, result.BorderShadeIndex);
            }

            if (starImage != null)
            {
                int borderCycleLength = baseColors.Count * shadesPerColor;
                bool showStar = tier >= borderCycleLength && !result.IsCapped;
                starImage.enabled = showStar;
                if (showStar)
                {
                    int displayStarBase = Mathf.Max(0, result.StarBaseIndex - 1);
                    starImage.color = ResolveColor(displayStarBase, 0);
                }
                else if (result.IsCapped)
                {
                    starImage.enabled = true;
                    starImage.color = ResolveColor(result.StarBaseIndex, result.BorderShadeIndex);
                }
            }

            if (fillImage != null)
            {
                float ratio = maxHealth > 0 ? Mathf.Clamp01(currentHealth / (float)maxHealth) : 0f;
                fillImage.fillAmount = ratio;
            }
        }

        private Color ResolveColor(int baseIndex, int shadeIndex)
        {
            if (baseColors == null || baseColors.Count == 0)
            {
                return Color.white;
            }

            int clampedBase = Mathf.Clamp(baseIndex, 0, baseColors.Count - 1);
            int clampedShade = Mathf.Clamp(shadeIndex, 0, Mathf.Max(1, shadesPerColor) - 1);

            var baseColor = baseColors[clampedBase];
            if (shadesPerColor <= 1)
            {
                return baseColor;
            }

            float half = (shadesPerColor - 1) * 0.5f;
            float offset = half - clampedShade;
            float step = 0.04f;
            float lightnessDelta = offset * step;

            bool isWhiteBase = clampedBase == baseColors.Count - 1;
            if (isWhiteBase)
            {
                lightnessDelta = -lightnessDelta;
            }

            float h, s, l;
            RgbToHsl(baseColor, out h, out s, out l);
            l = Mathf.Clamp01(l + lightnessDelta);
            return HslToRgb(h, s, l);
        }

        private static void RgbToHsl(Color color, out float h, out float s, out float l)
        {
            float r = color.r;
            float g = color.g;
            float b = color.b;
            float max = Mathf.Max(r, Mathf.Max(g, b));
            float min = Mathf.Min(r, Mathf.Min(g, b));
            l = (max + min) * 0.5f;

            if (Mathf.Approximately(max, min))
            {
                h = 0f;
                s = 0f;
                return;
            }

            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            if (Mathf.Approximately(max, r))
            {
                h = (g - b) / d + (g < b ? 6f : 0f);
            }
            else if (Mathf.Approximately(max, g))
            {
                h = (b - r) / d + 2f;
            }
            else
            {
                h = (r - g) / d + 4f;
            }

            h /= 6f;
        }

        private static Color HslToRgb(float h, float s, float l)
        {
            if (Mathf.Approximately(s, 0f))
            {
                return new Color(l, l, l, 1f);
            }

            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;
            float r = HueToRgb(p, q, h + 1f / 3f);
            float g = HueToRgb(p, q, h);
            float b = HueToRgb(p, q, h - 1f / 3f);
            return new Color(r, g, b, 1f);
        }

        private static float HueToRgb(float p, float q, float t)
        {
            if (t < 0f)
            {
                t += 1f;
            }

            if (t > 1f)
            {
                t -= 1f;
            }

            if (t < 1f / 6f)
            {
                return p + (q - p) * 6f * t;
            }

            if (t < 0.5f)
            {
                return q;
            }

            if (t < 2f / 3f)
            {
                return p + (q - p) * (2f / 3f - t) * 6f;
            }

            return p;
        }
    }
}

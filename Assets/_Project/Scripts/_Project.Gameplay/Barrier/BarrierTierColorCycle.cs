using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    public readonly struct TierColorCycleResult
    {
        public readonly int BorderBaseIndex;
        public readonly int BorderShadeIndex;
        public readonly int StarBaseIndex;
        public readonly bool IsCapped;

        public TierColorCycleResult(int borderBaseIndex, int borderShadeIndex, int starBaseIndex, bool isCapped)
        {
            BorderBaseIndex = borderBaseIndex;
            BorderShadeIndex = borderShadeIndex;
            StarBaseIndex = starBaseIndex;
            IsCapped = isCapped;
        }
    }

    public sealed class BarrierTierColorCycle
    {
        private readonly IReadOnlyList<Color> baseColors;
        private readonly int shadesPerColor;

        public BarrierTierColorCycle(IReadOnlyList<Color> baseColors, int shadesPerColor)
        {
            this.baseColors = baseColors;
            this.shadesPerColor = Mathf.Max(1, shadesPerColor);
        }

        public TierColorCycleResult Evaluate(int tier)
        {
            if (baseColors == null || baseColors.Count == 0)
            {
                return new TierColorCycleResult(0, 0, 0, true);
            }

            int safeTier = Mathf.Max(0, tier);
            int baseCount = baseColors.Count;
            int borderCycleLength = baseCount * shadesPerColor;
            int fullStarCycleLength = borderCycleLength * baseCount;

            if (safeTier >= fullStarCycleLength)
            {
                int capIndex = baseCount - 1;
                int capShade = shadesPerColor - 1;
                return new TierColorCycleResult(capIndex, capShade, capIndex, true);
            }

            int borderBaseIndex = (safeTier / shadesPerColor) % baseCount;
            int borderShadeIndex = safeTier % shadesPerColor;
            int starBaseIndex = safeTier / borderCycleLength;

            return new TierColorCycleResult(borderBaseIndex, borderShadeIndex, starBaseIndex, false);
        }
    }
}

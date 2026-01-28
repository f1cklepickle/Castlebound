using System.Collections.Generic;
using Castlebound.Gameplay.Barrier;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Barrier
{
    public class BarrierTierColorCycleTests
    {
        [Test]
        public void BorderCycle_AdvancesShadeThenBase_ByListOrder()
        {
            int baseCount = 4;
            int shadesPerColor = 3;
            var cycle = new BarrierTierColorCycle(CreateBaseColors(baseCount), shadesPerColor);

            var tier0 = cycle.Evaluate(0);
            Assert.That(tier0.BorderBaseIndex, Is.EqualTo(0));
            Assert.That(tier0.BorderShadeIndex, Is.EqualTo(0));
            Assert.That(tier0.StarBaseIndex, Is.EqualTo(0));
            Assert.IsFalse(tier0.IsCapped);

            var tier1 = cycle.Evaluate(1);
            Assert.That(tier1.BorderBaseIndex, Is.EqualTo(0));
            Assert.That(tier1.BorderShadeIndex, Is.EqualTo(1));

            var tier2 = cycle.Evaluate(2);
            Assert.That(tier2.BorderBaseIndex, Is.EqualTo(0));
            Assert.That(tier2.BorderShadeIndex, Is.EqualTo(2));

            var tier3 = cycle.Evaluate(3);
            Assert.That(tier3.BorderBaseIndex, Is.EqualTo(1));
            Assert.That(tier3.BorderShadeIndex, Is.EqualTo(0));
        }

        [Test]
        public void StarCycle_AdvancesAfterFullBorderCycle()
        {
            int baseCount = 4;
            int shadesPerColor = 3;
            int borderCycleLength = baseCount * shadesPerColor;
            var cycle = new BarrierTierColorCycle(CreateBaseColors(baseCount), shadesPerColor);

            var before = cycle.Evaluate(borderCycleLength - 1);
            Assert.That(before.BorderBaseIndex, Is.EqualTo(baseCount - 1));
            Assert.That(before.BorderShadeIndex, Is.EqualTo(shadesPerColor - 1));
            Assert.That(before.StarBaseIndex, Is.EqualTo(0));
            Assert.IsFalse(before.IsCapped);

            var after = cycle.Evaluate(borderCycleLength);
            Assert.That(after.BorderBaseIndex, Is.EqualTo(0));
            Assert.That(after.BorderShadeIndex, Is.EqualTo(0));
            Assert.That(after.StarBaseIndex, Is.EqualTo(1));
            Assert.IsFalse(after.IsCapped);
        }

        [Test]
        public void CapState_PinsIndicesAfterFullStarCycle()
        {
            int baseCount = 4;
            int shadesPerColor = 3;
            int fullStarCycleLength = baseCount * shadesPerColor * baseCount;
            var cycle = new BarrierTierColorCycle(CreateBaseColors(baseCount), shadesPerColor);

            var capped = cycle.Evaluate(fullStarCycleLength);
            Assert.IsTrue(capped.IsCapped);
            Assert.That(capped.BorderBaseIndex, Is.EqualTo(baseCount - 1));
            Assert.That(capped.BorderShadeIndex, Is.EqualTo(shadesPerColor - 1));
            Assert.That(capped.StarBaseIndex, Is.EqualTo(baseCount - 1));
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

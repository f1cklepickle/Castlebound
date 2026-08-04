using Castlebound.Gameplay.Combat;
using NUnit.Framework;

namespace Castlebound.Tests.Combat
{
    public class AttackRatePolicyTests
    {
        [TestCase(float.NaN, AttackRatePolicy.MinimumAttackRate)]
        [TestCase(float.NegativeInfinity, AttackRatePolicy.MinimumAttackRate)]
        [TestCase(-1f, AttackRatePolicy.MinimumAttackRate)]
        [TestCase(0f, AttackRatePolicy.MinimumAttackRate)]
        [TestCase(float.PositiveInfinity, AttackRatePolicy.MaximumAttackRate)]
        [TestCase(1000f, AttackRatePolicy.MaximumAttackRate)]
        public void Normalize_ClampsInvalidAndExtremeRates(float rate, float expected)
        {
            Assert.That(AttackRatePolicy.Normalize(rate), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void Normalize_PreservesSupportedRate()
        {
            Assert.That(AttackRatePolicy.Normalize(1.75f), Is.EqualTo(1.75f).Within(0.0001f));
        }
    }
}

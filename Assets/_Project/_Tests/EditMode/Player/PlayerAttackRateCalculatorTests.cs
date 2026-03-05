using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackRateCalculatorTests
    {
        [Test]
        public void ComputeEffectiveRate_UsesBaseTimesWeaponSpeed()
        {
            var rate = PlayerAttackRateCalculator.ComputeEffectiveRate(1.5f, 2f);
            Assert.AreEqual(3f, rate, 0.0001f);
        }

        [Test]
        public void ComputeEffectiveRate_WeaponSpeedOne_EqualsBaseRate()
        {
            var rate = PlayerAttackRateCalculator.ComputeEffectiveRate(1.5f, 1f);
            Assert.AreEqual(1.5f, rate, 0.0001f);
        }

        [Test]
        public void ComputeEffectiveRate_InvalidWeaponSpeed_DefaultsToOne()
        {
            var rate = PlayerAttackRateCalculator.ComputeEffectiveRate(1.5f, 0f);
            Assert.AreEqual(1.5f, rate, 0.0001f);
        }

        [Test]
        public void EffectiveRate_TracksWeaponSpeedChanges()
        {
            const float baseRate = 1.5f;

            var atOneX = PlayerAttackRateCalculator.ComputeEffectiveRate(baseRate, 1f);
            var atTwoX = PlayerAttackRateCalculator.ComputeEffectiveRate(baseRate, 2f);
            var atHalfX = PlayerAttackRateCalculator.ComputeEffectiveRate(baseRate, 0.5f);

            Assert.AreEqual(1.5f, atOneX, 0.0001f);
            Assert.AreEqual(3f, atTwoX, 0.0001f);
            Assert.AreEqual(0.75f, atHalfX, 0.0001f);
        }
    }
}

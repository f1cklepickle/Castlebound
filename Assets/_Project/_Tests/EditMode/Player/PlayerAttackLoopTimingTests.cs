using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackLoopTimingTests
    {
        private GameObject root;
        private PlayerAttackLoop loop;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PlayerAttackLoop");
            loop = root.AddComponent<PlayerAttackLoop>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void HeldFire_ChainsBackToBackWithoutIdleGap()
        {
            loop.Tick(0f, 2f, true);
            loop.Tick(1.1f, 2f, true);

            Assert.GreaterOrEqual(loop.CompletedSwingCount, 2,
                "Held fire should chain swings without idle gaps.");
            Assert.IsTrue(loop.IsSwingActive,
                "Loop should still be in an active swing while held.");
        }

        [Test]
        public void HigherEffectiveRate_ShortensTotalSwingDuration()
        {
            loop.Tick(0f, 1f, true);
            var rateOneDuration = loop.CurrentSwingDuration;

            loop.ResetLoopState();
            loop.Tick(0f, 2f, true);
            var rateTwoDuration = loop.CurrentSwingDuration;

            Assert.Less(rateTwoDuration, rateOneDuration,
                "Higher effective rate should shorten total swing duration.");
        }

        [Test]
        public void MinSwingDuration_ClampsExtremeRates()
        {
            loop.Tick(0f, 100f, true);

            Assert.That(loop.CurrentSwingDuration, Is.EqualTo(loop.MinSwingDuration).Within(0.001f),
                "Extreme rates should clamp to minimum swing duration.");
        }

        [Test]
        public void ReleaseStopsChaining_AfterCurrentSwing()
        {
            loop.Tick(0f, 2f, true);
            var firstSwingDuration = loop.CurrentSwingDuration;

            loop.Tick(firstSwingDuration + 0.05f, 2f, false);

            Assert.AreEqual(1, loop.CompletedSwingCount,
                "Release should allow current swing to finish, then stop chaining.");
            Assert.IsFalse(loop.IsSwingActive,
                "Loop should return to idle after finishing current swing on release.");
        }

        [Test]
        public void HeldIntent_RemainsActive_AcrossChainBoundary()
        {
            loop.Tick(0f, 5f, true);
            var firstSwingDuration = loop.CurrentSwingDuration;

            loop.Tick(firstSwingDuration + 0.001f, 5f, true);

            Assert.IsTrue(loop.IsSwingActive,
                "Loop should remain active while held across chained swing boundaries.");
            Assert.GreaterOrEqual(loop.CompletedSwingCount, 1,
                "Held cadence should complete at least one swing before continuing into the next.");
        }

        [Test]
        public void HeldIntent_ContinuesWithoutIdlePulse_OverMultipleTicks()
        {
            loop.Tick(0f, 5f, true);
            for (var i = 0; i < 8; i++)
                loop.Tick(0.05f, 5f, true);

            Assert.IsTrue(loop.IsSwingActive,
                "Loop should continue active across multiple ticks while held.");
        }

    }
}

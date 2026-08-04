using Castlebound.Gameplay.Combat;
using NUnit.Framework;

namespace Castlebound.Tests.Combat
{
    public class AttackClockTests
    {
        private static readonly AttackPhaseProfile PhaseProfile =
            new AttackPhaseProfile(0.25f, 0.30f, 0.45f);

        [Test]
        public void Start_CapturesNormalizedRateAndImmutablePhaseDurations()
        {
            var clock = new AttackClock();

            clock.Start(2f, PhaseProfile);
            AttackSwingTiming swing = clock.CurrentSwing;
            clock.Advance(0.1f);

            Assert.That(swing.AttackRate, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(swing.Duration, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(swing.WindupDuration, Is.EqualTo(0.125f).Within(0.0001f));
            Assert.That(clock.CurrentSwing.Duration, Is.EqualTo(swing.Duration).Within(0.0001f));
        }

        [Test]
        public void Advance_CarriesOvershootAcrossEveryPhaseAndReturnsUnusedTime()
        {
            var clock = new AttackClock();
            clock.Start(2f, PhaseProfile);

            AttackClockStep step = clock.Advance(0.8f);

            Assert.IsTrue(step.ImpactOccurred);
            Assert.IsTrue(step.ActiveWindowOccurred);
            Assert.IsTrue(step.SwingCompleted);
            Assert.That(step.UnusedDeltaTime, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(clock.NormalizedProgress, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ImpactBoundary_IsReportedExactlyOnce()
        {
            var clock = new AttackClock();
            clock.Start(1f, PhaseProfile);

            Assert.IsFalse(clock.Advance(0.24f).ImpactOccurred);
            Assert.IsTrue(clock.Advance(0.02f).ImpactOccurred);
            Assert.IsFalse(clock.Advance(0.2f).ImpactOccurred);
            Assert.IsFalse(clock.Advance(1f).ImpactOccurred);
        }

        [Test]
        public void Cancel_PreventsLateImpactAndCompletion()
        {
            var clock = new AttackClock();
            clock.Start(1f, PhaseProfile);
            clock.Advance(0.1f);

            clock.Cancel();
            AttackClockStep step = clock.Advance(1f);

            Assert.IsFalse(step.ImpactOccurred);
            Assert.IsFalse(step.SwingCompleted);
            Assert.IsFalse(clock.IsRunning);
        }

        [Test]
        public void RateChangeAfterStart_DoesNotRetimeActiveSwing()
        {
            var clock = new AttackClock();
            clock.Start(1f, PhaseProfile);
            float capturedDuration = clock.CurrentSwing.Duration;

            float nextRate = AttackRatePolicy.Normalize(4f);
            clock.Advance(0.5f);

            Assert.That(nextRate, Is.EqualTo(4f));
            Assert.That(clock.CurrentSwing.Duration, Is.EqualTo(capturedDuration).Within(0.0001f));
            Assert.IsFalse(clock.Advance(0.49f).SwingCompleted);
            Assert.IsTrue(clock.Advance(0.01f).SwingCompleted);
        }
    }
}

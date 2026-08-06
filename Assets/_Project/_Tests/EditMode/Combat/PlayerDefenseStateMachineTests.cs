using Castlebound.Gameplay.Combat;
using NUnit.Framework;

namespace Castlebound.Tests.Combat
{
    public class PlayerDefenseStateMachineTests
    {
        private const float ParryWindow = 0.15f;
        private const float RecoveryDuration = 0.15f;

        [Test]
        public void BeginDefense_FromIdle_EntersParryWindowWithCapturedCapacity()
        {
            var stateMachine = new PlayerDefenseStateMachine();

            bool started = stateMachine.BeginDefense(ParryWindow, 2);

            Assert.IsTrue(started);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.ParryWindow));
            Assert.That(stateMachine.RemainingParryCapacity, Is.EqualTo(2));
            Assert.IsTrue(stateMachine.IsGuarding);
            Assert.IsFalse(stateMachine.CanAttack);
        }

        [Test]
        public void BeginDefense_WithZeroCapacity_EntersBlocking()
        {
            var stateMachine = new PlayerDefenseStateMachine();

            bool started = stateMachine.BeginDefense(ParryWindow, 0);

            Assert.IsTrue(started);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
            Assert.That(stateMachine.RemainingParryCapacity, Is.Zero);
        }

        [Test]
        public void ConsumeParry_WhenCapacityReachesZero_ImmediatelyEntersBlocking()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);

            bool consumed = stateMachine.TryConsumeParry();

            Assert.IsTrue(consumed);
            Assert.That(stateMachine.RemainingParryCapacity, Is.Zero);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
        }

        [Test]
        public void ConsumeParry_WithRemainingCapacity_KeepsParryWindowActive()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 2);

            bool consumed = stateMachine.TryConsumeParry();

            Assert.IsTrue(consumed);
            Assert.That(stateMachine.RemainingParryCapacity, Is.EqualTo(1));
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.ParryWindow));
        }

        [Test]
        public void ConsumeParry_OutsideParryWindow_IsRejected()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);
            stateMachine.Advance(ParryWindow + 0.001f);

            bool consumed = stateMachine.TryConsumeParry();

            Assert.IsFalse(consumed);
            Assert.That(stateMachine.RemainingParryCapacity, Is.EqualTo(1));
        }

        [Test]
        public void Advance_AtInclusiveParryBoundary_RemainsParryWindow()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);

            stateMachine.Advance(ParryWindow);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.ParryWindow));
        }

        [Test]
        public void Advance_BeyondCapturedParryBoundaryWhileHeld_EntersBlocking()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);

            stateMachine.Advance(ParryWindow + 0.001f);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
            Assert.IsTrue(stateMachine.IsGuarding);
        }

        [TestCase(PlayerDefenseState.ParryWindow)]
        [TestCase(PlayerDefenseState.Blocking)]
        public void ReleaseDefense_FromGuarding_EntersRecovery(PlayerDefenseState releaseState)
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);
            if (releaseState == PlayerDefenseState.Blocking)
                stateMachine.Advance(ParryWindow + 0.001f);

            bool released = stateMachine.ReleaseDefense(RecoveryDuration);

            Assert.IsTrue(released);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Recovery));
            Assert.IsFalse(stateMachine.IsGuarding);
            Assert.IsFalse(stateMachine.CanAttack);
        }

        [Test]
        public void Recovery_BlocksRestartUntilCapturedDurationCompletes()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);
            stateMachine.ReleaseDefense(RecoveryDuration);

            Assert.IsFalse(stateMachine.BeginDefense(ParryWindow, 1));
            stateMachine.Advance(RecoveryDuration - 0.001f);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Recovery));

            stateMachine.Advance(0.001f);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Idle));
            Assert.IsTrue(stateMachine.CanAttack);
            Assert.IsTrue(stateMachine.BeginDefense(ParryWindow, 1));
        }

        [Test]
        public void Advance_LargeDelta_CarriesFromParryThroughHeldBlockingOnly()
        {
            var stateMachine = new PlayerDefenseStateMachine();
            stateMachine.BeginDefense(ParryWindow, 1);

            stateMachine.Advance(10f);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
        }

        [Test]
        public void InvalidActivationValuesAndDelta_AreNormalized()
        {
            var stateMachine = new PlayerDefenseStateMachine();

            stateMachine.BeginDefense(float.NaN, -1);
            stateMachine.Advance(float.NaN);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
            Assert.That(stateMachine.RemainingParryCapacity, Is.Zero);

            stateMachine.ReleaseDefense(-1f);
            stateMachine.Advance(0f);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Idle));
        }
    }
}

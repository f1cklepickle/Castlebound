using Castlebound.Gameplay.Combat;
using NUnit.Framework;

namespace Castlebound.Tests.Combat
{
    public class PlayerDefenseStateMachineTests
    {
        private const float ParryWindow = 0.15f;
        private const float RecoveryDuration = 0.15f;

        [Test]
        public void BeginDefense_FromIdle_EntersParryWindow()
        {
            var stateMachine = new PlayerDefenseStateMachine(ParryWindow, RecoveryDuration);

            bool started = stateMachine.BeginDefense();

            Assert.IsTrue(started);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.ParryWindow));
            Assert.IsTrue(stateMachine.IsGuarding);
            Assert.IsFalse(stateMachine.CanAttack);
        }

        [Test]
        public void Advance_AtInclusiveParryBoundary_RemainsParryWindow()
        {
            var stateMachine = new PlayerDefenseStateMachine(ParryWindow, RecoveryDuration);
            stateMachine.BeginDefense();

            stateMachine.Advance(ParryWindow);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.ParryWindow));
        }

        [Test]
        public void Advance_BeyondParryBoundaryWhileHeld_EntersBlocking()
        {
            var stateMachine = new PlayerDefenseStateMachine(ParryWindow, RecoveryDuration);
            stateMachine.BeginDefense();

            stateMachine.Advance(ParryWindow + 0.001f);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
            Assert.IsTrue(stateMachine.IsGuarding);
        }

        [TestCase(PlayerDefenseState.ParryWindow)]
        [TestCase(PlayerDefenseState.Blocking)]
        public void ReleaseDefense_FromGuarding_EntersRecovery(PlayerDefenseState releaseState)
        {
            var stateMachine = new PlayerDefenseStateMachine(ParryWindow, RecoveryDuration);
            stateMachine.BeginDefense();
            if (releaseState == PlayerDefenseState.Blocking)
                stateMachine.Advance(ParryWindow + 0.001f);

            bool released = stateMachine.ReleaseDefense();

            Assert.IsTrue(released);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Recovery));
            Assert.IsFalse(stateMachine.IsGuarding);
            Assert.IsFalse(stateMachine.CanAttack);
        }

        [Test]
        public void Recovery_BlocksRestartUntilDurationCompletes()
        {
            var stateMachine = new PlayerDefenseStateMachine(ParryWindow, RecoveryDuration);
            stateMachine.BeginDefense();
            stateMachine.ReleaseDefense();

            Assert.IsFalse(stateMachine.BeginDefense());
            stateMachine.Advance(RecoveryDuration - 0.001f);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Recovery));

            stateMachine.Advance(0.001f);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Idle));
            Assert.IsTrue(stateMachine.CanAttack);
            Assert.IsTrue(stateMachine.BeginDefense());
        }

        [Test]
        public void Advance_LargeDelta_CarriesFromParryThroughHeldBlockingOnly()
        {
            var stateMachine = new PlayerDefenseStateMachine(ParryWindow, RecoveryDuration);
            stateMachine.BeginDefense();

            stateMachine.Advance(10f);

            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
        }

        [Test]
        public void InvalidDurationsAndDelta_AreNormalized()
        {
            var stateMachine = new PlayerDefenseStateMachine(float.NaN, -1f);

            stateMachine.BeginDefense();
            stateMachine.Advance(float.NaN);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.ParryWindow));

            stateMachine.Advance(0.001f);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Blocking));
            stateMachine.ReleaseDefense();
            stateMachine.Advance(0f);
            Assert.That(stateMachine.State, Is.EqualTo(PlayerDefenseState.Idle));
        }
    }
}

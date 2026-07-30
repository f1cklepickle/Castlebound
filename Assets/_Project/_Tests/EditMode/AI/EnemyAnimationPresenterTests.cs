using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyAnimationPresenterTests
    {
        private GameObject enemy;
        private EnemyAnimationPresenter presenter;

        [SetUp]
        public void SetUp()
        {
            enemy = new GameObject("EnemyAnimationPresenterTests");
            presenter = enemy.AddComponent<EnemyAnimationPresenter>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void ResolveAttackFrame_ReachesImpactAtAuthoritativeWindup()
        {
            Assert.That(
                new EnemyAttackAnimationTiming().ResolveFrame(
                    elapsedSeconds: 0.3f,
                    authoritativeWindupSeconds: 0.3f,
                    frameCount: 7,
                    impactFrameIndex: 6),
                Is.EqualTo(6));
        }

        [Test]
        public void ResolveAttackFrame_HoldsDrawBackFrameForReadableWindup()
        {
            var timing = new EnemyAttackAnimationTiming();

            Assert.That(timing.ResolveFrame(0.12f, 0.3f, 7, 6), Is.EqualTo(3));
            Assert.That(timing.ResolveFrame(0.17f, 0.3f, 7, 6), Is.EqualTo(3));
            Assert.That(timing.ResolveFrame(0.18f, 0.3f, 7, 6), Is.EqualTo(4));
        }

        [Test]
        public void ResolveAttackFrame_ShowsFullyDrawnThenMidSwingBeforeImpact()
        {
            Assert.That(
                new EnemyAttackAnimationTiming().ResolveFrame(
                    elapsedSeconds: 0.2f,
                    authoritativeWindupSeconds: 0.3f,
                    frameCount: 7,
                    impactFrameIndex: 6),
                Is.EqualTo(4));
            Assert.That(new EnemyAttackAnimationTiming().ResolveFrame(0.25f, 0.3f, 7, 6), Is.EqualTo(5));
        }

        [Test]
        public void ResolveAttackFrame_HoldsImpactBeforeReturningToNeutral()
        {
            var timing = new EnemyAttackAnimationTiming();

            Assert.That(timing.ResolveFrame(0.3f, 0.3f, 7, 6), Is.EqualTo(6));
            Assert.That(timing.ResolveFrame(0.359f, 0.3f, 7, 6), Is.EqualTo(6));
        }

        [Test]
        public void IsAttackComplete_UsesPresentationDurationOnly()
        {
            var timing = new EnemyAttackAnimationTiming();
            Assert.IsFalse(timing.IsComplete(0.359f, 0.3f));
            Assert.IsTrue(timing.IsComplete(0.361f, 0.3f));
        }

        [Test]
        public void ResolveAttackFrame_ScalesSameSequenceAcrossAttackSpeeds()
        {
            var timing = new EnemyAttackAnimationTiming();

            Assert.That(timing.ResolveFrame(0.1f, 0.2f, 7, 6), Is.EqualTo(3));
            Assert.That(timing.ResolveFrame(0.15f, 0.3f, 7, 6), Is.EqualTo(3));
            Assert.That(timing.ResolveFrame(0.225f, 0.45f, 7, 6), Is.EqualTo(3));
            Assert.That(timing.ResolveFrame(0.2f, 0.2f, 7, 6), Is.EqualTo(6));
            Assert.That(timing.ResolveFrame(0.45f, 0.45f, 7, 6), Is.EqualTo(6));
        }

        [Test]
        public void MovementRequest_PlaysWalkUntilAttackTakesPriority()
        {
            presenter.SetMovementRequested(true);
            presenter.Advance(0.1f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Walk));

            presenter.PlayAttack(0.3f);
            presenter.Advance(0.1f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Attack));

            presenter.Advance(0.261f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Walk));
        }

        [Test]
        public void StationaryPresentation_WaitsTwoSecondsBeforeIdle()
        {
            presenter.SetMovementRequested(true);
            presenter.Advance(0.1f);
            presenter.SetMovementRequested(false);

            presenter.Advance(1.99f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));

            presenter.Advance(0.01f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Idle));
        }

        [Test]
        public void CompletedAttack_HoldsNeutralPoseBeforeIdle()
        {
            presenter.PlayAttack(0.3f);
            presenter.Advance(0.361f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));

            presenter.Advance(1.99f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));

            presenter.Advance(0.01f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Idle));
        }
    }
}

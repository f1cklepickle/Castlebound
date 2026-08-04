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
            enemy.AddComponent<Animator>();
            presenter = enemy.AddComponent<EnemyAnimationPresenter>();
            presenter.InitializePresentation();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(enemy);

        [Test]
        public void AttackSpeed_AlignsAuthoredImpactToAuthoritativeWindup()
        {
            Assert.That(EnemyAnimationPresenter.CalculateAttackSpeed(1f / 3f, 0.3f), Is.EqualTo((1f / 3f) / 0.3f));
            Assert.That(EnemyAnimationPresenter.CalculateAttackSpeed(1f / 3f, 0.5f), Is.EqualTo((1f / 3f) / 0.5f));
        }

        [Test]
        public void NormalizedClockProgress_MapsImpactAndRecoveryAcrossOneAuthoredCycle()
        {
            Assert.That(EnemyAnimationPresenter.MapAttackProgress(0.25f, 0.25f, 0.5f),
                Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(EnemyAnimationPresenter.MapAttackProgress(0.625f, 0.25f, 0.5f),
                Is.EqualTo(0.75f).Within(0.0001f));
            Assert.That(EnemyAnimationPresenter.MapAttackProgress(1f, 0.25f, 0.5f),
                Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void AttackPresentation_PersistsUntilCombatAuthorityCompletesIt()
        {
            presenter.PlayAttack(0.3f);
            presenter.Advance(5f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Attack));

            presenter.CompleteAttack();
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));
        }

        [Test]
        public void MovementRequest_DoesNotInterruptAttack()
        {
            presenter.PlayAttack(0.3f);
            presenter.SetMovementRequested(true);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Attack));

            presenter.CompleteAttack();
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Walk));
        }

        [Test]
        public void MovementStop_ReturnsToNeutralBeforeDelayedIdle()
        {
            presenter.SetMovementRequested(true);
            presenter.SetMovementRequested(false);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));

            presenter.Advance(2f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Idle));
        }

        [Test]
        public void StationaryPresentation_WaitsTwoSecondsBeforeIdle()
        {
            presenter.Advance(1.99f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Hold));
            presenter.Advance(0.01f);
            Assert.That(presenter.CurrentState, Is.EqualTo(EnemyAnimationPresenter.PresentationState.Idle));
        }
    }
}

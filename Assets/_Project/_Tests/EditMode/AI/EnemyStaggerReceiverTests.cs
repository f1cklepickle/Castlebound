using Castlebound.Gameplay.AI;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyStaggerReceiverTests
    {
        private GameObject enemy;
        private EnemyAttack attack;
        private EnemyStaggerReceiver receiver;

        [SetUp]
        public void SetUp()
        {
            enemy = new GameObject("Enemy");
            enemy.AddComponent<Rigidbody2D>();
            enemy.AddComponent<EnemyController2D>();
            attack = enemy.AddComponent<EnemyAttack>();
            receiver = enemy.AddComponent<EnemyStaggerReceiver>();
            receiver.Configure(true, 1f, attack);
        }

        [TearDown]
        public void TearDown()
        {
            if (enemy != null)
                Object.DestroyImmediate(enemy);
        }

        [Test]
        public void TryStagger_EligibleEnemy_EntersStaggeredForAuthoredDuration()
        {
            bool started = receiver.TryStagger();

            Assert.IsTrue(started);
            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.Staggered));
            Assert.That(receiver.RemainingSeconds, Is.EqualTo(1f));
            Assert.IsTrue(receiver.IsActionLocked);
        }

        [Test]
        public void Tick_WhenDurationCompletes_AwaitsTargetRefreshWhileRemainingLocked()
        {
            receiver.TryStagger();

            receiver.Tick(0.999f);
            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.Staggered));

            receiver.Tick(0.001f);

            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.AwaitingTargetRefresh));
            Assert.That(receiver.RemainingSeconds, Is.Zero);
            Assert.IsTrue(receiver.IsActionLocked);
        }

        [Test]
        public void AcknowledgeTargetRefresh_AfterTimer_ReleasesActionLock()
        {
            receiver.TryStagger();
            receiver.Tick(1f);

            bool acknowledged = receiver.AcknowledgeTargetRefresh();

            Assert.IsTrue(acknowledged);
            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.Inactive));
            Assert.IsFalse(receiver.IsActionLocked);
        }

        [Test]
        public void TryStagger_WhileAlreadyLocked_IsIgnoredWithoutRefreshingDuration()
        {
            receiver.TryStagger();
            receiver.Tick(0.4f);

            bool repeated = receiver.TryStagger();

            Assert.IsFalse(repeated);
            Assert.That(receiver.RemainingSeconds, Is.EqualTo(0.6f).Within(0.0001f));
        }

        [Test]
        public void TryStagger_WhenIneligible_IsRejected()
        {
            receiver.Configure(false, 1f, attack);

            Assert.IsFalse(receiver.TryStagger());
            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.Inactive));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void TryStagger_WithInvalidDuration_IsRejected(float duration)
        {
            receiver.Configure(true, duration, attack);

            Assert.IsFalse(receiver.TryStagger());
            Assert.That(receiver.RemainingSeconds, Is.Zero);
        }

        [Test]
        public void TryStagger_WithoutExplicitAttackReference_IsRejected()
        {
            receiver.Configure(true, 1f, null);

            Assert.IsFalse(receiver.TryStagger());
            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.Inactive));
        }

        [Test]
        public void OnDisable_ClearsStaggerStateAndRemainingTime()
        {
            receiver.TryStagger();

            MethodInfo onDisable = typeof(EnemyStaggerReceiver)
                .GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(onDisable);
            onDisable.Invoke(receiver, null);

            Assert.That(receiver.State, Is.EqualTo(EnemyStaggerState.Inactive));
            Assert.That(receiver.RemainingSeconds, Is.Zero);
            Assert.IsFalse(receiver.IsActionLocked);
        }
    }
}

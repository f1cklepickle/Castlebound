using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class EnemyHitBarrierFeedbackTests
    {
        [Test]
        public void EnemyAttack_DealsDamageToBarrier_RaisesEnemyHitBarrierFeedbackCue()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();

            var enemy = new GameObject("Enemy");
            var attack = enemy.AddComponent<EnemyAttack>();
            attack.Damage = 1;

            var field = typeof(EnemyAttack).GetField("enemyHitBarrierFeedbackChannel", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "EnemyAttack should define an enemyHitBarrierFeedbackChannel field for barrier-hit feedback.");
            field.SetValue(attack, channel);

            var barrierGo = new GameObject("Barrier");
            var barrier = barrierGo.AddComponent<BarrierHealth>();
            barrier.MaxHealth = 5;
            barrier.CurrentHealth = 5;

            var raised = false;
            FeedbackCue received = default;
            channel.Raised += cue =>
            {
                raised = true;
                received = cue;
            };

            attack.DealDamage(barrier);

            Assert.IsTrue(raised, "Enemy hit barrier feedback cue should be raised when barrier takes damage.");
            Assert.AreEqual(FeedbackCueType.EnemyHitBarrier, received.Type, "Feedback cue type should be EnemyHitBarrier.");
            Assert.That(received.Position, Is.EqualTo(barrier.transform.position));

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(barrierGo);
            Object.DestroyImmediate(channel);
        }
    }
}

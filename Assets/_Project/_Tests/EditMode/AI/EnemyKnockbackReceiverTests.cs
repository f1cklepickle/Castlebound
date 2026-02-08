using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Tests.AI
{
    public class EnemyKnockbackReceiverTests
    {
        [Test]
        public void ProvidesDecayingDisplacement()
        {
            var enemy = new GameObject("Enemy");
            var receiver = enemy.AddComponent<EnemyKnockbackReceiver>();

            receiver.AddKnockback(new Vector2(10f, 0f), 5f);

            var first = receiver.ConsumeDisplacement(0.1f);
            var second = receiver.ConsumeDisplacement(0.1f);

            Assert.Greater(first.x, 0.01f, "Expected initial knockback displacement.");
            Assert.Less(second.x, first.x, "Expected knockback to decay over time.");

            Object.DestroyImmediate(enemy);
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTests
    {
        private class DummyDamageable : IDamageable
        {
            public int DamageTaken { get; private set; }
            public void TakeDamage(int amount) => DamageTaken += amount;
        }

        [Test]
        public void EnemyAttack_DealsDamage_ToIDamageableTarget()
        {
            // Arrange
            var go = new GameObject("Enemy");
            var attack = go.AddComponent<EnemyAttack>();

            // These members do NOT exist yet. Test must fail until we implement them:
            attack.Damage = 3;

            var dummy = new DummyDamageable();

            // Act
            attack.DealDamage(dummy);

            // Assert
            Assert.AreEqual(3, dummy.DamageTaken,
                "EnemyAttack should deal its configured Damage to any IDamageable target.");
        }
    }
}

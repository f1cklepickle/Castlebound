using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Gate
{
    public class BarrierHealthTests
    {
        [Test]
        public void Breaks_WhenHealthReachesZero()
        {
            var go = new GameObject("Gate");
            var health = go.AddComponent<BarrierHealth>();

            health.MaxHealth = 10;
            health.CurrentHealth = 10;

            health.TakeDamage(10);

            Assert.IsTrue(health.IsBroken, "Gate should be broken when health reaches zero.");
        }
    }
}

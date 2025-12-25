using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Spawning
{
    public class SpawnedEntityLifetimeTests
    {
        [Test]
        public void InvokesCallbackOnceOnDestroy()
        {
            var go = new GameObject("lifetime-test");
            var lifetime = go.AddComponent<SpawnedEntityLifetime>();

            int count = 0;
            lifetime.Initialize(() => count++);

            lifetime.NotifyForTests();
            lifetime.NotifyForTests(); // second call should be ignored

            Assert.AreEqual(1, count, "OnDespawn should be invoked exactly once across disable/destroy.");
        }
    }
}

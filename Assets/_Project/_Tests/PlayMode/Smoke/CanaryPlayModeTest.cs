using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace _Project.Tests.PlayMode
{
    public class CanaryPlayModeTest
    {
        [UnityTest]
        public IEnumerator Canary_ShouldAlwaysPass()
        {
            // Arrange
            Debug.Log("[Canary] PlayMode test started — verifying runner is active.");

            // Act
            yield return null; // simulate a single frame in PlayMode

            // Assert
            Assert.IsTrue(Application.isPlaying, "[Canary] Application should be in Play Mode during PlayMode tests.");
            Debug.Log("[Canary] PlayMode test completed successfully.");
        }
    }
}

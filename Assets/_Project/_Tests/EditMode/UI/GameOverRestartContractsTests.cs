using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Castlebound.Tests.UI
{
    public class GameOverRestartContractsTests
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/_Project/Scenes/MainPrototype.unity",
            "Assets/_Project/Scenes/SampleScene.unity"
        };

        [Test]
        public void GameManager_ExposesUiRestartEntryPoint()
        {
            var method = typeof(GameManager).GetMethod(
                "RequestRestart",
                BindingFlags.Instance | BindingFlags.Public);

            Assert.IsNotNull(method,
                "GameManager must expose RequestRestart() so UI buttons and keyboard input can trigger one shared restart path.");
        }

        [Test]
        public void DeathScreenPrompt_DoesNotUseSpaceOnlyCopy()
        {
            foreach (var scenePath in ScenePaths)
            {
                var text = File.ReadAllText(scenePath);
                StringAssert.DoesNotContain("Press Space - Restart", text,
                    $"Scene '{scenePath}' should not ship with a spacebar-only restart prompt.");
            }
        }

        [Test]
        public void DeathScreenPrompt_UsesTouchFriendlyRestartCopy()
        {
            foreach (var scenePath in ScenePaths)
            {
                var text = File.ReadAllText(scenePath);
                var hasTouchCopy =
                    text.Contains("Tap Restart") ||
                    text.Contains("Tap to Restart") ||
                    text.Contains("Tap the Restart button");

                Assert.IsTrue(hasTouchCopy,
                    $"Scene '{scenePath}' should include touch-friendly restart copy.");
            }
        }
    }
}

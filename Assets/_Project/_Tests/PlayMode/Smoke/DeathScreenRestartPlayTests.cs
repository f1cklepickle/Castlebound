using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.UI
{
    public class DeathScreenRestartPlayTests
    {
        [UnityTest]
        public IEnumerator OnPlayerDied_GameOverUiContainsTappableRestartButton()
        {
            yield return LoadMainPrototype();

            var manager = Object.FindObjectOfType<GameManager>();
            Assert.NotNull(manager, "Expected GameManager in MainPrototype.");

            manager.OnPlayerDied();

            var gameOverUi = GetGameOverUi(manager);
            Assert.NotNull(gameOverUi, "GameManager should reference a game-over UI object.");
            Assert.IsTrue(gameOverUi.activeSelf, "Game-over UI should be active after player death.");

            var buttonComponents = gameOverUi
                .GetComponentsInChildren<MonoBehaviour>(true)
                .Where(component => component.GetType().Name == "Button")
                .ToArray();

            Assert.That(buttonComponents.Length, Is.GreaterThan(0),
                "Death screen should include a tappable Restart button.");
        }

        private static IEnumerator LoadMainPrototype()
        {
            var load = SceneManager.LoadSceneAsync("MainPrototype", LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static GameObject GetGameOverUi(GameManager manager)
        {
            var field = typeof(GameManager).GetField(
                "gameOverUI",
                BindingFlags.Instance | BindingFlags.NonPublic);

            return field?.GetValue(manager) as GameObject;
        }
    }
}

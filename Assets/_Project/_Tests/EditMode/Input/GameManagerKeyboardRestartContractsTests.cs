using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class GameManagerKeyboardRestartContractsTests
    {
        private const string GameManagerPath = "Assets/_Project/Scripts/_Project.Gameplay/Managers/GameManager.cs";

        [Test]
        public void SpaceRestartContract_IsPresentForInputSystemAndLegacyInput()
        {
            var source = File.ReadAllText(GameManagerPath);

            StringAssert.Contains("Keyboard.current.spaceKey.wasPressedThisFrame", source,
                "GameManager should support Space restart through the new Input System.");
            StringAssert.Contains("Input.GetKeyDown(KeyCode.Space)", source,
                "GameManager should retain legacy Space restart fallback.");
        }
    }
}

using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class DefenseInputContractsTests
    {
        private const string InputActionsPath =
            "Assets/_Project/Settings/Input/PlayerControls.inputactions";
        private const string GeneratedControlsPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/PlayerControls.cs";
        private const string MobileDriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/MobileInputDriver.cs";
        private const string MainPrototypePath = "Assets/_Project/Scenes/MainPrototype.unity";

        [Test]
        public void PlayerControls_DefendUsesRequiredDesktopAndGamepadBindings()
        {
            string actions = File.ReadAllText(InputActionsPath);
            string generated = File.ReadAllText(GeneratedControlsPath);

            StringAssert.Contains("\"name\": \"Defend\"", actions);
            StringAssert.Contains("<Mouse>/rightButton", actions);
            StringAssert.Contains("<Gamepad>/leftTrigger", actions);
            StringAssert.Contains("m_Player_Defend", generated);
        }

        [Test]
        public void MobileDefense_SuppressesAttackAndUsesIndependentLeftStick()
        {
            string source = File.ReadAllText(MobileDriverPath);

            StringAssert.Contains("state.leftStick = movementZone.MoveVector", source);
            StringAssert.Contains("defenseAimButton.IsDefending", source);
            StringAssert.Contains("state.leftTrigger = 1f", source);
            StringAssert.Contains("state.rightTrigger = 0f", source);
        }

        [Test]
        public void MainPrototype_WiresSmallTouchDefenseAimButton()
        {
            string scene = File.ReadAllText(MainPrototypePath);

            StringAssert.Contains("m_Name: TouchDefenseAimButton", scene);
            StringAssert.Contains("defenseAimButton: {fileID: 3004000004}", scene);
            StringAssert.Contains("m_SizeDelta: {x: 120, y: 120}", scene);
            StringAssert.Contains("softAnchorRadius: 160", scene);
            StringAssert.Contains("maxAnchorDrift: 170", scene);
        }
    }
}

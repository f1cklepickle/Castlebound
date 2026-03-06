using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class MobileInputDriverEditorGatingContractsTests
    {
        private const string DriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/MobileInputDriver.cs";

        [Test]
        public void EditorGating_UsesTouchscreenPresenceForAutoEnable()
        {
            var source = File.ReadAllText(DriverPath);

            StringAssert.Contains("Touchscreen.current != null", source,
                "MobileInputDriver should auto-enable in editor when a touch device is present.");
            StringAssert.Contains("enableInEditor", source,
                "MobileInputDriver should keep an explicit editor override toggle.");
        }
    }
}

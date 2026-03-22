using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Input
{
    public class MobileInputDriverOwnershipContractsTests
    {
        private const string MobileInputDriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Input/MobileInputDriver.cs";
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";

        [Test]
        public void MobileInputDriver_DoesNotExposeAttackRateApi()
        {
            var source = File.ReadAllText(MobileInputDriverPath);

            StringAssert.DoesNotContain("SetAttackRate(", source,
                "Mobile input should not expose an attack-rate API once cadence authority lives in the attack runtime.");
            StringAssert.DoesNotContain("baseAttackRate", source,
                "Mobile input should not retain a stale attack-rate field once it only forwards held intent.");
        }

        [Test]
        public void PlayerController_DoesNotSyncAttackRate_IntoMobileInput()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.DoesNotContain("SyncMobileAttackRate(", source,
                "PlayerController should not push attack cadence into MobileInputDriver once mobile input is intent-only.");
            StringAssert.DoesNotContain("mobileInputDriver.SetAttackRate", source,
                "PlayerController should not call a removed mobile attack-rate API.");
        }
    }
}

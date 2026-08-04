using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackAuthorityOwnershipContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string AttackLoopPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackLoop.cs";

        [Test]
        public void PlayerController_DoesNotOwnAttackCooldownAuthority()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.DoesNotContain("PlayerAttackCooldownGate", source,
                "Attack cooldown authority should move out of PlayerController.");
            StringAssert.DoesNotContain("attackCooldownGate", source,
                "PlayerController should not retain a cooldown gate field once attack authority lives in the loop runtime.");
        }

        [Test]
        public void PlayerController_DoesNotExposeDirectAttackAuthorityMethods()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.DoesNotContain("TryTriggerAttack(", source,
                "PlayerController should not own direct attack trigger authority.");
            StringAssert.DoesNotContain("SetHitWindowActive(", source,
                "PlayerController should not translate runtime attack phases into hitbox window authority.");
        }

        [Test]
        public void PlayerAttackLoop_DelegatesTimingToSharedAttackClock()
        {
            var source = File.ReadAllText(AttackLoopPath);

            StringAssert.Contains("AttackClock", source,
                "Player attacks should consume the holder-neutral timing authority.");
            StringAssert.DoesNotContain("PlayerAttackCooldownGate", source,
                "Player attacks should not layer a second cooldown authority over the shared clock.");
        }

        [Test]
        public void PlayerAttackLoop_DoesNotRequireExternalAttackCallbacks()
        {
            var source = File.ReadAllText(AttackLoopPath);

            StringAssert.DoesNotContain("Func<bool> tryStartSwing", source,
                "Attack loop runtime should not require an external attack callback once it owns cadence authority.");
            StringAssert.DoesNotContain("Action<bool> setHitWindowActive", source,
                "Attack loop runtime should expose hit-window state directly rather than pushing callbacks into the controller.");
        }
    }
}

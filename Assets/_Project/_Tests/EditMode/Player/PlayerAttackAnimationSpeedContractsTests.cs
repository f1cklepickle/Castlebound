using System.IO;
using NUnit.Framework;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackAnimationSpeedContractsTests
    {
        private const string PlayerControllerPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/PlayerController.cs";
        private const string AttackAnimationDriverPath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackAnimationDriver.cs";

        [Test]
        public void PlayerController_DelegatesAttackAnimationSpeed_ToDedicatedDriver()
        {
            var source = File.ReadAllText(PlayerControllerPath);

            StringAssert.Contains("PlayerAttackAnimationDriver", source,
                "PlayerController should delegate animator attack-speed updates to a dedicated component.");
            StringAssert.Contains("attackAnimationDriver.ApplyLoopPresentation", source,
                "PlayerController should invoke attackAnimationDriver.ApplyLoopPresentation(...) each frame/tick path.");
        }

        [Test]
        public void AttackAnimationDriver_Exists_WithAnimatorParameterContract()
        {
            Assert.That(File.Exists(AttackAnimationDriverPath), Is.True,
                "A dedicated PlayerAttackAnimationDriver component should exist.");

            var source = File.ReadAllText(AttackAnimationDriverPath);

            StringAssert.Contains("[SerializeField] private string attackSpeedParameter", source,
                "Driver should expose an animator float parameter name for attack speed.");
            StringAssert.Contains("animator.SetFloat", source,
                "Driver should apply attack speed through Animator.SetFloat.");
        }

        [Test]
        public void AttackAnimationDriver_ClampsAnimationSpeedMultiplier()
        {
            Assert.That(File.Exists(AttackAnimationDriverPath), Is.True,
                "Driver must exist before validating multiplier clamp contract.");

            var source = File.ReadAllText(AttackAnimationDriverPath);

            StringAssert.Contains("[SerializeField] private float minAttackSpeedMultiplier", source,
                "Driver should define a minimum multiplier clamp.");
            StringAssert.Contains("[SerializeField] private float maxAttackSpeedMultiplier", source,
                "Driver should define a maximum multiplier clamp.");
            StringAssert.Contains("Mathf.Clamp", source,
                "Driver should clamp computed animation speed to a safe range.");
        }
    }
}

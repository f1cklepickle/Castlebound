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
        private const string AttackRuntimePath =
            "Assets/_Project/Scripts/_Project.Gameplay/Player/Components/PlayerAttackRuntime.cs";

        [Test]
        public void PlayerAttackRuntime_DelegatesAttackAnimationSpeed_ToDedicatedDriver()
        {
            var controllerSource = File.ReadAllText(PlayerControllerPath);
            var runtimeSource = File.ReadAllText(AttackRuntimePath);

            StringAssert.Contains("PlayerAttackAnimationDriver", controllerSource,
                "PlayerController should retain its dedicated presentation adapter reference.");
            StringAssert.Contains("animationDriver.ApplyLoopPresentation", runtimeSource,
                "The extracted runtime should invoke the presentation adapter each tick.");
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

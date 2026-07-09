using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;

namespace Castlebound.Tests.Castle
{
    public class VaultInteractionPolicyTests
    {
        [Test]
        public void CanOpen_RequiresPlayerRange_AndPreWave()
        {
            Assert.IsTrue(VaultInteractionPolicy.CanOpen(true, WavePhase.PreWave));
            Assert.IsFalse(VaultInteractionPolicy.CanOpen(false, WavePhase.PreWave));
            Assert.IsFalse(VaultInteractionPolicy.CanOpen(true, WavePhase.InWave));
        }

        [Test]
        public void GetVisualState_UsesHiddenAccessibleBlockedContract()
        {
            Assert.That(
                VaultInteractionPolicy.GetVisualState(false, WavePhase.PreWave),
                Is.EqualTo(VaultInteractionVisualState.Hidden));
            Assert.That(
                VaultInteractionPolicy.GetVisualState(true, WavePhase.PreWave),
                Is.EqualTo(VaultInteractionVisualState.Accessible));
            Assert.That(
                VaultInteractionPolicy.GetVisualState(true, WavePhase.InWave),
                Is.EqualTo(VaultInteractionVisualState.Blocked));
        }
    }
}

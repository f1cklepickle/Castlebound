using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Barrier
{
    public class BarrierUpgradeActionTests
    {
        [Test]
        public void Upgrade_Succeeds_InPreWave_And_RaisesSuccessFeedback()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 10;
            config.CostPerTier = 5;

            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.PreWave);

            var inventory = new InventoryState();
            inventory.AddGold(20);

            var target = new GameObject("Barrier");
            var barrierHealth = target.AddComponent<BarrierHealth>();
            var controller = target.AddComponent<BarrierUpgradeController>();
            controller.Config = config;
            controller.SetInventory(inventory);
            controller.SetPhaseTracker(phase);
            controller.SetFeedbackChannel(channel);

            bool raised = false;
            FeedbackCueType? raisedType = null;
            int raisedTarget = 0;
            channel.Raised += cue =>
            {
                raised = true;
                raisedType = cue.Type;
                raisedTarget = cue.TargetInstanceId;
            };

            bool upgraded = controller.TryUpgrade();

            Assert.IsTrue(upgraded, "Upgrade should succeed when pre-wave and gold is sufficient.");
            Assert.That(controller.State.Tier, Is.EqualTo(1), "Tier should increment after upgrade.");
            Assert.That(inventory.Gold, Is.EqualTo(10), "Gold should be spent on success.");
            Assert.IsTrue(raised, "Success feedback should be raised.");
            Assert.That(raisedType, Is.EqualTo(FeedbackCueType.UpgradeSuccess));
            Assert.That(raisedTarget, Is.EqualTo(barrierHealth.GetInstanceID()));

            Object.DestroyImmediate(target);
        }

        [Test]
        public void Upgrade_Denied_WhenGoldInsufficient_InPreWave()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 10;
            config.CostPerTier = 5;

            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.PreWave);

            var inventory = new InventoryState();
            inventory.AddGold(5);

            var target = new GameObject("Barrier");
            var controller = target.AddComponent<BarrierUpgradeController>();
            controller.Config = config;
            controller.SetInventory(inventory);
            controller.SetPhaseTracker(phase);
            controller.SetFeedbackChannel(channel);

            bool raised = false;
            FeedbackCueType? raisedType = null;
            channel.Raised += cue =>
            {
                raised = true;
                raisedType = cue.Type;
            };

            bool upgraded = controller.TryUpgrade();

            Assert.IsFalse(upgraded, "Upgrade should fail when gold is insufficient.");
            Assert.That(controller.State.Tier, Is.EqualTo(0), "Tier should not change on denial.");
            Assert.That(inventory.Gold, Is.EqualTo(5), "Gold should not be spent on denial.");
            Assert.IsTrue(raised, "Denied feedback should be raised.");
            Assert.That(raisedType, Is.EqualTo(FeedbackCueType.UpgradeDenied));

            Object.DestroyImmediate(target);
        }

        [Test]
        public void Upgrade_Denied_WhenInWave_EvenWithGold()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 10;
            config.CostPerTier = 5;

            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.InWave);

            var inventory = new InventoryState();
            inventory.AddGold(50);

            var target = new GameObject("Barrier");
            var controller = target.AddComponent<BarrierUpgradeController>();
            controller.Config = config;
            controller.SetInventory(inventory);
            controller.SetPhaseTracker(phase);
            controller.SetFeedbackChannel(channel);

            bool raised = false;
            FeedbackCueType? raisedType = null;
            channel.Raised += cue =>
            {
                raised = true;
                raisedType = cue.Type;
            };

            bool upgraded = controller.TryUpgrade();

            Assert.IsFalse(upgraded, "Upgrade should fail during active wave.");
            Assert.That(controller.State.Tier, Is.EqualTo(0), "Tier should not change on denial.");
            Assert.That(inventory.Gold, Is.EqualTo(50), "Gold should not be spent on denial.");
            Assert.IsTrue(raised, "Denied feedback should be raised.");
            Assert.That(raisedType, Is.EqualTo(FeedbackCueType.UpgradeDenied));

            Object.DestroyImmediate(target);
        }

        [Test]
        public void Upgrade_Denied_WhenMenuClosedStartsWave()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseCost = 10;
            config.CostPerTier = 5;

            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.PreWave);

            var inventory = new InventoryState();
            inventory.AddGold(50);

            var target = new GameObject("Barrier");
            var controller = target.AddComponent<BarrierUpgradeController>();
            controller.Config = config;
            controller.SetInventory(inventory);
            controller.SetPhaseTracker(phase);
            controller.SetFeedbackChannel(channel);

            phase.SetPhase(WavePhase.InWave);

            bool upgraded = controller.TryUpgrade();

            Assert.IsFalse(upgraded, "Upgrade should fail during active wave.");

            Object.DestroyImmediate(target);
        }
    }
}

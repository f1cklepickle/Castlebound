using Castlebound.Gameplay.Barrier;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Barrier
{
    public class BarrierUpgradePersistenceTests
    {
        [Test]
        public void SharedState_PersistsAcrossWaveTransitions()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseMaxHealth = 10;
            config.MaxHealthPerTier = 2;

            var sharedState = ScriptableObject.CreateInstance<BarrierUpgradeStateAsset>();
            sharedState.Tier = 3;

            var go = new GameObject("Barrier");
            var health = go.AddComponent<BarrierHealth>();
            health.MaxHealth = 10;
            health.CurrentHealth = 7;

            var controller = go.AddComponent<BarrierUpgradeController>();
            controller.Config = config;
            controller.SharedState = sharedState;

            controller.ApplyCurrentTier();

            Assert.That(health.MaxHealth, Is.EqualTo(16), "Shared tier should apply max health scaling.");
            Assert.That(health.CurrentHealth, Is.EqualTo(7), "Current health should not reset on apply.");

            controller.HandleWaveStarted();

            Assert.That(health.MaxHealth, Is.EqualTo(16), "Wave transition should keep the shared tier applied.");
            Assert.That(health.CurrentHealth, Is.EqualTo(7), "Current health should not reset on wave start.");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void LocalState_PersistsAcrossWaveTransitions()
        {
            var config = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            config.BaseMaxHealth = 8;
            config.MaxHealthPerTier = 1;

            var go = new GameObject("Barrier");
            var health = go.AddComponent<BarrierHealth>();
            health.MaxHealth = 8;
            health.CurrentHealth = 5;

            var controller = go.AddComponent<BarrierUpgradeController>();
            controller.Config = config;
            controller.State.IncrementTier();
            controller.State.IncrementTier();

            controller.ApplyCurrentTier();

            Assert.That(health.MaxHealth, Is.EqualTo(10), "Local tier should apply max health scaling.");
            Assert.That(health.CurrentHealth, Is.EqualTo(5), "Current health should not reset on apply.");

            controller.HandleWaveStarted();

            Assert.That(health.MaxHealth, Is.EqualTo(10), "Wave transition should keep the local tier applied.");
            Assert.That(health.CurrentHealth, Is.EqualTo(5), "Current health should not reset on wave start.");

            Object.DestroyImmediate(go);
        }
    }
}

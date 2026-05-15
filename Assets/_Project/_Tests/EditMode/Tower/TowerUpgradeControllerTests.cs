using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Tower
{
    public class TowerUpgradeControllerTests
    {
        [Test]
        public void TryUpgrade_DamageTrack_SpendsGoldAndAppliesDamageOnly()
        {
            var fixture = CreateFixture();

            try
            {
                Assert.IsTrue(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Damage));

                Assert.That(fixture.Inventory.Gold, Is.EqualTo(90));
                Assert.That(fixture.Controller.GetLevel(TowerUpgradeTrack.Damage), Is.EqualTo(1));
                Assert.That(fixture.Attack.Damage, Is.EqualTo(3));
                Assert.That(fixture.Attack.CooldownSeconds, Is.EqualTo(1.2f).Within(0.001f));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void TryUpgrade_TracksCanDiverge_PerTowerInstance()
        {
            var sharedConfig = CreateConfig();
            var damageTower = CreateFixture(sharedConfig);
            var rateTower = CreateFixture(sharedConfig);

            try
            {
                Assert.IsTrue(damageTower.Controller.TryUpgrade(TowerUpgradeTrack.Damage));
                Assert.IsTrue(rateTower.Controller.TryUpgrade(TowerUpgradeTrack.FireRate));

                Assert.That(damageTower.Attack.Damage, Is.EqualTo(3));
                Assert.That(damageTower.Attack.CooldownSeconds, Is.EqualTo(1.2f).Within(0.001f));
                Assert.That(rateTower.Attack.Damage, Is.EqualTo(1));
                Assert.That(rateTower.Attack.CooldownSeconds, Is.EqualTo(1.05f).Within(0.001f));
            }
            finally
            {
                damageTower.Destroy(destroyConfig: false);
                rateTower.Destroy(destroyConfig: false);
                Object.DestroyImmediate(sharedConfig);
            }
        }

        [Test]
        public void TryUpgrade_HealthTrack_AddsPurchasedHealthDelta()
        {
            var fixture = CreateFixture();
            fixture.Runtime.MaxHealth = 10;
            fixture.Runtime.CurrentHealth = 4;

            try
            {
                Assert.IsTrue(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Health));

                Assert.That(fixture.Runtime.MaxHealth, Is.EqualTo(14));
                Assert.That(fixture.Runtime.CurrentHealth, Is.EqualTo(8));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void TryUpgrade_RangeTrack_AppliesInstanceRange()
        {
            var fixture = CreateFixture();

            try
            {
                Assert.IsTrue(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Range));

                Assert.That(fixture.Targeting.MaxRange, Is.EqualTo(6f).Within(0.001f));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void TryUpgrade_Denied_WhenTrackDisabledOrMaxed()
        {
            var fixture = CreateFixture();
            fixture.Config.Range.Enabled = false;

            try
            {
                Assert.IsFalse(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Range));
                Assert.That(fixture.Inventory.Gold, Is.EqualTo(100));

                Assert.IsTrue(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Damage));
                Assert.IsTrue(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Damage));
                Assert.IsFalse(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Damage));
                Assert.That(fixture.Controller.GetLevel(TowerUpgradeTrack.Damage), Is.EqualTo(2));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void TryUpgrade_Denied_WhenNotPreWave()
        {
            var fixture = CreateFixture();
            fixture.Phase.SetPhase(WavePhase.InWave);

            try
            {
                Assert.IsFalse(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Damage));

                Assert.That(fixture.Inventory.Gold, Is.EqualTo(100));
                Assert.That(fixture.Attack.Damage, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void TryUpgrade_RaisesSuccessAndDeniedFeedback()
        {
            var fixture = CreateFixture(startingGold: 10);
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            fixture.Controller.SetFeedbackChannel(channel);

            FeedbackCueType? lastType = null;
            channel.Raised += cue => lastType = cue.Type;

            try
            {
                Assert.IsTrue(fixture.Controller.TryUpgrade(TowerUpgradeTrack.Damage));
                Assert.That(lastType, Is.EqualTo(FeedbackCueType.UpgradeSuccess));

                Assert.IsFalse(fixture.Controller.TryUpgrade(TowerUpgradeTrack.FireRate));
                Assert.That(lastType, Is.EqualTo(FeedbackCueType.UpgradeDenied));
            }
            finally
            {
                Object.DestroyImmediate(channel);
                fixture.Destroy();
            }
        }

        [Test]
        public void TrackConfig_ClampsCostsAndResolvesLinearValues()
        {
            var config = new TowerUpgradeTrackConfig
            {
                Enabled = true,
                MaxLevel = -1,
                BaseCost = -5,
                CostPerLevel = -10,
                BaseValue = 1f,
                ValuePerLevel = 2f,
                MinValue = 0f,
                MaxValue = 4f
            };

            config.Normalize();

            Assert.That(config.MaxLevel, Is.EqualTo(0));
            Assert.That(config.GetCostForLevel(2), Is.EqualTo(0));
            config.MaxLevel = 4;
            Assert.That(config.GetValueForLevel(3), Is.EqualTo(4f));
        }

        private static TowerUpgradeFixture CreateFixture(int startingGold = 100)
        {
            return CreateFixture(CreateConfig(), startingGold);
        }

        private static TowerUpgradeFixture CreateFixture(TowerUpgradeConfig config, int startingGold = 100)
        {
            var tower = new GameObject("Tower");
            var runtime = tower.AddComponent<TowerRuntime>();
            var attack = tower.AddComponent<TowerAttackController>();
            var targeting = tower.AddComponent<TowerTargetingController>();
            var controller = tower.AddComponent<TowerUpgradeController>();

            runtime.MaxHealth = 10;
            runtime.CurrentHealth = 10;
            attack.Damage = 1;
            attack.CooldownSeconds = 1.2f;
            targeting.MaxRange = 5f;

            var inventory = new InventoryState();
            inventory.AddGold(startingGold);

            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.PreWave);

            controller.Config = config;
            controller.SetInventory(inventory);
            controller.SetPhaseTracker(phase);
            controller.ApplyCurrentUpgrades();

            return new TowerUpgradeFixture(tower, runtime, attack, targeting, controller, config, inventory, phase);
        }

        private static TowerUpgradeConfig CreateConfig()
        {
            var config = ScriptableObject.CreateInstance<TowerUpgradeConfig>();

            Configure(config.Damage, enabled: true, maxLevel: 2, baseValue: 1f, valuePerLevel: 2f, baseCost: 10, costPerLevel: 5);
            Configure(config.FireRate, enabled: true, maxLevel: 2, baseValue: 1.2f, valuePerLevel: -0.15f, baseCost: 20, costPerLevel: 5, minValue: 0.5f);
            Configure(config.Health, enabled: true, maxLevel: 2, baseValue: 10f, valuePerLevel: 4f, baseCost: 15, costPerLevel: 5, minValue: 1f);
            Configure(config.Range, enabled: true, maxLevel: 2, baseValue: 5f, valuePerLevel: 1f, baseCost: 25, costPerLevel: 5);

            return config;
        }

        private static void Configure(
            TowerUpgradeTrackConfig track,
            bool enabled,
            int maxLevel,
            float baseValue,
            float valuePerLevel,
            int baseCost,
            int costPerLevel,
            float minValue = 0f)
        {
            track.Enabled = enabled;
            track.MaxLevel = maxLevel;
            track.BaseValue = baseValue;
            track.ValuePerLevel = valuePerLevel;
            track.MinValue = minValue;
            track.MaxValue = 9999f;
            track.BaseCost = baseCost;
            track.CostPerLevel = costPerLevel;
        }

        private sealed class TowerUpgradeFixture
        {
            public TowerUpgradeFixture(
                GameObject tower,
                TowerRuntime runtime,
                TowerAttackController attack,
                TowerTargetingController targeting,
                TowerUpgradeController controller,
                TowerUpgradeConfig config,
                InventoryState inventory,
                WavePhaseTracker phase)
            {
                Tower = tower;
                Runtime = runtime;
                Attack = attack;
                Targeting = targeting;
                Controller = controller;
                Config = config;
                Inventory = inventory;
                Phase = phase;
            }

            public GameObject Tower { get; }
            public TowerRuntime Runtime { get; }
            public TowerAttackController Attack { get; }
            public TowerTargetingController Targeting { get; }
            public TowerUpgradeController Controller { get; }
            public TowerUpgradeConfig Config { get; }
            public InventoryState Inventory { get; }
            public WavePhaseTracker Phase { get; }

            public void Destroy(bool destroyConfig = true)
            {
                Object.DestroyImmediate(Tower);
                if (destroyConfig)
                {
                    Object.DestroyImmediate(Config);
                }
            }
        }
    }
}

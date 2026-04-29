using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.Tower;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Tower
{
    public class TowerBuildControllerTests
    {
        [Test]
        public void TryBuild_WithEmptyPlotInPreWave_SpendsGoldSpawnsTowerAndAssignsOccupant()
        {
            var context = CreateContext(startingGold: 75, buildCost: 50);
            context.Anchor.position = new Vector3(3f, 4f, 0f);
            context.Anchor.rotation = Quaternion.Euler(0f, 0f, 45f);

            try
            {
                var result = context.Controller.TryBuild(context.Plot);

                Assert.That(result, Is.EqualTo(TowerBuildResult.Success));
                Assert.That(context.Inventory.Gold, Is.EqualTo(25), "Tower build should spend gold exactly once.");
                Assert.IsTrue(context.Plot.IsOccupied, "Successful build should mark the plot occupied.");
                Assert.NotNull(context.Plot.OccupantInstance, "Successful build should assign the spawned tower to the plot.");
                Assert.AreNotSame(context.TowerPrefab, context.Plot.OccupantInstance, "Build should spawn an instance instead of assigning the prefab.");
                Assert.NotNull(context.Plot.OccupantInstance.GetComponent<TowerRuntime>(), "Build should spawn the configured tower runtime prefab.");
                Assert.That(context.Plot.OccupantInstance.transform.position, Is.EqualTo(context.Anchor.position));
                Assert.That(context.Plot.OccupantInstance.transform.rotation.eulerAngles.z, Is.EqualTo(45f).Within(0.001f));
                Assert.AreSame(context.TowerParent, context.Plot.OccupantInstance.transform.parent);
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void TryBuild_WithOccupiedPlot_RejectsWithoutSpendingOrSpawning()
        {
            var context = CreateContext(startingGold: 75, buildCost: 50);
            var existingTower = new GameObject("ExistingTower");
            context.Plot.TryAssignOccupant(existingTower);

            try
            {
                var result = context.Controller.TryBuild(context.Plot);

                Assert.That(result, Is.EqualTo(TowerBuildResult.Occupied));
                Assert.That(context.Inventory.Gold, Is.EqualTo(75), "Occupied plots should not spend gold.");
                Assert.AreSame(existingTower, context.Plot.OccupantInstance, "Rejected build should keep the existing tower assigned.");
                Assert.That(context.TowerParent.childCount, Is.EqualTo(0), "Occupied plots should not spawn replacement towers.");
            }
            finally
            {
                Object.DestroyImmediate(existingTower);
                context.Destroy();
            }
        }

        [Test]
        public void TryBuild_WithInvalidPrerequisites_RejectsBeforeSpendingOrSpawning()
        {
            var context = CreateContext(startingGold: 75, buildCost: 50);

            try
            {
                Assert.That(context.Controller.TryBuild(null), Is.EqualTo(TowerBuildResult.InvalidPlot));

                context.Phase.SetPhase(WavePhase.InWave);
                Assert.That(context.Controller.TryBuild(context.Plot), Is.EqualTo(TowerBuildResult.NotPreWave));
                context.Phase.SetPhase(WavePhase.PreWave);

                context.Controller.Config = null;
                Assert.That(context.Controller.TryBuild(context.Plot), Is.EqualTo(TowerBuildResult.MissingConfig));

                context.Controller.Config = context.Config;
                context.Config.TowerPrefab = null;
                Assert.That(context.Controller.TryBuild(context.Plot), Is.EqualTo(TowerBuildResult.MissingPrefab));

                context.Config.TowerPrefab = context.TowerPrefab;
                context.Controller.SetInventory(null);
                Assert.That(context.Controller.TryBuild(context.Plot), Is.EqualTo(TowerBuildResult.MissingInventory));

                Assert.That(context.Inventory.Gold, Is.EqualTo(75), "Invalid builds should not spend gold.");
                Assert.IsFalse(context.Plot.IsOccupied, "Invalid builds should not occupy the plot.");
                Assert.That(context.TowerParent.childCount, Is.EqualTo(0), "Invalid builds should not spawn towers.");
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void TryBuild_WithInsufficientGold_RejectsWithoutSpawning()
        {
            var context = CreateContext(startingGold: 25, buildCost: 50);

            try
            {
                var result = context.Controller.TryBuild(context.Plot);

                Assert.That(result, Is.EqualTo(TowerBuildResult.InsufficientGold));
                Assert.That(context.Inventory.Gold, Is.EqualTo(25), "Insufficient gold should not change inventory.");
                Assert.IsFalse(context.Plot.IsOccupied, "Insufficient gold should not occupy the plot.");
                Assert.That(context.TowerParent.childCount, Is.EqualTo(0), "Insufficient gold should not spawn towers.");
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void TryBuild_RaisesSuccessAndDeniedFeedback_ForMenuFlashReuse()
        {
            var context = CreateContext(startingGold: 75, buildCost: 50);
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            context.Controller.SetFeedbackChannel(channel);

            FeedbackCueType? lastType = null;
            int raisedCount = 0;
            channel.Raised += cue =>
            {
                lastType = cue.Type;
                raisedCount++;
            };

            try
            {
                Assert.That(context.Controller.TryBuild(context.Plot), Is.EqualTo(TowerBuildResult.Success));
                Assert.That(lastType, Is.EqualTo(FeedbackCueType.UpgradeSuccess));

                Assert.That(context.Controller.TryBuild(context.Plot), Is.EqualTo(TowerBuildResult.Occupied));
                Assert.That(lastType, Is.EqualTo(FeedbackCueType.UpgradeDenied));
                Assert.That(raisedCount, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(channel);
                context.Destroy();
            }
        }

        [Test]
        public void TowerBuildConfig_ClampsNumericAuthoringValues()
        {
            var config = ScriptableObject.CreateInstance<TowerBuildConfig>();

            try
            {
                config.BuildCost = -1;
                config.BaseMaxHealth = -1;
                config.BaseDamage = -1;
                config.BaseUpgradeCost = -1;

                Assert.That(config.BuildCost, Is.EqualTo(0));
                Assert.That(config.BaseMaxHealth, Is.EqualTo(0));
                Assert.That(config.BaseDamage, Is.EqualTo(0));
                Assert.That(config.BaseUpgradeCost, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        private static BuildContext CreateContext(int startingGold, int buildCost)
        {
            var root = new GameObject("TowerBuildController");
            var controller = root.AddComponent<TowerBuildController>();
            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.PreWave);
            controller.SetPhaseTracker(phase);

            var inventory = new InventoryState();
            inventory.AddGold(startingGold);
            controller.SetInventory(inventory);

            var config = ScriptableObject.CreateInstance<TowerBuildConfig>();
            config.BuildCost = buildCost;

            var towerPrefab = new GameObject("ArcherTower");
            towerPrefab.AddComponent<TowerRuntime>();
            config.TowerPrefab = towerPrefab;
            controller.Config = config;

            var towerParent = new GameObject("TowerParent").transform;
            controller.TowerParent = towerParent;

            var plotRoot = new GameObject("TowerPlot");
            var anchor = new GameObject("Anchor").transform;
            anchor.SetParent(plotRoot.transform, false);
            var plot = plotRoot.AddComponent<TowerPlot>();
            plot.SetAnchor(anchor);

            return new BuildContext(root, controller, phase, inventory, config, towerPrefab, towerParent, plotRoot, plot, anchor);
        }

        private sealed class BuildContext
        {
            private readonly GameObject root;
            private readonly GameObject plotRoot;

            public BuildContext(
                GameObject root,
                TowerBuildController controller,
                WavePhaseTracker phase,
                InventoryState inventory,
                TowerBuildConfig config,
                GameObject towerPrefab,
                Transform towerParent,
                GameObject plotRoot,
                TowerPlot plot,
                Transform anchor)
            {
                this.root = root;
                Controller = controller;
                Phase = phase;
                Inventory = inventory;
                Config = config;
                TowerPrefab = towerPrefab;
                TowerParent = towerParent;
                this.plotRoot = plotRoot;
                Plot = plot;
                Anchor = anchor;
            }

            public TowerBuildController Controller { get; }
            public WavePhaseTracker Phase { get; }
            public InventoryState Inventory { get; }
            public TowerBuildConfig Config { get; }
            public GameObject TowerPrefab { get; }
            public Transform TowerParent { get; }
            public TowerPlot Plot { get; }
            public Transform Anchor { get; }

            public void Destroy()
            {
                if (Config != null)
                {
                    Object.DestroyImmediate(Config);
                }

                if (TowerPrefab != null)
                {
                    Object.DestroyImmediate(TowerPrefab);
                }

                if (TowerParent != null)
                {
                    Object.DestroyImmediate(TowerParent.gameObject);
                }

                if (plotRoot != null)
                {
                    Object.DestroyImmediate(plotRoot);
                }

                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }
    }
}

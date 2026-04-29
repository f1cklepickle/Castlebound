using System.Linq;
using System.Reflection;
using Castlebound.Gameplay.Barrier;
using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.Tower;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Castlebound.Tests.UI
{
    public class UpgradeMenuListViewTowerRowsTests
    {
        [Test]
        public void Refresh_RendersBarrierUpgradeRowAndTowerPlotChildRows()
        {
            var context = CreateContext();

            try
            {
                context.View.Refresh();

                AssertTextExists(context.ContentRoot, "North Barrier");
                AssertTextExists(context.ContentRoot, "Tier 0 | HP 10/10 | 20 Gold");
                AssertTextExists(context.ContentRoot, "- Left Plot");
                AssertTextExists(context.ContentRoot, "- Right Plot");
                AssertTextExists(context.ContentRoot, "ArcherTower | HP 10 | DMG 3 | Build 50 Gold");
                Assert.That(context.ContentRoot.GetComponentsInChildren<Button>(true).Length, Is.EqualTo(3));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void BuildButton_InvokesTowerBuildController_AndAssignsPlotOccupant()
        {
            var context = CreateContext();

            try
            {
                context.View.Refresh();
                var buildButton = FindButtonByLabel(context.ContentRoot, "Build");

                buildButton.onClick.Invoke();

                Assert.That(context.Inventory.Gold, Is.EqualTo(50), "Build button should delegate spending to TowerBuildController.");
                Assert.IsTrue(context.LeftPlot.IsOccupied, "Build button should build into the selected plot.");
                Assert.NotNull(context.LeftPlot.OccupantInstance.GetComponent<TowerRuntime>());
                Assert.That(context.TowerParent.childCount, Is.EqualTo(1));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void Refresh_RendersOccupiedPlotAsUnavailableWithoutHidingBarrier()
        {
            var context = CreateContext();
            var existingTower = new GameObject("ArcherTower_Instance");
            existingTower.AddComponent<TowerRuntime>();
            context.LeftPlot.TryAssignOccupant(existingTower);

            try
            {
                context.View.Refresh();

                AssertTextExists(context.ContentRoot, "North Barrier");
                AssertTextExists(context.ContentRoot, "ArcherTower | HP 10/10 | DMG 3 | Upg 75 Gold");

                var occupiedButton = FindButtonByLabel(context.ContentRoot, "Occupied");
                Assert.IsFalse(occupiedButton.interactable, "Occupied tower plots should render as unavailable.");

                var buildButtons = FindButtonsByLabel(context.ContentRoot, "Build");
                Assert.That(buildButtons.Length, Is.EqualTo(1), "Only the remaining empty plot should expose a build action.");
            }
            finally
            {
                Object.DestroyImmediate(existingTower);
                context.Destroy();
            }
        }

        [Test]
        public void BarrierUpgradeButton_StillInvokesExistingUpgradeFlow()
        {
            var context = CreateContext();

            try
            {
                context.View.Refresh();
                var upgradeButton = FindButtonByLabel(context.ContentRoot, "Upgrade");

                upgradeButton.onClick.Invoke();

                Assert.That(context.BarrierController.GetCurrentTier(), Is.EqualTo(1));
                Assert.That(context.Inventory.Gold, Is.EqualTo(80), "Barrier upgrade should keep using the existing barrier controller path.");
                Assert.IsFalse(context.LeftPlot.IsOccupied, "Barrier upgrade should not build towers.");
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void MainPrototype_WiresTowerBuildController_ForUpgradeMenuRows()
        {
            Scene scene = default;

            try
            {
                scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainPrototype.unity", OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var view = FindInScene<UpgradeMenuListView>(scene);
                Assert.NotNull(view, "MainPrototype must include UpgradeMenuListView.");

                var controller = FindInScene<TowerBuildController>(scene);
                Assert.NotNull(controller, "MainPrototype must include TowerBuildController so tower child rows can render.");
                Assert.NotNull(controller.Config, "TowerBuildController must reference TowerBuildConfig.");
                Assert.NotNull(controller.Config.TowerPrefab, "TowerBuildConfig must reference the tower prefab.");

                var panel = FindTransform(scene, "UpgradeMenuPanel") as RectTransform;
                Assert.NotNull(panel, "MainPrototype must include UpgradeMenuPanel.");
                Assert.That(panel.sizeDelta, Is.EqualTo(new Vector2(980f, 620f)), "UpgradeMenuPanel should be large enough for barrier-owned tower rows.");

                var field = typeof(UpgradeMenuListView).GetField("towerBuildController", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.NotNull(field, "UpgradeMenuListView should serialize its TowerBuildController reference.");
                Assert.AreSame(controller, field.GetValue(view), "UpgradeMenuListView should be wired to the scene TowerBuildController.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static TestContext CreateContext()
        {
            var inventory = new InventoryState();
            inventory.AddGold(100);
            var phase = new WavePhaseTracker();
            phase.SetPhase(WavePhase.PreWave);

            var contentRoot = new GameObject("UpgradeMenuContent", typeof(RectTransform)).GetComponent<RectTransform>();

            var viewRoot = new GameObject("UpgradeMenuListView");
            var view = viewRoot.AddComponent<UpgradeMenuListView>();
            view.SetContentRoot(contentRoot);

            var barrier = new GameObject("North Barrier");
            var health = barrier.AddComponent<BarrierHealth>();
            health.MaxHealth = 10;
            health.CurrentHealth = 10;

            var barrierConfig = ScriptableObject.CreateInstance<BarrierUpgradeConfig>();
            barrierConfig.BaseMaxHealth = 10;
            barrierConfig.BaseCost = 20;
            barrierConfig.CostPerTier = 10;

            var barrierController = barrier.AddComponent<BarrierUpgradeController>();
            barrierController.Config = barrierConfig;
            barrierController.SetInventory(inventory);
            barrierController.SetPhaseTracker(phase);

            var leftPlot = CreatePlot("LeftPlot", barrier.transform, new Vector3(-1f, 0f, 0f));
            var rightPlot = CreatePlot("RightPlot", barrier.transform, new Vector3(1f, 0f, 0f));
            var collection = barrier.AddComponent<BarrierTowerPlotCollection>();
            collection.SetPlots(new[] { leftPlot, rightPlot });

            var towerRoot = new GameObject("TowerBuildController");
            var towerController = towerRoot.AddComponent<TowerBuildController>();
            towerController.SetInventory(inventory);
            towerController.SetPhaseTracker(phase);

            var towerConfig = ScriptableObject.CreateInstance<TowerBuildConfig>();
            towerConfig.BuildCost = 50;
            towerConfig.BaseMaxHealth = 10;
            towerConfig.BaseDamage = 3;
            towerConfig.BaseUpgradeCost = 75;

            var towerPrefab = new GameObject("ArcherTower");
            towerPrefab.AddComponent<TowerRuntime>();
            towerConfig.TowerPrefab = towerPrefab;
            towerController.Config = towerConfig;

            var towerParent = new GameObject("TowerParent").transform;
            towerController.TowerParent = towerParent;
            view.SetTowerBuildController(towerController);

            return new TestContext(
                viewRoot,
                contentRoot.gameObject,
                barrier,
                barrierConfig,
                barrierController,
                leftPlot,
                rightPlot,
                towerRoot,
                towerConfig,
                towerPrefab,
                towerParent,
                inventory);
        }

        private static TowerPlot CreatePlot(string name, Transform parent, Vector3 position)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            return root.AddComponent<TowerPlot>();
        }

        private static void AssertTextExists(Transform root, string expected)
        {
            var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            Assert.IsTrue(texts.Any(text => text.text == expected), $"Expected UI text '{expected}' was not found.");
        }

        private static Button FindButtonByLabel(Transform root, string label)
        {
            var buttons = FindButtonsByLabel(root, label);
            Assert.That(buttons.Length, Is.GreaterThan(0), $"Expected button '{label}' was not found.");
            return buttons[0];
        }

        private static Button[] FindButtonsByLabel(Transform root, string label)
        {
            return root.GetComponentsInChildren<Button>(true)
                .Where(button =>
                {
                    var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
                    return text != null && text.text == label;
                })
                .ToArray();
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static Transform FindTransform(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == name)
                    {
                        return transform;
                    }
                }
            }

            return null;
        }

        private sealed class TestContext
        {
            private readonly GameObject viewRoot;
            private readonly GameObject contentRoot;
            private readonly GameObject barrier;
            private readonly BarrierUpgradeConfig barrierConfig;
            private readonly GameObject towerRoot;
            private readonly TowerBuildConfig towerConfig;
            private readonly GameObject towerPrefab;

            public TestContext(
                GameObject viewRoot,
                GameObject contentRoot,
                GameObject barrier,
                BarrierUpgradeConfig barrierConfig,
                BarrierUpgradeController barrierController,
                TowerPlot leftPlot,
                TowerPlot rightPlot,
                GameObject towerRoot,
                TowerBuildConfig towerConfig,
                GameObject towerPrefab,
                Transform towerParent,
                InventoryState inventory)
            {
                this.viewRoot = viewRoot;
                this.contentRoot = contentRoot;
                this.barrier = barrier;
                this.barrierConfig = barrierConfig;
                BarrierController = barrierController;
                LeftPlot = leftPlot;
                RightPlot = rightPlot;
                this.towerRoot = towerRoot;
                this.towerConfig = towerConfig;
                this.towerPrefab = towerPrefab;
                TowerParent = towerParent;
                Inventory = inventory;
            }

            public UpgradeMenuListView View => viewRoot.GetComponent<UpgradeMenuListView>();
            public Transform ContentRoot => contentRoot.transform;
            public BarrierUpgradeController BarrierController { get; }
            public TowerPlot LeftPlot { get; }
            public TowerPlot RightPlot { get; }
            public Transform TowerParent { get; }
            public InventoryState Inventory { get; }

            public void Destroy()
            {
                if (TowerParent != null)
                {
                    Object.DestroyImmediate(TowerParent.gameObject);
                }

                if (towerPrefab != null)
                {
                    Object.DestroyImmediate(towerPrefab);
                }

                if (towerConfig != null)
                {
                    Object.DestroyImmediate(towerConfig);
                }

                if (towerRoot != null)
                {
                    Object.DestroyImmediate(towerRoot);
                }

                if (barrierConfig != null)
                {
                    Object.DestroyImmediate(barrierConfig);
                }

                if (barrier != null)
                {
                    Object.DestroyImmediate(barrier);
                }

                if (contentRoot != null)
                {
                    Object.DestroyImmediate(contentRoot);
                }

                if (viewRoot != null)
                {
                    Object.DestroyImmediate(viewRoot);
                }
            }
        }
    }
}

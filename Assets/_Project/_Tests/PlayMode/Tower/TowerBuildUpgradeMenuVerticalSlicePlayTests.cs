using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.Tower;
using Castlebound.Gameplay.UI;
using NUnit.Framework;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Castlebound.Tests.PlayMode.Tower
{
    public class TowerBuildUpgradeMenuVerticalSlicePlayTests
    {
        private const string MainPrototypeSceneName = "MainPrototype";

        [UnityTest]
        public IEnumerator MainPrototype_UpgradeMenu_BuildsTowerOnBarrierPlot_AndBlocksRepurchase()
        {
            yield return LoadMainPrototype();

            var menu = FindInActiveScene<UpgradeMenuController>();
            var listView = FindInActiveScene<UpgradeMenuListView>();
            var buildController = FindInActiveScene<TowerBuildController>();
            var inventorySource = FindInActiveScene<InventoryStateComponent>();
            Assert.NotNull(menu, "Expected MainPrototype to include UpgradeMenuController.");
            Assert.NotNull(listView, "Expected MainPrototype to include UpgradeMenuListView.");
            Assert.NotNull(buildController, "Expected MainPrototype to include TowerBuildController.");
            Assert.NotNull(buildController.Config, "TowerBuildController must have a build config.");
            Assert.NotNull(buildController.Config.TowerPrefab, "TowerBuildConfig must reference the real tower prefab.");
            Assert.NotNull(inventorySource, "Expected MainPrototype to expose an inventory source for upgrade purchases.");

            var tracker = new WavePhaseTracker();
            menu.SetPhaseTracker(tracker);
            buildController.SetPhaseTracker(tracker);
            buildController.SetInventory(inventorySource.State);
            listView.SetTowerBuildController(buildController);

            int startingGold = inventorySource.State.Gold;
            inventorySource.State.AddGold(buildController.Config.BuildCost * 2);
            int goldBeforeBuild = inventorySource.State.Gold;
            int towerCountBeforeBuild = Object.FindObjectsOfType<TowerRuntime>().Length;

            var plots = FindPlots();
            Assert.That(plots.Count, Is.GreaterThan(0), "Expected generated barriers to expose tower plots.");
            Assert.That(plots.Any(plot => !plot.IsOccupied), Is.True, "Expected at least one empty tower plot before building.");
            menu.ToggleMenu();
            yield return null;

            Assert.IsTrue(menu.IsMenuOpen, "Upgrade menu should open in pre-wave.");
            var buildButton = FindActiveButtonWithLabel("Build");
            Assert.NotNull(buildButton, "Expected the upgrade menu to expose a Build button for an empty tower plot.");
            Assert.IsTrue(buildButton.interactable, "Empty tower plot Build button should be interactable.");
            Assert.That(buildButton.onClick.GetPersistentEventCount(), Is.EqualTo(0), "Runtime-created build buttons should use non-persistent listeners.");

            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = new GameObject("TestEventSystem").AddComponent<EventSystem>();
            }

            ExecuteEvents.Execute(buildButton.gameObject, new PointerEventData(eventSystem), ExecuteEvents.pointerClickHandler);
            yield return null;
            yield return new WaitForSeconds(0.2f);

            var occupiedPlots = FindPlots().Where(plot => plot.IsOccupied).ToList();
            if (occupiedPlots.Count != 1)
            {
                var directResult = buildController.TryBuild(plots.First(plot => !plot.IsOccupied));
                Assert.Fail($"Expected one occupied tower plot after menu build click, but found {occupiedPlots.Count}. Direct build result on an empty plot was {directResult}.");
            }

            var occupiedPlot = occupiedPlots[0];
            Assert.NotNull(occupiedPlot.OccupantInstance, "Built plot should reference the spawned tower instance.");
            Assert.NotNull(occupiedPlot.OccupantInstance.GetComponent<TowerRuntime>(), "Build should spawn the real Tower prefab runtime.");
            Assert.That(Object.FindObjectsOfType<TowerRuntime>().Length, Is.EqualTo(towerCountBeforeBuild + 1), "Build should add one tower runtime to the scene.");
            Assert.That(inventorySource.State.Gold, Is.EqualTo(goldBeforeBuild - buildController.Config.BuildCost), "Build should spend gold exactly once.");

            int goldAfterBuild = inventorySource.State.Gold;
            int towerCountAfterBuild = Object.FindObjectsOfType<TowerRuntime>().Length;

            var occupiedButton = FindActiveButtonWithLabel("Occupied");
            Assert.NotNull(occupiedButton, "Occupied plot should remain visible in the menu as a blocked row.");
            Assert.IsFalse(occupiedButton.interactable, "Occupied plot row should not allow repeat purchase.");

            var duplicateResult = buildController.TryBuild(occupiedPlot);
            yield return null;

            Assert.That(duplicateResult, Is.EqualTo(TowerBuildResult.Occupied), "Repeat build attempts on the same plot should be rejected.");
            Assert.That(inventorySource.State.Gold, Is.EqualTo(goldAfterBuild), "Rejected duplicate builds should not spend gold again.");
            Assert.That(Object.FindObjectsOfType<TowerRuntime>().Length, Is.EqualTo(towerCountAfterBuild), "Rejected duplicate builds should not spawn another tower.");

            if (startingGold == 0)
            {
                Assert.That(inventorySource.State.Gold, Is.EqualTo(buildController.Config.BuildCost), "Test setup should leave only the second grant after one successful purchase.");
            }
        }

        private static IEnumerator LoadMainPrototype()
        {
            var load = SceneManager.LoadSceneAsync(MainPrototypeSceneName, LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            yield return new WaitForFixedUpdate();
        }

        private static List<TowerPlot> FindPlots()
        {
            return Object.FindObjectsOfType<BarrierTowerPlotCollection>()
                .SelectMany(collection => collection.Plots)
                .Where(plot => plot != null)
                .ToList();
        }

        private static Button FindActiveButtonWithLabel(string label)
        {
            return Object.FindObjectsOfType<Button>()
                .Where(button => button.gameObject.activeInHierarchy && button.name == "BuildButton")
                .FirstOrDefault(button =>
                {
                    foreach (var component in button.GetComponentsInChildren<Component>())
                    {
                        if (component == null || component.GetType().Name != "TextMeshProUGUI")
                        {
                            continue;
                        }

                        var textProperty = component.GetType().GetProperty("text");
                        var text = textProperty != null ? textProperty.GetValue(component) as string : null;
                        if (text == label)
                        {
                            return true;
                        }
                    }

                    return false;
                });
        }

        private static T FindInActiveScene<T>() where T : Component
        {
            var scene = SceneManager.GetActiveScene();
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
    }
}

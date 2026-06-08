using System.IO;
using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.World.Placement;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Castlebound.Tests.World.Placement
{
    public class BearTrapPlacementPrototypeTests
    {
        private const string BearTrapDefinitionPath = "Assets/_Project/Placeables/Defense/Placeable_Defense_BearTrap.asset";
        private const string BearTrapPrefabPath = "Assets/_Project/Prefabs/BearTrap.prefab";
        private const string MainPrototypeScenePath = "Assets/_Project/Scenes/MainPrototype.unity";
        private const string PlacementControllerSourcePath = "Assets/_Project/Scripts/_Project.Gameplay/World/Placement/WorldPlaceablePlacementController.cs";

        [Test]
        public void BearTrapDefinition_IsDefenseOutsideGroundOneByOne()
        {
            var definition = AssetDatabase.LoadAssetAtPath<PlaceableObjectDefinition>(BearTrapDefinitionPath);

            Assert.NotNull(definition, "Bear trap placeable definition must exist.");
            Assert.IsTrue(definition.IsValid, "Bear trap definition should be fully authored.");
            Assert.That(definition.Id, Is.EqualTo("defense.bear_trap"));
            Assert.That(definition.DisplayName, Is.EqualTo("Bear Trap"));
            Assert.That(definition.Category, Is.EqualTo(PlaceableObjectCategory.Defense));
            Assert.That(definition.PlacementSurface, Is.EqualTo(PlaceablePlacementSurface.OutsideGround));
            Assert.That(definition.Footprint, Is.EqualTo(GridFootprint.OneByOne));
        }

        [Test]
        public void BearTrapPlacementRules_AcceptsOutsideGroundAndRejectsOccupiedOrWrongSurface()
        {
            var definition = CreateBearTrapDefinition();
            var occupancy = new CastleOccupancyMap();
            var position = new Vector2(2f, -4f);

            try
            {
                Assert.IsTrue(WorldPlaceablePlacementRules.CanPlaceAt(
                    definition,
                    position,
                    PlaceablePlacementSurface.OutsideGround,
                    occupancy));

                Assert.IsFalse(WorldPlaceablePlacementRules.CanPlaceAt(
                    definition,
                    position,
                    PlaceablePlacementSurface.CastleWall,
                    occupancy));

                occupancy.Occupy(position, definition.Footprint);

                Assert.IsFalse(WorldPlaceablePlacementRules.CanPlaceAt(
                    definition,
                    position,
                    PlaceablePlacementSurface.OutsideGround,
                    occupancy));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void SnapToGrid_RoundsPointerWorldPositionToOneByOneGrid()
        {
            var snapped = WorldPlaceablePlacementRules.SnapToGrid(new Vector2(2.49f, -4.51f));

            Assert.That(snapped, Is.EqualTo(new Vector2(2f, -5f)));
        }

        [Test]
        public void BearTrapPrefab_UsesReplaceableSpriteOrAnimatorVisualContract()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BearTrapPrefabPath);

            Assert.NotNull(prefab, "Bear trap prefab must exist.");
            Assert.NotNull(prefab.GetComponentInChildren<SpriteRenderer>(true), "Bear trap prefab should expose a SpriteRenderer for placeholder art.");

            var visualState = prefab.GetComponent<BearTrapVisualState>();
            Assert.NotNull(visualState, "Bear trap prefab should include BearTrapVisualState so closed art or animation can be swapped later.");
            Assert.NotNull(visualState.OpenSprite, "Bear trap visual state should reference the current placeholder/open sprite.");
        }

        [Test]
        public void MainPrototype_HasBearTrapPlacementPrototypeController()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var controller = Object.FindObjectOfType<WorldPlaceablePlacementController>();

                Assert.NotNull(controller, "MainPrototype should include the prototype bear trap placement controller.");
                Assert.NotNull(controller.DefaultPlaceable, "MainPrototype should reference the bear trap definition.");
                Assert.That(controller.DefaultPlaceable.Id, Is.EqualTo("defense.bear_trap"));
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void PlacementController_DoesNotCreateStandaloneBearTrapButton()
        {
            var source = File.ReadAllText(PlacementControllerSourcePath);

            StringAssert.DoesNotContain("BearTrapPlaceButton", source);
            StringAssert.DoesNotContain("EnsureSelectButton", source);
        }

        private static PlaceableObjectDefinition CreateBearTrapDefinition()
        {
            var definition = ScriptableObject.CreateInstance<PlaceableObjectDefinition>();
            definition.Id = "defense.bear_trap";
            definition.DisplayName = "Bear Trap";
            definition.Category = PlaceableObjectCategory.Defense;
            definition.PlacementSurface = PlaceablePlacementSurface.OutsideGround;
            definition.SetFootprint(GridFootprint.OneByOne);
            definition.Prefab = new GameObject("BearTrapPrefab");
            return definition;
        }
    }
}

using System.IO;
using System.Reflection;
using Castlebound.Gameplay.Input;
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

                var serializedController = new SerializedObject(controller);
                Assert.NotNull(
                    serializedController.FindProperty("castleRegionCollider").objectReferenceValue,
                    "MainPrototype placement controller should reference the castle region collider for outside-ground validation.");
                Assert.That(
                    serializedController.FindProperty("placementBlockingLayers").intValue,
                    Is.EqualTo(LayerMask.GetMask("Walls", "Barriers")),
                    "MainPrototype placement controller should reject blocking wall and barrier colliders.");
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

        [Test]
        public void PlacementController_DoesNotBlockPreviewOnGlobalUiHitTests()
        {
            var source = File.ReadAllText(PlacementControllerSourcePath);

            StringAssert.DoesNotContain("IsPointerOverGameObject", source);
        }

        [Test]
        public void PlacementRules_ResolveCastleRegionAsCastleFloor()
        {
            var regionObject = new GameObject("CastleRegion");
            var region = regionObject.AddComponent<BoxCollider2D>();
            region.size = new Vector2(4f, 4f);

            try
            {
                Assert.That(
                    WorldPlaceablePlacementRules.ResolveAvailableSurface(
                        Vector2.zero,
                        GridFootprint.OneByOne,
                        PlaceablePlacementSurface.OutsideGround,
                        region),
                    Is.EqualTo(PlaceablePlacementSurface.CastleFloor));

                Assert.That(
                    WorldPlaceablePlacementRules.ResolveAvailableSurface(
                        new Vector2(5f, 0f),
                        GridFootprint.OneByOne,
                        PlaceablePlacementSurface.OutsideGround,
                        region),
                    Is.EqualTo(PlaceablePlacementSurface.OutsideGround));
            }
            finally
            {
                Object.DestroyImmediate(regionObject);
            }
        }

        [Test]
        public void ConfirmPlacement_PlacesLockedTrapAndKeepsPlacementActiveForNextTrap()
        {
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                Assert.IsTrue(controller.BeginPlacement(definition));

                controller.LockPlacementTarget(new Vector2(4f, -2f));
                Assert.IsTrue(controller.HasLockedTarget);
                Assert.IsTrue(controller.ConfirmPlacement());

                Assert.IsTrue(controller.IsPlacementActive, "Placement should remain active after one trap is placed.");
                Assert.IsFalse(controller.HasLockedTarget, "Placed trap should clear the lock so another target can be selected.");
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(1));

                controller.LockPlacementTarget(new Vector2(5f, -2f));
                Assert.IsTrue(controller.ConfirmPlacement());
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void ConfirmPlacement_RejectsOccupiedLockedCell()
        {
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                Assert.IsTrue(controller.BeginPlacement(definition));

                controller.LockPlacementTarget(new Vector2(4f, -2f));
                Assert.IsTrue(controller.ConfirmPlacement());

                controller.LockPlacementTarget(new Vector2(4f, -2f));
                Assert.IsFalse(controller.ConfirmPlacement());

                Assert.IsTrue(controller.IsPlacementActive);
                Assert.IsTrue(controller.HasLockedTarget, "Rejected placement should keep the locked cell for player correction.");
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void ConfirmPlacement_RejectsLockedCellInsideCastleRegion()
        {
            var regionObject = new GameObject("CastleRegion");
            var region = regionObject.AddComponent<BoxCollider2D>();
            region.size = new Vector2(4f, 4f);
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                SetPrivateField(controller, "castleRegionCollider", region);

                Assert.IsTrue(controller.BeginPlacement(definition));
                controller.LockPlacementTarget(Vector2.zero);

                Assert.IsFalse(controller.CanPlaceSelectedAt(Vector2.zero));
                Assert.IsFalse(controller.ConfirmPlacement());
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(0));

                controller.LockPlacementTarget(new Vector2(5f, 0f));

                Assert.IsTrue(controller.CanPlaceSelectedAt(new Vector2(5f, 0f)));
                Assert.IsTrue(controller.ConfirmPlacement());
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(regionObject);
            }
        }

        [Test]
        [TestCase("Walls")]
        [TestCase("Barriers")]
        public void ConfirmPlacement_RejectsLockedCellOnBlockingLayer(string layerName)
        {
            var blockerObject = new GameObject($"{layerName}Blocker");
            var blocker = blockerObject.AddComponent<BoxCollider2D>();
            blocker.size = Vector2.one;
            var layer = LayerMask.NameToLayer(layerName);
            Assert.That(layer, Is.Not.EqualTo(-1), $"Project must define the {layerName} layer.");
            blockerObject.layer = layer;
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                SetPrivateField(controller, "placementBlockingLayers", (LayerMask)LayerMask.GetMask("Walls", "Barriers"));

                Assert.IsTrue(controller.BeginPlacement(definition));
                controller.LockPlacementTarget(Vector2.zero);

                Assert.IsFalse(controller.CanPlaceSelectedAt(Vector2.zero));
                Assert.IsFalse(controller.ConfirmPlacement());
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(0));

                controller.LockPlacementTarget(new Vector2(3f, 0f));

                Assert.IsTrue(controller.CanPlaceSelectedAt(new Vector2(3f, 0f)));
                Assert.IsTrue(controller.ConfirmPlacement());
                Assert.That(CountPlacedTraps(controllerObject.transform), Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(blockerObject);
            }
        }

        [Test]
        public void CancelPlacement_ClearsSelectionAndInvokesCallback()
        {
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();
            var canceled = false;

            try
            {
                Assert.IsTrue(controller.BeginPlacement(definition, () => canceled = true));
                controller.LockPlacementTarget(new Vector2(4f, -2f));

                controller.CancelPlacement();

                Assert.IsFalse(controller.IsPlacementActive);
                Assert.IsFalse(controller.HasLockedTarget);
                Assert.IsTrue(canceled);
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
            }
        }

        [Test]
        public void CancelPlacement_ReleasesActiveTouchAttackState()
        {
            var aimObject = new GameObject("TouchAimAttackZone");
            var aimZone = aimObject.AddComponent<TouchAimAttackZone>();
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                aimZone.AttackDeadzone = 10f;
                aimZone.SimulatePointerDown(Vector2.zero);
                aimZone.SimulateAimInput(new Vector2(100f, 0f));
                Assert.IsTrue(aimZone.IsFiring, "Precondition: touch attack should be active.");

                Assert.IsTrue(controller.BeginPlacement(definition));
                controller.CancelPlacement();

                Assert.IsFalse(aimZone.IsFiring);
                Assert.That(aimZone.FacingDirection, Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(aimObject);
            }
        }

        [Test]
        public void CancelPlacement_ReleasesHeldPcFireState()
        {
            var fireObject = new GameObject("PlayerFireInputController");
            var fireInput = fireObject.AddComponent<PlayerFireInputController>();
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                fireInput.OnFirePressedStateChanged(true);
                Assert.IsTrue(fireInput.IsFireHeld, "Precondition: PC fire should be held.");

                Assert.IsTrue(controller.BeginPlacement(definition));
                controller.CancelPlacement();

                Assert.IsFalse(fireInput.IsFireHeld);
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(fireObject);
            }
        }

        [Test]
        public void CancelPlacement_ResetsActivePcAttackLoop()
        {
            var playerObject = new GameObject("Player");
            var fireInput = playerObject.AddComponent<PlayerFireInputController>();
            var attackLoop = playerObject.AddComponent<PlayerAttackLoop>();
            playerObject.AddComponent<PlayerController>();
            var controllerObject = new GameObject("PlacementController");
            var controller = controllerObject.AddComponent<WorldPlaceablePlacementController>();
            var definition = CreateBearTrapDefinition();

            try
            {
                fireInput.OnFirePressedStateChanged(true);
                attackLoop.Tick(0f, 1.5f, true);
                Assert.IsTrue(attackLoop.IsSwingActive, "Precondition: PC fire click should have started an attack swing.");

                Assert.IsTrue(controller.BeginPlacement(definition));
                controller.CancelPlacement();

                Assert.IsFalse(fireInput.IsFireHeld);
                Assert.IsFalse(attackLoop.IsSwingActive);
            }
            finally
            {
                Object.DestroyImmediate(definition.Prefab);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(playerObject);
            }
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

        private static int CountPlacedTraps(Transform root)
        {
            var count = 0;
            foreach (Transform child in root)
            {
                if (child.name == "Bear Trap")
                {
                    count++;
                }
            }

            return count;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, $"Expected private field '{fieldName}' to exist.");
            field.SetValue(target, value);
        }
    }
}

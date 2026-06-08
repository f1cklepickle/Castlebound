using Castlebound.Gameplay.Castle;
using Castlebound.Gameplay.World.Placement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.World.Placement
{
    public class PlaceableObjectDefinitionTests
    {
        private const string CastleBarrierGatePath = "Assets/_Project/Placeables/Castle/Placeable_Castle_BarrierGate.asset";
        private const string DefenseTowerPath = "Assets/_Project/Placeables/Defense/Placeable_Defense_Tower.asset";

        [Test]
        public void NewDefinition_IsInvalidUntilRequiredAuthoringFieldsAreAssigned()
        {
            var definition = ScriptableObject.CreateInstance<PlaceableObjectDefinition>();

            try
            {
                Assert.IsFalse(definition.IsValid);

                definition.Id = "castle.test_piece";
                definition.DisplayName = "Test Piece";
                definition.Prefab = new GameObject("TestPrefab");

                Assert.IsTrue(definition.IsValid);
            }
            finally
            {
                if (definition.Prefab != null)
                {
                    Object.DestroyImmediate(definition.Prefab);
                }

                Object.DestroyImmediate(definition);
            }
        }

        [TestCase(PlaceableObjectCategory.Castle)]
        [TestCase(PlaceableObjectCategory.Defense)]
        public void Category_SupportsCurrentCastleAndDefensePlaceableFamilies(PlaceableObjectCategory category)
        {
            var definition = ScriptableObject.CreateInstance<PlaceableObjectDefinition>();

            try
            {
                definition.Category = category;

                Assert.That(definition.Category, Is.EqualTo(category));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void SetFootprint_StoresExplicitGridFootprintDimensions()
        {
            var definition = ScriptableObject.CreateInstance<PlaceableObjectDefinition>();

            try
            {
                definition.SetFootprint(GridFootprint.ThreeByThree);

                Assert.That(definition.FootprintWidth, Is.EqualTo(3));
                Assert.That(definition.FootprintHeight, Is.EqualTo(3));
                Assert.That(definition.Footprint, Is.EqualTo(GridFootprint.ThreeByThree));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void InvalidFootprint_IsReportedWithoutCreatingAFootprint()
        {
            var definition = ScriptableObject.CreateInstance<PlaceableObjectDefinition>();

            try
            {
                definition.FootprintWidth = 0;
                definition.FootprintHeight = 3;

                Assert.IsFalse(definition.HasValidFootprint);
                Assert.IsFalse(definition.Footprint.IsValid);
                Assert.IsFalse(definition.IsValid);
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void ProjectAssets_AuthorCurrentCastleAndDefensePlaceablesAsThreeByThree()
        {
            var castle = AssetDatabase.LoadAssetAtPath<PlaceableObjectDefinition>(CastleBarrierGatePath);
            var defense = AssetDatabase.LoadAssetAtPath<PlaceableObjectDefinition>(DefenseTowerPath);

            Assert.NotNull(castle, "Castle barrier gate placeable definition must exist.");
            Assert.NotNull(defense, "Defense tower placeable definition must exist.");

            AssertPlaceable(castle, "castle.barrier_gate", "Barrier Gate", PlaceableObjectCategory.Castle);
            AssertPlaceable(defense, "defense.tower", "Tower", PlaceableObjectCategory.Defense);
        }

        private static void AssertPlaceable(
            PlaceableObjectDefinition definition,
            string expectedId,
            string expectedDisplayName,
            PlaceableObjectCategory expectedCategory)
        {
            Assert.IsTrue(definition.IsValid, $"{expectedId} should be fully authored.");
            Assert.That(definition.Id, Is.EqualTo(expectedId));
            Assert.That(definition.DisplayName, Is.EqualTo(expectedDisplayName));
            Assert.That(definition.Category, Is.EqualTo(expectedCategory));
            Assert.That(definition.Footprint, Is.EqualTo(GridFootprint.ThreeByThree));
            Assert.NotNull(definition.Prefab, $"{expectedId} must reference a prefab.");
        }
    }
}

using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemySurroundPrefabContractTests
    {
        [TestCase("Assets/_Project/Prefabs/Enemy.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy_Lurker.prefab")]
        public void CurrentMeleePrefab_DefinesSurroundEligibility(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.NotNull(prefab, $"Expected melee prefab at {prefabPath}.");
            Assert.NotNull(prefab.GetComponent<EnemySurroundEligibility>(),
                $"Melee prefab {prefabPath} must opt into player-surround calculations.");
            Assert.NotNull(prefab.GetComponent<EnemyApproachSpread>(),
                $"Melee prefab {prefabPath} must define approach spreading.");
        }

        [TestCase("Assets/_Project/Prefabs/Enemy.prefab")]
        [TestCase("Assets/_Project/Prefabs/Enemy_Lurker.prefab")]
        public void CurrentMeleePrefab_DefinesExtractedResponsibilities(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.NotNull(prefab, $"Expected melee prefab at {prefabPath}.");

            var controller = prefab.GetComponent<EnemyController2D>();
            var targeting = prefab.GetComponent<EnemyTargeting>();
            var locomotion = prefab.GetComponent<EnemyLocomotion>();

            Assert.NotNull(controller, $"Melee prefab {prefabPath} must define its controller.");
            Assert.NotNull(targeting, $"Melee prefab {prefabPath} must define targeting.");
            Assert.NotNull(locomotion, $"Melee prefab {prefabPath} must define locomotion.");

            var controllerData = new SerializedObject(controller);
            Assert.That(
                controllerData.FindProperty("targeting").objectReferenceValue,
                Is.EqualTo(targeting));
            Assert.That(
                controllerData.FindProperty("locomotion").objectReferenceValue,
                Is.EqualTo(locomotion));

            var targetingData = new SerializedObject(targeting);
            Assert.That(
                targetingData.FindProperty("passThroughRadius").floatValue,
                Is.EqualTo(2f).Within(0.001f),
                $"Melee prefab {prefabPath} must retain its authored pass-through radius.");
        }
    }
}

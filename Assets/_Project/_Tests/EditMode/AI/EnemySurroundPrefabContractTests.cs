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
            var facing = prefab.GetComponent<EnemyFacing>();
            var engagement = prefab.GetComponent<EnemyEngagement>();
            var attack = prefab.GetComponent<EnemyAttack>();

            Assert.NotNull(controller, $"Melee prefab {prefabPath} must define its controller.");
            Assert.NotNull(targeting, $"Melee prefab {prefabPath} must define targeting.");
            Assert.NotNull(locomotion, $"Melee prefab {prefabPath} must define locomotion.");
            Assert.NotNull(facing, $"Melee prefab {prefabPath} must define facing.");
            Assert.NotNull(engagement, $"Melee prefab {prefabPath} must define engagement tuning.");
            Assert.NotNull(attack, $"Melee prefab {prefabPath} must define its attack.");

            var controllerData = new SerializedObject(controller);
            Assert.That(
                controllerData.FindProperty("targeting").objectReferenceValue,
                Is.EqualTo(targeting));
            Assert.That(
                controllerData.FindProperty("locomotion").objectReferenceValue,
                Is.EqualTo(locomotion));
            Assert.That(
                controllerData.FindProperty("facing").objectReferenceValue,
                Is.EqualTo(facing));
            Assert.That(
                controllerData.FindProperty("engagement").objectReferenceValue,
                Is.EqualTo(engagement));
            Assert.IsNull(controllerData.FindProperty("holdRadius"),
                $"Melee prefab {prefabPath} must not retain controller-owned engagement tuning.");
            Assert.IsNull(controllerData.FindProperty("releaseMargin"),
                $"Melee prefab {prefabPath} must not retain a duplicate release margin.");

            var targetingData = new SerializedObject(targeting);
            Assert.That(
                targetingData.FindProperty("passThroughRadius").floatValue,
                Is.EqualTo(2f).Within(0.001f),
                $"Melee prefab {prefabPath} must retain its authored pass-through radius.");

            var facingData = new SerializedObject(facing);
            Assert.That(
                facingData.FindProperty("visualTransform").objectReferenceValue,
                Is.EqualTo(prefab.transform.Find("Sprite")),
                $"Melee prefab {prefabPath} must rotate only its Sprite child.");
            Assert.That(
                facingData.FindProperty("initialAimDirection").vector2Value,
                Is.EqualTo(Vector2.down),
                $"Melee prefab {prefabPath} must use the sprite-authored south-facing direction.");
            Assert.That(
                facingData.FindProperty("turnSpeedDegreesPerSecond").floatValue,
                Is.EqualTo(120f).Within(0.001f),
                $"Melee prefab {prefabPath} must retain its authored turn speed.");

            var engagementData = new SerializedObject(engagement);
            Assert.That(
                engagementData.FindProperty("bodyCollider").objectReferenceValue,
                Is.EqualTo(prefab.GetComponent<Collider2D>()));
            Assert.That(
                engagementData.FindProperty("engagementDistance").floatValue,
                Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(
                engagementData.FindProperty("releaseMargin").floatValue,
                Is.EqualTo(0.25f).Within(0.001f));

            var attackData = new SerializedObject(attack);
            Assert.IsNull(attackData.FindProperty("attackRange"),
                $"Melee prefab {prefabPath} must use shared engagement reach.");
        }
    }
}

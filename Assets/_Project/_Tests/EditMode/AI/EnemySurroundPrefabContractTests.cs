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
        }
    }
}

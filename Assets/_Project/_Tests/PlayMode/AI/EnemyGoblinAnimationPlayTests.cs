using System.Collections;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.AI
{
    public class EnemyGoblinAnimationPlayTests
    {
        [UnityTest]
        public IEnumerator SpawnedGoblin_InitializesUnarmedWithAuthoredAnimationRig()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab");
            var enemy = Object.Instantiate(prefab);
            try
            {
                yield return null;

                var equipment = enemy.GetComponent<EnemyEquipment>();
                Assert.NotNull(equipment);
                Assert.That(equipment.ActiveEquipment.EquipmentId, Is.EqualTo("unarmed"));
                Assert.NotNull(enemy.GetComponent<Animator>().runtimeAnimatorController);
                Transform weapon = enemy.transform.Find("VisualRoot/HandSocket/Weapon");
                Assert.NotNull(weapon);
                Assert.IsFalse(weapon.GetComponent<SpriteRenderer>().enabled);
            }
            finally
            {
                Object.Destroy(enemy);
            }
        }
    }
}

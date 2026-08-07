using System.Reflection;
using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyStaggerPrefabContractTests
    {
        private const string MeleePrefabPath = "Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab";
        private const string RangedPrefabPath = "Assets/_Project/Prefabs/Enemy_Goblin_Ranged.prefab";
        private const string LurkerPrefabPath = "Assets/_Project/Prefabs/Enemy_Lurker.prefab";

        [Test]
        public void MeleeGoblin_AuthorsEligibleOneSecondStaggerWithExplicitReferences()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MeleePrefabPath);
            var receiver = prefab.GetComponent<EnemyStaggerReceiver>();

            Assert.NotNull(receiver);
            Assert.IsTrue(receiver.StaggerEligible);
            Assert.That(receiver.StaggerDurationSeconds, Is.EqualTo(1f));
            Assert.That(GetField<EnemyAttack>(receiver, "enemyAttack"), Is.SameAs(prefab.GetComponent<EnemyAttack>()));
            Assert.That(GetField<EnemyStaggerReceiver>(prefab.GetComponent<EnemyAttack>(), "staggerReceiver"),
                Is.SameAs(receiver));
            Assert.That(GetField<EnemyStaggerReceiver>(prefab.GetComponent<EnemyController2D>(), "staggerReceiver"),
                Is.SameAs(receiver));
            Assert.That(GetField<EnemyStaggerReceiver>(prefab.GetComponent<EnemyMeleeAttackDelivery>(), "staggerReceiver"),
                Is.SameAs(receiver));
            Assert.That(GetField<EnemyStaggerReceiver>(prefab.GetComponent<HitFlashListener>(), "staggerReceiver"),
                Is.SameAs(receiver));
        }

        [TestCase(RangedPrefabPath)]
        [TestCase(LurkerPrefabPath)]
        public void OtherEnemyPrefabs_DoNotOptIntoStagger(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Assert.NotNull(prefab);
            Assert.IsNull(prefab.GetComponent<EnemyStaggerReceiver>());
        }

        private static T GetField<T>(object instance, string fieldName)
        {
            FieldInfo field = instance.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Missing field {fieldName}.");
            return (T)field.GetValue(instance);
        }
    }
}

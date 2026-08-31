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
        public IEnumerator SustainedMovementRequests_AllowWalkAnimationToAdvance()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab");
            var enemy = Object.Instantiate(prefab);
            try
            {
                enemy.GetComponent<EnemyController2D>().enabled = false;
                enemy.GetComponent<EnemyAttack>().enabled = false;
                var presenter = enemy.GetComponent<EnemyAnimationPresenter>();
                var animator = enemy.GetComponent<Animator>();
                presenter.InitializePresentation();
                presenter.SetMovementRequested(true);

                for (int i = 0; i < 12; i++)
                {
                    yield return new WaitForFixedUpdate();
                    presenter.SetMovementRequested(true);
                }
                yield return null;

                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                Assert.That(state.IsName("Walk"), Is.True);
                Assert.That(state.normalizedTime, Is.GreaterThan(0.1f),
                    "Sustained moving requests must allow runtime Walk playback to advance.");
            }
            finally
            {
                Object.Destroy(enemy);
            }
        }

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

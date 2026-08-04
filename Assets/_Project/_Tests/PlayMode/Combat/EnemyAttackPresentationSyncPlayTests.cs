using System.Collections;
using System.Linq;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class EnemyAttackPresentationSyncPlayTests
    {
        [UnityTest]
        public IEnumerator ImpactBoundary_RendersSixthSprite_AtUnarmedAndClubRates()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Enemy_Goblin_Melee.prefab");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/_Project/Art/Goblin_Assets/Goblin_Attack.anim");
            var spriteBinding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                .Single(binding =>
                    binding.path == "VisualRoot/Sprite" &&
                    binding.propertyName == "m_Sprite");
            Sprite expectedImpactSprite = AnimationUtility
                .GetObjectReferenceCurve(clip, spriteBinding)[6]
                .value as Sprite;
            float baseRate = EnemyAttack.CalculateBaseAttackRate(0.3f, 0.8f);
            float[] rates = { baseRate, baseRate * 1.45f };

            foreach (float rate in rates)
            {
                GameObject enemy = Object.Instantiate(prefab);
                try
                {
                    enemy.GetComponent<EnemyAttack>().enabled = false;
                    enemy.GetComponent<EnemyController2D>().enabled = false;
                    var presenter = enemy.GetComponent<EnemyAnimationPresenter>();
                    var animator = enemy.GetComponent<Animator>();
                    var renderer = enemy.transform.Find("VisualRoot/Sprite").GetComponent<SpriteRenderer>();
                    var clock = new AttackClock();
                    clock.Start(rate, new AttackPhaseProfile(0.3f, 0f, 0.8f));
                    presenter.PlayAttack(
                        clock.CurrentSwing.WindupDuration,
                        clock.CurrentSwing.Duration);

                    AttackClockStep step = clock.Advance(clock.CurrentSwing.WindupDuration);
                    presenter.ApplyAttackProgress(clock.NormalizedProgress);
                    animator.Update(0f);

                    Assert.IsTrue(step.ImpactOccurred);
                    Assert.That(renderer.sprite, Is.SameAs(expectedImpactSprite),
                        $"Rate {rate} should render zero-based frame 6 at impact.");
                }
                finally
                {
                    Object.Destroy(enemy);
                }

                yield return null;
            }
        }
    }
}

using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;

namespace Castlebound.Tests.Player
{
    public class AnimatorAttackTransitionContractTests
    {
        [Test]
        public void PlayerAttackTransition_UsesExitTimeWithShortBlend()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/_Project/Art/Knight_Assets/Player.controller");

            Assert.IsNotNull(controller, "Player animator controller asset was not found.");

            var layer = controller.layers[0];
            var states = layer.stateMachine.states;
            AnimatorState attackState = null;
            for (var i = 0; i < states.Length; i++)
            {
                if (states[i].state != null && states[i].state.name == "Knight_Attack")
                {
                    attackState = states[i].state;
                    break;
                }
            }

            Assert.IsNotNull(attackState, "Knight_Attack state is missing.");
            Assert.Greater(attackState.transitions.Length, 0,
                "Knight_Attack should have an exit transition.");

            var attackToIdle = attackState.transitions[0];

            Assert.IsTrue(attackToIdle.hasExitTime,
                "Attack->Idle should keep exit-time flow for this animator graph to avoid one-and-done attack states.");
            Assert.LessOrEqual(attackToIdle.duration, 0.1f,
                "Attack->Idle transition duration should be short to avoid hard-capping repeat attacks.");

            var clip = attackState.motion as AnimationClip;
            Assert.IsNotNull(clip, "Knight_Attack state must use an AnimationClip motion.");

            var disableEvent = AnimationUtility.GetAnimationEvents(clip)
                .FirstOrDefault(e => e.functionName == "DisableHitbox");
            Assert.IsNotNull(disableEvent,
                "Knight_Attack clip must include DisableHitbox event to close the hit window.");

            var disableNormalizedTime = disableEvent.time / clip.length;
            Assert.GreaterOrEqual(attackToIdle.exitTime, disableNormalizedTime,
                "Attack->Idle exitTime must not start before DisableHitbox event; otherwise hitbox damage can be skipped.");
        }
    }
}

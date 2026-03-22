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
        public void PlayerAttackTransition_UsesLoopActiveConditionWithShortBlend()
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

            Assert.IsFalse(attackToIdle.hasExitTime,
                "Loop-era attack should not rely on exit-time gate for Attack->Idle transitions.");
            Assert.LessOrEqual(attackToIdle.duration, 0.1f,
                "Attack->Idle transition duration should be short to avoid hard-capping repeat attacks.");

            Assert.IsTrue(
                attackToIdle.conditions.Any(c => c.parameter == "AttackLoopActive" && c.mode == AnimatorConditionMode.IfNot),
                "Attack->Idle should be driven by AttackLoopActive=false in loop-era presentation.");
        }
    }
}

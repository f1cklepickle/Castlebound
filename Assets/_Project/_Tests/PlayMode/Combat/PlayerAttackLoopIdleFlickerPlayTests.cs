using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerAttackLoopIdleFlickerPlayTests
    {
        [UnityTest]
        public IEnumerator HeldAttack_DoesNotPulseIdle_BetweenChainedSwings()
        {
            var root = new GameObject("PlayerAttackLoop");
            var loop = root.AddComponent<PlayerAttackLoop>();

            loop.Tick(0f, 5f, true);
            float firstSwing = loop.CurrentSwingDuration;

            loop.Tick(firstSwing + 0.001f, 5f, true);
            Assert.IsTrue(loop.IsSwingActive,
                "Loop should remain active through a chained swing boundary while held.");

            for (int i = 0; i < 5; i++)
            {
                loop.Tick(0.01f, 5f, true);
                Assert.IsTrue(loop.IsSwingActive,
                    "Held attack should not pulse to idle between chained swings.");
                yield return null;
            }

            Object.Destroy(root);
        }
    }
}

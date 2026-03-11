using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerAttackLoopIdleFlickerPlayTests
    {
        [UnityTest]
        public IEnumerator HeldAttack_DoesNotPulseIdle_WhenCooldownDeniesSingleTick()
        {
            var root = new GameObject("PlayerAttackLoop");
            var loop = root.AddComponent<PlayerAttackLoop>();

            bool denyThisTick = false;
            bool TryStartSwing()
            {
                if (denyThisTick)
                {
                    denyThisTick = false;
                    return false;
                }

                return true;
            }

            loop.Tick(0f, 5f, true, TryStartSwing, null);
            float firstSwing = loop.CurrentSwingDuration;

            // Deny exactly once at chain boundary.
            denyThisTick = true;
            loop.Tick(firstSwing + 0.001f, 5f, true, TryStartSwing, null);
            Assert.IsTrue(loop.IsSwingActive,
                "Loop should remain active during a one-tick cooldown denial while held.");

            for (int i = 0; i < 5; i++)
            {
                loop.Tick(0.01f, 5f, true, TryStartSwing, null);
                Assert.IsTrue(loop.IsSwingActive,
                    "Held attack should not pulse to idle between chained swings.");
                yield return null;
            }

            Object.Destroy(root);
        }
    }
}

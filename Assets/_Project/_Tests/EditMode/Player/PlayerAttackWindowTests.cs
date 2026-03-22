using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Player
{
    public class PlayerAttackWindowTests
    {
        private GameObject root;
        private PlayerAttackLoop loop;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PlayerAttackLoop");
            loop = root.AddComponent<PlayerAttackLoop>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void HitWindow_OpensDuringActivePhase()
        {
            loop.Tick(0f, 1f, true);

            loop.Tick(loop.CurrentWindupDuration * 0.9f, 1f, true);
            Assert.IsFalse(loop.IsHitWindowOpen, "Hit window should not open during windup.");

            loop.Tick(loop.CurrentWindupDuration * 0.2f, 1f, true);
            Assert.IsTrue(loop.IsHitWindowOpen, "Hit window should open when entering active phase.");
        }

        [Test]
        public void HitWindow_ClosesAtActivePhaseEnd()
        {
            loop.Tick(0f, 1f, true);
            loop.Tick(loop.CurrentWindupDuration + (loop.CurrentActiveDuration * 0.5f), 1f, true);
            Assert.IsTrue(loop.IsHitWindowOpen, "Hit window should be open mid-active phase.");

            loop.Tick(loop.CurrentActiveDuration, 1f, true);
            Assert.IsFalse(loop.IsHitWindowOpen, "Hit window should close after active phase ends.");
        }

        [Test]
        public void HitWindow_DoesNotOpenOutsideActivePhase()
        {
            loop.Tick(0f, 1f, false);
            Assert.IsFalse(loop.IsHitWindowOpen, "Hit window should be closed while idle.");

            loop.Tick(0f, 1f, true);
            loop.Tick(loop.CurrentWindupDuration + loop.CurrentActiveDuration + 0.001f, 1f, true);
            Assert.IsFalse(loop.IsHitWindowOpen, "Hit window should be closed in recovery.");
        }
    }
}

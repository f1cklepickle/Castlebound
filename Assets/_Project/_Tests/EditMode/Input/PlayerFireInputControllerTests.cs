using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Input
{
    public class PlayerFireInputControllerTests
    {
        private GameObject root;
        private PlayerFireInputController controller;
        private int attackAttempts;
        private bool isPressed;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PlayerFireInputController");
            controller = root.AddComponent<PlayerFireInputController>();
            controller.Configure(OnTryAttack, () => isPressed);
            attackAttempts = 0;
            isPressed = false;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void OnFirePressedStateChanged_True_SetsHeldState()
        {
            controller.OnFirePressedStateChanged(true);

            Assert.IsTrue(controller.IsFireHeld,
                "Held-fire state should be true after pressed-state update.");
        }

        [Test]
        public void OnFirePressedStateChanged_False_ClearsHeldState()
        {
            controller.OnFirePressedStateChanged(true);
            controller.OnFirePressedStateChanged(false);

            Assert.IsFalse(controller.IsFireHeld,
                "Held-fire state should be false after release-state update.");
        }

        [Test]
        public void Tick_WhenHeldAndStillPressed_AttemptsAttack()
        {
            isPressed = true;
            controller.OnFirePressedStateChanged(true);

            controller.Tick();

            Assert.AreEqual(1, attackAttempts,
                "Tick should attempt one attack while held and still pressed.");
        }

        [Test]
        public void Tick_WhenHeldButNotPressed_ClearsHeldWithoutAttackAttempt()
        {
            isPressed = false;
            controller.OnFirePressedStateChanged(true);

            controller.Tick();

            Assert.AreEqual(0, attackAttempts,
                "Tick should not attempt attacks after release fallback clears held state.");
            Assert.IsFalse(controller.IsFireHeld,
                "Held-fire state should be cleared if pressed fallback returns false.");
        }

        [Test]
        public void ClearHeldFire_StopsFurtherTickAttempts()
        {
            isPressed = true;
            controller.OnFirePressedStateChanged(true);
            controller.ClearHeldFire();

            controller.Tick();

            Assert.AreEqual(0, attackAttempts,
                "ClearHeldFire should stop auto-swing attempts.");
            Assert.IsFalse(controller.IsFireHeld,
                "Held-fire state should be false after explicit clear.");
        }

        private bool OnTryAttack()
        {
            attackAttempts++;
            return true;
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class PlayerHoldFireCadencePlayTests
    {
        [UnityTest]
        public IEnumerator HoldFire_AttacksRepeatedly_AndStopsAfterRelease()
        {
            var root = new GameObject("PlayerFireInputController");
            var controller = root.AddComponent<PlayerFireInputController>();

            var attempts = 0;
            var isPressed = true;

            controller.Configure(
                () =>
                {
                    attempts++;
                    return true;
                },
                () => isPressed);

            controller.OnFirePressedStateChanged(true);

            for (var i = 0; i < 5; i++)
            {
                controller.Tick();
                yield return null;
            }

            Assert.That(attempts, Is.GreaterThanOrEqualTo(3),
                "Held fire should attempt multiple attacks over multiple frames.");

            var beforeReleaseAttempts = attempts;
            isPressed = false;

            for (var i = 0; i < 3; i++)
            {
                controller.Tick();
                yield return null;
            }

            Assert.AreEqual(beforeReleaseAttempts, attempts,
                "Attack attempts should stop after release fallback detects no press state.");
            Assert.IsFalse(controller.IsFireHeld,
                "Held-fire state should clear after release fallback.");

            Object.Destroy(root);
        }
    }
}

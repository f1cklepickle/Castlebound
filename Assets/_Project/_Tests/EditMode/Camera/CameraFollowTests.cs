using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Camera
{
    public class CameraFollowTests
    {
        [Test]
        public void AutoAssignsTarget_WhenMissing()
        {
            var cameraGo = new GameObject("Main Camera");
            var follow = cameraGo.AddComponent<CameraFollow>();

            var player = new GameObject("Player");
            player.tag = "Player";

            Assert.IsNull(follow.target, "Precondition: target is not assigned.");

            follow.Tick();

            Assert.IsNotNull(follow.target, "Target should be auto-assigned from Player tag.");
            Assert.That(follow.target, Is.EqualTo(player.transform));

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cameraGo);
        }
    }
}

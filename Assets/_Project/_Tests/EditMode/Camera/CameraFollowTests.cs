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

            Assert.IsNull(follow.Target, "Precondition: target is not assigned.");

            follow.Tick();

            Assert.IsNotNull(follow.Target, "Target should be auto-assigned from Player tag.");
            Assert.That(follow.Target, Is.EqualTo(player.transform));

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cameraGo);
        }

        [Test]
        public void Tick_FollowsPlayerWithAuthoredOffset()
        {
            var cameraGo = new GameObject("Main Camera");
            var follow = cameraGo.AddComponent<CameraFollow>();
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(3f, -2f, 0f);

            follow.Tick();

            Assert.That(cameraGo.transform.position, Is.EqualTo(player.transform.position + follow.Offset));

            Object.DestroyImmediate(player);
            Object.DestroyImmediate(cameraGo);
        }

    }
}

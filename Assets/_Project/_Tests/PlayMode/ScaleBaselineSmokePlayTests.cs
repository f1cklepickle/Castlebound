using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Scale
{
    public class ScaleBaselineSmokePlayTests
    {
        [UnityTest]
        public IEnumerator MainPrototype_Loads_WithReadableCameraAndPlayer()
        {
            var load = SceneManager.LoadSceneAsync("MainPrototype", LoadSceneMode.Single);
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;

            var camera = Object.FindObjectOfType<Camera>();
            Assert.NotNull(camera, "Expected main camera in MainPrototype.");
            Assert.IsTrue(camera.orthographic, "Main camera should remain orthographic in scale baseline.");
            Assert.That(camera.orthographicSize, Is.GreaterThanOrEqualTo(9f).And.LessThanOrEqualTo(12f),
                "Camera size should remain in baseline readable range after scale migration.");

            var player = Object.FindObjectOfType<PlayerController>();
            Assert.NotNull(player, "Expected PlayerController in MainPrototype.");

            var playerSprites = player.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(playerSprites.Length, Is.GreaterThan(0), "Player should have visible sprite renderers after scale migration.");
        }
    }
}

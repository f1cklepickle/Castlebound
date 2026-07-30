using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.U2D;

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
            Assert.That(camera.orthographicSize, Is.EqualTo(8.4375f).Within(0.001f),
                "Pixel-perfect camera should preserve the authored 540-pixel vertical view.");

            var pixelPerfect = camera.GetComponent<PixelPerfectCamera>();
            Assert.NotNull(pixelPerfect, "Main camera should include PixelPerfectCamera at runtime.");
            Assert.That(pixelPerfect.assetsPPU, Is.EqualTo(32));
            Assert.That(pixelPerfect.refResolutionX, Is.EqualTo(960));
            Assert.That(pixelPerfect.refResolutionY, Is.EqualTo(540));

            var player = Object.FindObjectOfType<PlayerController>();
            Assert.NotNull(player, "Expected PlayerController in MainPrototype.");

            var playerSprites = player.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(playerSprites.Length, Is.GreaterThan(0), "Player should have visible sprite renderers after scale migration.");
        }
    }
}

using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;

namespace Castlebound.Tests.Scale
{
    public class CameraScaleBaselineTests
    {
        [Test]
        public void MainPrototype_MainCamera_UsesOrthographicBaseline()
        {
            UnityEngine.SceneManagement.Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainPrototype.unity", OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var camera = FindInScene<UnityEngine.Camera>(scene);
                Assert.NotNull(camera, "Expected a Camera in MainPrototype.");
                Assert.IsTrue(camera.orthographic, "Main camera must be orthographic for 2D scale baseline.");
                Assert.That(camera.orthographicSize, Is.EqualTo(8.4375f).Within(0.001f),
                    "Main camera should author the strict 540-pixel vertical view.");

                var pixelPerfect = camera.GetComponent<PixelPerfectCamera>();
                Assert.NotNull(pixelPerfect, "Main camera must own the pixel-perfect rendering contract.");
                Assert.That(pixelPerfect.assetsPPU, Is.EqualTo(32));
                Assert.That(pixelPerfect.refResolutionX, Is.EqualTo(960));
                Assert.That(pixelPerfect.refResolutionY, Is.EqualTo(540));
                Assert.IsTrue(pixelPerfect.upscaleRT);
                Assert.IsFalse(pixelPerfect.cropFrameX,
                    "Wider displays should reveal additional world instead of adding side borders.");
                Assert.IsFalse(pixelPerfect.cropFrameY,
                    "Taller landscape displays should reveal additional world instead of adding top/bottom borders.");
                Assert.IsFalse(pixelPerfect.stretchFill);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static T FindInScene<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<T>(true);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}

using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

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
                Assert.That(camera.orthographicSize, Is.GreaterThanOrEqualTo(3f).And.LessThanOrEqualTo(8f),
                    "Orthographic size should be in a readable baseline range for PPU=32.");
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

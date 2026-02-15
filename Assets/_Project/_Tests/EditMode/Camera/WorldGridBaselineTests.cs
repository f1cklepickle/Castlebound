using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Castlebound.Tests.Scale
{
    public class WorldGridBaselineTests
    {
        [Test]
        public void MainPrototype_HasWorldGrid_WithOneUnitCells()
        {
            UnityEngine.SceneManagement.Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainPrototype.unity", OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var grid = FindInScene<Grid>(scene);
                Assert.NotNull(grid, "MainPrototype must include a Grid as authoritative world reference.");
                Assert.That(grid.cellSize.x, Is.EqualTo(1f).Within(0.0001f), "Grid cell width must be 1 unit.");
                Assert.That(grid.cellSize.y, Is.EqualTo(1f).Within(0.0001f), "Grid cell height must be 1 unit.");
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

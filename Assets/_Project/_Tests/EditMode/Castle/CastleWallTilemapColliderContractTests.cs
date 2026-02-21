using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Castlebound.Tests.Castle
{
    public class CastleWallTilemapColliderContractTests
    {
        private const string MainPrototypeScenePath = "Assets/_Project/Scenes/MainPrototype.unity";
        private const string WallsTilemapName = "CastleWallsTilemap";
        private const string BarriersTilemapName = "CastleBarriersTilemap";

        [Test]
        public void MainPrototype_CastleWallsTilemap_HasSolidTilemapCollider()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var walls = FindByName<Tilemap>(scene, WallsTilemapName);
                Assert.NotNull(walls, $"Expected tilemap '{WallsTilemapName}' in MainPrototype.");

                var collider = walls.GetComponent<TilemapCollider2D>();
                Assert.NotNull(collider, "CastleWallsTilemap must include TilemapCollider2D.");
                Assert.IsFalse(collider.isTrigger, "CastleWallsTilemap collider must be solid (non-trigger).");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void MainPrototype_CastleWallsTilemap_UsesWallsLayer()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var walls = FindByName<Tilemap>(scene, WallsTilemapName);
                Assert.NotNull(walls, $"Expected tilemap '{WallsTilemapName}' in MainPrototype.");

                var wallsLayer = LayerMask.NameToLayer("Walls");
                Assert.That(wallsLayer, Is.Not.EqualTo(-1), "Project must define a 'Walls' layer.");
                Assert.That(walls.gameObject.layer, Is.EqualTo(wallsLayer), "CastleWallsTilemap must use the 'Walls' layer.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void MainPrototype_CastleBarriersTilemap_DoesNotUseBlockingCollider()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var barriers = FindByName<Tilemap>(scene, BarriersTilemapName);
                Assert.NotNull(barriers, $"Expected tilemap '{BarriersTilemapName}' in MainPrototype.");

                var collider = barriers.GetComponent<TilemapCollider2D>();
                if (collider == null)
                {
                    Assert.Pass();
                }

                var blocksMovement = collider.enabled && !collider.isTrigger;
                Assert.IsFalse(blocksMovement, "CastleBarriersTilemap must not block movement. Blocking comes from spawned barrier prefabs.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static T FindByName<T>(Scene scene, string name) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (tr.name != name)
                    {
                        continue;
                    }

                    var comp = tr.GetComponent<T>();
                    if (comp != null)
                    {
                        return comp;
                    }
                }
            }

            return null;
        }
    }
}

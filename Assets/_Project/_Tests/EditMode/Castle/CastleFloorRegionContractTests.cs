using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Castlebound.Tests.Castle
{
    public class CastleFloorRegionContractTests
    {
        private const string MainPrototypeScenePath = "Assets/_Project/Scenes/MainPrototype.unity";
        private const string FloorTilemapName = "CastleFloorTilemap";

        [Test]
        public void MainPrototype_CastleFloorTilemap_HasTriggerTilemapCollider()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var floor = FindByName<Tilemap>(scene, FloorTilemapName);
                Assert.NotNull(floor, $"Expected tilemap '{FloorTilemapName}' in MainPrototype.");

                var collider = floor.GetComponent<TilemapCollider2D>();
                Assert.NotNull(collider, "CastleFloorTilemap must include TilemapCollider2D.");
                Assert.IsTrue(collider.isTrigger, "Castle floor collider must be trigger-based for region detection.");
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
        public void MainPrototype_HasCastleRegionTracker_OnFloorDrivenRegionSource()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var floor = FindByName<Tilemap>(scene, FloorTilemapName);
                Assert.NotNull(floor, $"Expected tilemap '{FloorTilemapName}' in MainPrototype.");

                var tracker = Object.FindObjectOfType<CastleRegionTracker>();
                Assert.NotNull(tracker, "Expected CastleRegionTracker in MainPrototype.");

                var floorCollider = floor.GetComponent<TilemapCollider2D>();
                Assert.NotNull(floorCollider, "Expected floor tilemap collider for region source.");

                var trackerCollider = tracker.GetComponent<Collider2D>();
                Assert.NotNull(trackerCollider, "CastleRegionTracker object must have Collider2D for trigger events.");
                Assert.IsTrue(trackerCollider.isTrigger, "CastleRegionTracker collider must be trigger.");

                Assert.That(
                    trackerCollider.gameObject.name,
                    Is.EqualTo(FloorTilemapName),
                    "CastleRegionTracker should be on the floor-driven region source object.");
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

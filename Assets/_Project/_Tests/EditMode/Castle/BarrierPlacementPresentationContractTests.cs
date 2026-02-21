using System.Collections.Generic;
using System.Linq;
using Castlebound.Gameplay.Castle;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Castlebound.Tests.Castle
{
    public class BarrierPlacementPresentationContractTests
    {
        private const string MainPrototypeScenePath = "Assets/_Project/Scenes/MainPrototype.unity";

        [Test]
        public void MainPrototype_RebuildNow_GeneratedBarriers_AlignToBarrierMarkerTiles()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var tilemap = FindByName<Tilemap>(scene, "CastleBarriersTilemap");
                Assert.NotNull(tilemap, "Expected CastleBarriersTilemap in MainPrototype.");

                var bootstrap = tilemap.GetComponent<BarrierAssemblyBootstrap>();
                Assert.NotNull(bootstrap, "Expected BarrierAssemblyBootstrap on CastleBarriersTilemap.");
                bootstrap.RebuildNow();

                var markerCenters = CollectBarrierMarkerCenters(tilemap);
                Assert.That(markerCenters.Count, Is.GreaterThan(0), "Expected barrier marker tiles in CastleBarriersTilemap.");

                var generated = FindByName<GameObject>(scene, "GeneratedBarriers");
                Assert.NotNull(generated, "Expected GeneratedBarriers container after rebuild.");
                Assert.That(generated.transform.childCount, Is.GreaterThan(0), "Expected generated barriers after rebuild.");

                foreach (Transform barrier in generated.transform)
                {
                    var barrierPos = (Vector2)barrier.position;
                    var nearest = markerCenters.Min(center => Vector2.Distance(center, barrierPos));
                    Assert.That(
                        nearest,
                        Is.LessThanOrEqualTo(0.15f),
                        $"Generated barrier '{barrier.name}' should align with a barrier marker tile center.");
                }
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
        public void MainPrototype_RebuildNow_HidesBarrierMarkerTilemapRenderer()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var tilemap = FindByName<Tilemap>(scene, "CastleBarriersTilemap");
                Assert.NotNull(tilemap, "Expected CastleBarriersTilemap in MainPrototype.");

                var bootstrap = tilemap.GetComponent<BarrierAssemblyBootstrap>();
                Assert.NotNull(bootstrap, "Expected BarrierAssemblyBootstrap on CastleBarriersTilemap.");

                var renderer = tilemap.GetComponent<TilemapRenderer>();
                Assert.NotNull(renderer, "Expected TilemapRenderer on CastleBarriersTilemap.");

                bootstrap.RebuildNow();
                Assert.IsFalse(renderer.enabled, "Barrier marker tilemap renderer should be hidden after barrier generation.");
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
        public void BarrierAssemblyBuilder_Rebuild_KeepsGeneratedBarrierRootsUnrotated()
        {
            var parent = new GameObject("GeneratedBarriers");
            var prefab = new GameObject("BarrierPrefab");

            try
            {
                var slots = new[]
                {
                    new BarrierPlacementSlot { Id = "n", Position = new Vector2(0f, 0f), Side = BarrierSide.North },
                    new BarrierPlacementSlot { Id = "e", Position = new Vector2(3f, 0f), Side = BarrierSide.East },
                    new BarrierPlacementSlot { Id = "s", Position = new Vector2(6f, 0f), Side = BarrierSide.South },
                    new BarrierPlacementSlot { Id = "w", Position = new Vector2(9f, 0f), Side = BarrierSide.West }
                };

                BarrierAssemblyBuilder.Rebuild(parent.transform, prefab, slots);

                AssertZRotation(parent.transform.Find("n"), 0f, "North barrier root should remain unrotated.");
                AssertZRotation(parent.transform.Find("e"), 0f, "East barrier root should remain unrotated.");
                AssertZRotation(parent.transform.Find("s"), 0f, "South barrier root should remain unrotated.");
                AssertZRotation(parent.transform.Find("w"), 0f, "West barrier root should remain unrotated.");
            }
            finally
            {
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(parent);
            }
        }

        private static void AssertZRotation(Transform tr, float expectedZ, string message)
        {
            Assert.NotNull(tr, "Expected spawned barrier transform.");
            float z = Mathf.DeltaAngle(0f, tr.eulerAngles.z);
            Assert.That(z, Is.EqualTo(expectedZ).Within(0.5f), message);
        }

        private static List<Vector2> CollectBarrierMarkerCenters(Tilemap tilemap)
        {
            var centers = new List<Vector2>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile == null)
                {
                    continue;
                }

                if (!BarrierTileSideMapper.TryMapTileNameToSide(tile.name, out _))
                {
                    continue;
                }

                centers.Add(tilemap.GetCellCenterWorld(pos));
            }

            return centers;
        }

        private static T FindByName<T>(Scene scene, string name) where T : class
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (typeof(T) == typeof(GameObject) && root.name == name)
                {
                    return root as T;
                }

                foreach (var tr in root.GetComponentsInChildren<Transform>(true))
                {
                    if (tr.name != name)
                    {
                        continue;
                    }

                    if (typeof(T) == typeof(GameObject))
                    {
                        return tr.gameObject as T;
                    }

                    var comp = tr.GetComponent(typeof(T));
                    if (comp != null)
                    {
                        return comp as T;
                    }
                }
            }

            return null;
        }
    }
}

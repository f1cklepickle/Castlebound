using System;
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
    public class MainPrototypeBarrierAssemblyIntegrationTests
    {
        private const string MainPrototypeScenePath = "Assets/_Project/Scenes/MainPrototype.unity";
        private const string GeneratedParentName = "GeneratedBarriers";
        private const string BarrierPrefabPath = "Assets/_Project/Prefabs/Barrier_Gate.prefab";

        [Test]
        public void MainPrototype_RebuildBarriers_GeneratesFromBarrierTilemap_AndIsIdempotent()
        {
            Scene scene = default;
            GameObject prefab = null;

            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var tilemap = FindBarrierMarkerTilemap(scene);
                Assert.NotNull(tilemap, "Expected a tilemap containing barrier marker tiles in MainPrototype.");

                var source = new BarrierTilemapLayoutSource(tilemap);
                var slots = source.GetSlots().ToArray();
                Assert.That(slots.Length, Is.GreaterThan(0), "Expected barrier tilemap to produce at least one barrier slot.");

                var generatedParent = FindOrCreateRoot(scene, GeneratedParentName).transform;
                ClearChildrenImmediate(generatedParent);

                prefab = PrefabTestUtil.Load(BarrierPrefabPath);
                BarrierAssemblyBuilder.Rebuild(generatedParent, prefab, slots);
                Assert.That(generatedParent.childCount, Is.EqualTo(slots.Length), "Expected one generated barrier per slot after first rebuild.");

                BarrierAssemblyBuilder.Rebuild(generatedParent, prefab, slots);
                Assert.That(generatedParent.childCount, Is.EqualTo(slots.Length), "Expected idempotent rebuild without duplicate generated barriers.");

                var uniqueNames = new HashSet<string>();
                foreach (Transform child in generatedParent)
                {
                    Assert.IsTrue(uniqueNames.Add(child.name), $"Duplicate generated barrier name detected: {child.name}");
                }
            }
            finally
            {
                if (prefab != null)
                {
                    PrefabTestUtil.Unload(prefab);
                }

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void MainPrototype_RebuildBarriers_GeneratedInstances_MeetRuntimeContract()
        {
            Scene scene = default;
            GameObject prefab = null;

            try
            {
                scene = EditorSceneManager.OpenScene(MainPrototypeScenePath, OpenSceneMode.Additive);
                Assert.IsTrue(scene.isLoaded, "MainPrototype scene failed to load.");

                var tilemap = FindBarrierMarkerTilemap(scene);
                Assert.NotNull(tilemap, "Expected a tilemap containing barrier marker tiles in MainPrototype.");

                var source = new BarrierTilemapLayoutSource(tilemap);
                var slots = source.GetSlots().ToArray();
                Assert.That(slots.Length, Is.GreaterThan(0), "Expected barrier tilemap to produce at least one barrier slot.");

                var generatedParent = FindOrCreateRoot(scene, GeneratedParentName).transform;
                ClearChildrenImmediate(generatedParent);

                prefab = PrefabTestUtil.Load(BarrierPrefabPath);
                BarrierAssemblyBuilder.Rebuild(generatedParent, prefab, slots);

                foreach (Transform child in generatedParent)
                {
                    var worldPos = (Vector2)child.position;
                    Assert.IsTrue(CastlePlacementRules.IsOnLattice(worldPos),
                        $"Generated barrier '{child.name}' must be on 3-unit lattice. Position=({worldPos.x:0.###},{worldPos.y:0.###})");

                    Assert.NotNull(child.GetComponent<BarrierHealth>(), $"Generated barrier '{child.name}' missing BarrierHealth.");
                    Assert.NotNull(child.GetComponent<BoxCollider2D>(), $"Generated barrier '{child.name}' missing BoxCollider2D.");
                    Assert.NotNull(child.GetComponent<BarrierVisualBinder>(), $"Generated barrier '{child.name}' missing BarrierVisualBinder.");

                    var renderer = child.GetComponent<SpriteRenderer>();
                    Assert.NotNull(renderer, $"Generated barrier '{child.name}' missing SpriteRenderer.");
                    Assert.NotNull(renderer.sprite, $"Generated barrier '{child.name}' missing assigned side sprite.");
                }
            }
            finally
            {
                if (prefab != null)
                {
                    PrefabTestUtil.Unload(prefab);
                }

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static GameObject FindOrCreateRoot(Scene scene, string rootName)
        {
            var existing = FindByName<GameObject>(scene, rootName);
            if (existing != null)
            {
                return existing;
            }

            var created = new GameObject(rootName);
            SceneManager.MoveGameObjectToScene(created, scene);
            return created;
        }

        private static void ClearChildrenImmediate(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
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

        private static Tilemap FindBarrierMarkerTilemap(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var tilemap in root.GetComponentsInChildren<Tilemap>(true))
                {
                    if (HasAnyBarrierMarkers(tilemap))
                    {
                        return tilemap;
                    }
                }
            }

            return null;
        }

        private static bool HasAnyBarrierMarkers(Tilemap tilemap)
        {
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile == null)
                {
                    continue;
                }

                if (BarrierTileSideMapper.TryMapTileNameToSide(tile.name, out _))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

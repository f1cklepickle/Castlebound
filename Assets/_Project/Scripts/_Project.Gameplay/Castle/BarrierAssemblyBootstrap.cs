using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierAssemblyBootstrap : MonoBehaviour
    {
        [SerializeField] private Tilemap barrierTilemap;
        [SerializeField] private GameObject barrierPrefab;
        [SerializeField] private Transform generatedParent;
        [SerializeField] private bool rebuildOnAwake = true;
        [SerializeField] private bool clearGeneratedBeforeRebuild = true;

        private const string GeneratedParentName = "GeneratedBarriers";

        private void Start()
        {
            if (!rebuildOnAwake)
            {
                return;
            }

            RebuildNow();
        }

        [ContextMenu("Rebuild Barriers Now")]
        public void RebuildNow()
        {
            EnsureReferences();
            if (barrierTilemap == null || barrierPrefab == null || generatedParent == null)
            {
                return;
            }

            var sourceTilemap = ResolveSourceTilemap();
            if (sourceTilemap == null)
            {
                return;
            }

            if (clearGeneratedBeforeRebuild)
            {
                ClearGeneratedChildren();
            }

            var source = new BarrierTilemapLayoutSource(sourceTilemap);
            var runner = new BarrierAssemblyRunner(source, barrierPrefab, generatedParent);
            runner.RebuildNow();
            HideMarkerTilemapRenderer(sourceTilemap);
            if (barrierTilemap != sourceTilemap)
            {
                HideMarkerTilemapRenderer(barrierTilemap);
            }
        }

        private void EnsureReferences()
        {
            if (barrierTilemap == null)
            {
                barrierTilemap = GetComponent<Tilemap>();
            }

            if (generatedParent != null)
            {
                return;
            }

            var existing = GameObject.Find(GeneratedParentName);
            if (existing != null)
            {
                generatedParent = existing.transform;
                return;
            }

            var container = new GameObject(GeneratedParentName);
            SceneManager.MoveGameObjectToScene(container, gameObject.scene);
            generatedParent = container.transform;
        }

        private void ClearGeneratedChildren()
        {
            for (var i = generatedParent.childCount - 1; i >= 0; i--)
            {
                var child = generatedParent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private Tilemap ResolveSourceTilemap()
        {
            if (barrierTilemap != null && HasAnyBarrierMarkers(barrierTilemap))
            {
                return barrierTilemap;
            }

            foreach (var tilemap in FindObjectsOfType<Tilemap>(true))
            {
                if (HasAnyBarrierMarkers(tilemap))
                {
                    return tilemap;
                }
            }

            return barrierTilemap;
        }

        private static bool HasAnyBarrierMarkers(Tilemap tilemap)
        {
            if (tilemap == null)
            {
                return false;
            }

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

        private static void HideMarkerTilemapRenderer(Tilemap tilemap)
        {
            if (tilemap == null)
            {
                return;
            }

            var renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }
}

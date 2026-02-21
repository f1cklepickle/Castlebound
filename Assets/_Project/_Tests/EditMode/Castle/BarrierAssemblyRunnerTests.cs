using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class BarrierAssemblyRunnerTests
    {
        [Test]
        public void RebuildNow_SpawnsExpectedCount_AndIsIdempotent()
        {
            var generatedParent = new GameObject("GeneratedBarriers");

            var gridGo = new GameObject("Grid");
            var tilemapGo = new GameObject("BarrierTilemap");
            tilemapGo.transform.SetParent(gridGo.transform);
            var tilemap = tilemapGo.AddComponent<Tilemap>();
            tilemapGo.AddComponent<TilemapRenderer>();

            tilemap.SetTile(new Vector3Int(0, 0, 0), CreateNamedTile("Barrier_Top"));
            tilemap.SetTile(new Vector3Int(1, 0, 0), CreateNamedTile("Barrier_Bottom"));

            var source = new BarrierTilemapLayoutSource(tilemap);
            var barrierPrefab = CreateBarrierPrefab();
            var runner = new BarrierAssemblyRunner(source, barrierPrefab, generatedParent.transform);

            try
            {
                runner.RebuildNow();
                Assert.That(generatedParent.transform.childCount, Is.EqualTo(2), "Expected one generated barrier per slot.");

                runner.RebuildNow();
                Assert.That(generatedParent.transform.childCount, Is.EqualTo(2), "Expected idempotent rebuild with no duplicate barriers.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(barrierPrefab);
                UnityEngine.Object.DestroyImmediate(generatedParent);
                UnityEngine.Object.DestroyImmediate(gridGo);
            }
        }

        private static Tile CreateNamedTile(string name)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            return tile;
        }
        
        private static GameObject CreateBarrierPrefab()
        {
            return new GameObject("Barrier_Gate_Template");
        }
    }
}

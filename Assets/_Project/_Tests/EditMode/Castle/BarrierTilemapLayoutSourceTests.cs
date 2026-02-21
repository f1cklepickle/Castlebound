using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using Castlebound.Gameplay.Castle;

namespace Castlebound.Tests.Castle
{
    public class BarrierTilemapLayoutSourceTests
    {
        [Test]
        public void GetSlots_MapsTileNamesToSides_AndReturnsStableIds()
        {
            var gridGo = new GameObject("Grid");
            var tilemapGo = new GameObject("BarrierTilemap");
            tilemapGo.transform.SetParent(gridGo.transform);

            var tilemap = tilemapGo.AddComponent<Tilemap>();
            tilemapGo.AddComponent<TilemapRenderer>();

            tilemap.SetTile(new Vector3Int(0, 0, 0), CreateNamedTile("Barrier_Top"));
            tilemap.SetTile(new Vector3Int(1, 0, 0), CreateNamedTile("Barrier_Right"));
            var source = new BarrierTilemapLayoutSource(tilemap);

            try
            {
                var first = source.GetSlots().OrderBy(s => s.Id).ToArray();
                var second = source.GetSlots().OrderBy(s => s.Id).ToArray();

                Assert.That(first.Length, Is.EqualTo(2), "Expected one slot per barrier marker tile.");
                Assert.That(second.Length, Is.EqualTo(2), "Expected deterministic slot count on repeated reads.");

                CollectionAssert.AreEquivalent(
                    first.Select(s => s.Id),
                    second.Select(s => s.Id),
                    "Slot IDs must be stable across repeated GetSlots calls.");

                CollectionAssert.AreEquivalent(
                    new[] { BarrierSide.North, BarrierSide.East },
                    first.Select(s => s.Side),
                    "Expected Top->North and Right->East side mapping.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gridGo);
            }
        }

        [Test]
        public void GetSlots_EmitsPositions_OnThreeUnitLattice()
        {
            var gridGo = new GameObject("Grid");
            var tilemapGo = new GameObject("BarrierTilemap");
            tilemapGo.transform.SetParent(gridGo.transform);

            var tilemap = tilemapGo.AddComponent<Tilemap>();
            tilemapGo.AddComponent<TilemapRenderer>();
            tilemap.SetTile(new Vector3Int(2, -1, 0), CreateNamedTile("Barrier_Left"));
            var source = new BarrierTilemapLayoutSource(tilemap);

            try
            {
                var slots = source.GetSlots().ToArray();
                Assert.That(slots.Length, Is.EqualTo(1), "Expected one slot for one barrier marker tile.");

                Vector2 p = slots[0].Position;
                Assert.IsTrue(
                    CastlePlacementRules.IsOnLattice(p),
                    $"Expected slot position on 3-unit lattice. Position=({p.x:0.###},{p.y:0.###})");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gridGo);
            }
        }

        private static Tile CreateNamedTile(string name)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            return tile;
        }
    }
}

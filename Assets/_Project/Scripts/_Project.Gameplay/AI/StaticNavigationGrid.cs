using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Sampled walkability, separate from building occupancy. Bounds use global subdivided cell indices.
    // The supplied Unity Grid must remain unchanged for this snapshot's lifetime; rebuild after changes.
    public sealed class StaticNavigationGrid : IStaticNavigationGraph
    {
        // 0.5-unit centers miss the 2.2-unit doorway at integer alignment for a 0.897-radius body.
        public const int DefaultSubdivisions = 4;
        private static readonly Vector2Int[] directions = {
            Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down,
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1)
        };
        private readonly bool[] walkable;
        private readonly byte[] edges;
        private readonly StaticNavigationLayout layout;
        public BoundsInt Bounds { get; }
        public long Revision => 0;
        public int Subdivisions { get; }
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public int CellCount => walkable.Length;

        public StaticNavigationGrid(Grid worldGrid, BoundsInt navBounds,
            Func<Vector2, bool> pointClear, Func<Vector2, Vector2, bool> segmentClear,
            int subdivisions = DefaultSubdivisions)
        {
            layout = new StaticNavigationLayout(worldGrid, navBounds, subdivisions);
            if (pointClear == null) throw new ArgumentNullException(nameof(pointClear));
            if (segmentClear == null) throw new ArgumentNullException(nameof(segmentClear));
            Bounds = navBounds;
            Subdivisions = subdivisions;
            Width = navBounds.size.x;
            Height = navBounds.size.y;
            CellSize = layout.CellSize;
            walkable = new bool[Width * Height];
            edges = new byte[walkable.Length];
            for (int i = 0; i < CellCount; i++) walkable[i] = pointClear(CellToWorld(Cell(i)));
            // Sample each undirected edge once. Clearance callbacks must remain stable during construction.
            for (int i = 0; i < CellCount; i++)
            {
                Vector2Int cell = Cell(i);
                if (!walkable[i]) continue;
                for (int d = 0; d < directions.Length; d++)
                {
                    Vector2Int next = cell + directions[d];
                    if (!IsWalkable(next) || Index(next) <= i) continue;
                    if (!segmentClear(CellToWorld(cell), CellToWorld(next))) continue;
                    edges[i] |= (byte)(1 << d);
                    int reverse = d < 4 ? (d + 2) % 4 : 4 + (d - 4 + 2) % 4;
                    edges[Index(next)] |= (byte)(1 << reverse);
                }
            }
        }

        public bool Contains(Vector2Int cell) => Bounds.Contains(new Vector3Int(cell.x, cell.y, 0));
        public bool IsWalkable(Vector2Int cell) => Contains(cell) && walkable[Index(cell)];
        public Vector2 CellToWorld(Vector2Int cell) => layout.CellToWorld(cell);
        public bool TryWorldToCell(Vector2 world, out Vector2Int cell) => layout.TryWorldToCell(world, out cell);
        public StaticNavigationSampleState GetCellState(Vector2Int cell)
            => IsWalkable(cell) ? StaticNavigationSampleState.Clear : StaticNavigationSampleState.Blocked;
        public StaticNavigationSampleState GetTraversalState(Vector2Int from, Vector2Int to)
            => CanTraverse(from, to) ? StaticNavigationSampleState.Clear : StaticNavigationSampleState.Blocked;
        public bool CanTraverse(Vector2Int from, Vector2Int to)
        {
            if (!IsWalkable(from) || !IsWalkable(to)) return false;
            Vector2Int delta = to - from;
            if (!HasEdge(from, delta)) return false;
            if (delta.x == 0 || delta.y == 0) return true;
            Vector2Int horizontal = from + new Vector2Int(delta.x, 0);
            Vector2Int vertical = from + new Vector2Int(0, delta.y);
            // Both orthogonal alternatives must be clear, including all four perimeter edges.
            return IsWalkable(horizontal) && IsWalkable(vertical) &&
                HasEdge(from, horizontal - from) && HasEdge(horizontal, to - horizontal) &&
                HasEdge(from, vertical - from) && HasEdge(vertical, to - vertical);
        }

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell)
        {
            foreach (Vector2Int direction in directions)
                if (CanTraverse(cell, cell + direction)) yield return cell + direction;
        }

        private bool HasEdge(Vector2Int cell, Vector2Int delta)
        {
            for (int d = 0; d < directions.Length; d++)
                if (directions[d] == delta) return (edges[Index(cell)] & (1 << d)) != 0;
            return false;
        }
        internal int Index(Vector2Int cell) => layout.Index(cell);
        internal Vector2Int Cell(int index) => layout.Cell(index);
    }
}

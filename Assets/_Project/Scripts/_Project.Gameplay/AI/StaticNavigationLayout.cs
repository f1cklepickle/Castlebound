using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Shared coordinate authority only: no sampling, occupancy, or route state.
    // Recreate snapshots/caches if the supplied Grid geometry changes.
    public sealed class StaticNavigationLayout
    {
        private readonly Grid worldGrid;
        public BoundsInt Bounds { get; }
        public int Subdivisions { get; }
        public float CellSize { get; }
        public int Width => Bounds.size.x;
        public int Height => Bounds.size.y;
        public int CellCount => Width * Height;
        public StaticNavigationLayout(Grid worldGrid, BoundsInt navBounds, int subdivisions = 4)
        {
            if (worldGrid == null) throw new ArgumentNullException(nameof(worldGrid));
            if (subdivisions != 2 && subdivisions != 4) throw new ArgumentOutOfRangeException(nameof(subdivisions));
            if (navBounds.size.x <= 0 || navBounds.size.y <= 0 || navBounds.size.z != 1 || navBounds.zMin != 0 ||
                (long)navBounds.size.x * navBounds.size.y > int.MaxValue ||
                (long)navBounds.xMin + navBounds.size.x > int.MaxValue ||
                (long)navBounds.yMin + navBounds.size.y > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(navBounds));
            Vector3 axisX = worldGrid.CellToWorld(Vector3Int.right) - worldGrid.CellToWorld(Vector3Int.zero);
            Vector3 axisY = worldGrid.CellToWorld(Vector3Int.up) - worldGrid.CellToWorld(Vector3Int.zero);
            if (worldGrid.cellLayout != GridLayout.CellLayout.Rectangle || worldGrid.cellSwizzle != GridLayout.CellSwizzle.XYZ ||
                worldGrid.cellGap != Vector3.zero || worldGrid.cellSize.x <= 0f || worldGrid.cellSize.y <= 0f ||
                !Finite(axisX.magnitude) || !Finite(axisY.magnitude) ||
                axisX.magnitude <= 0f || axisY.magnitude <= 0f ||
                Mathf.Abs(axisX.z) > 0.00001f || Mathf.Abs(axisY.z) > 0.00001f ||
                Mathf.Abs(Vector3.Dot(axisX.normalized, axisY.normalized)) > 0.00001f ||
                !Mathf.Approximately(axisX.magnitude, axisY.magnitude))
                throw new ArgumentException("Navigation requires a planar square rectangular Grid with XYZ swizzle and zero gap.", nameof(worldGrid));
            this.worldGrid = worldGrid;
            Bounds = navBounds;
            Subdivisions = subdivisions;
            CellSize = axisX.magnitude / subdivisions;
        }
        public bool Contains(Vector2Int cell) => Bounds.Contains(new Vector3Int(cell.x, cell.y, 0));
        public Vector2 CellToWorld(Vector2Int cell)
        {
            if (!Contains(cell)) throw new ArgumentOutOfRangeException(nameof(cell));
            // Unity owns cell geometry and transform; only the integer subdivision is navigation-specific.
            return worldGrid.LocalToWorld(worldGrid.CellToLocalInterpolated(
                new Vector3((cell.x + 0.5f) / Subdivisions, (cell.y + 0.5f) / Subdivisions, 0f)));
        }

        public bool TryWorldToCell(Vector2 world, out Vector2Int cell)
        {
            cell = default;
            if (!Finite(world.x) || !Finite(world.y)) return false;
            Vector3 position = new Vector3(world.x, world.y, worldGrid.CellToWorld(Vector3Int.zero).z);
            Vector3Int parent = worldGrid.WorldToCell(position);
            Vector3 local = worldGrid.WorldToLocal(position) - worldGrid.CellToLocal(parent);
            // WorldToCell owns floor/negative-coordinate rules; choose a subcell within that parent.
            int x = Mathf.Clamp(Mathf.FloorToInt(local.x / worldGrid.cellSize.x * Subdivisions), 0, Subdivisions - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(local.y / worldGrid.cellSize.y * Subdivisions), 0, Subdivisions - 1);
            long nx = (long)parent.x * Subdivisions + x, ny = (long)parent.y * Subdivisions + y;
            if (nx < Bounds.xMin || nx >= Bounds.xMax || ny < Bounds.yMin || ny >= Bounds.yMax) return false;
            cell = new Vector2Int((int)nx, (int)ny);
            return true;
        }

        internal int Index(Vector2Int cell) => (cell.y - Bounds.yMin) * Width + cell.x - Bounds.xMin;
        internal Vector2Int Cell(int index) => new Vector2Int(index % Width + Bounds.xMin, index / Width + Bounds.yMin);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
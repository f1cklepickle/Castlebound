using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public static class StaticNavigationPlanningBounds
    {
        // Axis-aligned envelope of the target-oriented corridor, at the foundation's .25 lattice.
        // Padding is 1m, then 3m; the final attempt grows symmetrically under BOTH size caps.
        public static bool TryCreate(BoundsInt world, Vector2Int start, Vector2Int goal, int attempt, out BoundsInt result)
        {
            result = default;
            if (!world.Contains(new Vector3Int(start.x, start.y, 0)) ||
                !world.Contains(new Vector3Int(goal.x, goal.y, 0))) return false;
            int width = Mathf.Abs(goal.x - start.x) + 1, height = Mathf.Abs(goal.y - start.y) + 1;
            if (!Fits(width, height)) return false;
            int padding = attempt == 0 ? 4 : attempt == 1 ? 12 : 96;
            int w = width, h = height;
            for (int i = 0; i < padding; i++)
            {
                bool grew = false;
                if (w + 2 <= world.size.x && Fits(w + 2, h)) { w += 2; grew = true; }
                if (h + 2 <= world.size.y && Fits(w, h + 2)) { h += 2; grew = true; }
                if (!grew) break;
            }
            int x = Mathf.Clamp(Mathf.Min(start.x, goal.x) - (w - width) / 2, world.xMin, world.xMax - w);
            int y = Mathf.Clamp(Mathf.Min(start.y, goal.y) - (h - height) / 2, world.yMin, world.yMax - h);
            result = new BoundsInt(x, y, 0, w, h, 1);
            return true;
        }

        private static bool Fits(int width, int height) => width <= StaticNavigationScheduler.MaxAxisCells &&
            height <= StaticNavigationScheduler.MaxAxisCells && (long)width * height <= StaticNavigationScheduler.MaxRequestCells;
    }
}

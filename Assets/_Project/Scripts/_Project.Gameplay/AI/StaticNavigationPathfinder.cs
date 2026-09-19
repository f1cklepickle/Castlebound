using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public sealed class StaticNavigationPathfinder
    {
        public const int DefaultMaxExpandedNodes = 4096;

        // Existing eager-grid API: the same incremental algorithm, completed synchronously.
        public StaticNavigationPathResult FindPath(StaticNavigationGrid grid, Vector2Int start,
            Vector2Int goal, int maxExpandedNodes = DefaultMaxExpandedNodes)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            var search = new StaticNavigationSearch(grid, grid.Bounds, start, goal, maxExpandedNodes);
            search.Step(maxExpandedNodes);
            return search.Result;
        }
    }
}
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public interface IStaticNavigationGraph
    {
        BoundsInt Bounds { get; }
        long Revision { get; }
        Vector2 CellToWorld(Vector2Int cell);
        // Lazy graphs enqueue sampling when returning Unknown. These reads perform no physics queries.
        StaticNavigationSampleState GetCellState(Vector2Int cell);
        StaticNavigationSampleState GetTraversalState(Vector2Int from, Vector2Int to);
    }
}

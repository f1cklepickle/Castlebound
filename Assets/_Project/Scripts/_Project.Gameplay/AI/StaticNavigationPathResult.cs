using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public sealed class StaticNavigationPathResult
    {
        public StaticNavigationPathStatus Status { get; }
        public IReadOnlyList<Vector2Int> Cells { get; }
        public IReadOnlyList<Vector2> WorldPoints { get; }
        public int ExpandedNodes { get; }
        public double Cost { get; }
        public long WorldRevision { get; }

        internal StaticNavigationPathResult(StaticNavigationPathStatus status, int expandedNodes,
            List<Vector2Int> cells, List<Vector2> worldPoints, double cost = double.PositiveInfinity, long worldRevision = 0)
        {
            Status = status;
            ExpandedNodes = expandedNodes;
            Cells = cells.AsReadOnly();
            WorldPoints = worldPoints.AsReadOnly();
            Cost = cost;
            WorldRevision = worldRevision;
        }
    }
}

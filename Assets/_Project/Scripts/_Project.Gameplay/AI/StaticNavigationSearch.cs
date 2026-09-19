using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Pausing never changes frontier order. An expansion commits only once all required edges are known.
    public sealed class StaticNavigationSearch
    {
        private static readonly Vector2Int[] directions = { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down,
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1) };
        private static readonly double diagonalCost = Math.Sqrt(2d);
        private readonly IStaticNavigationGraph graph;
        private readonly BoundsInt bounds;
        private readonly Vector2Int start, goal;
        private readonly int totalBudget;
        private readonly double[] costs;
        private readonly int[] parents;
        private readonly bool[] closed;
        private readonly StaticNavigationSampleState[] neighborStates = new StaticNavigationSampleState[8];
        private readonly SortedSet<int> open;
        private bool initialized;
        private long revision;
        private StaticNavigationPathResult result;
        public int ExpandedNodes { get; private set; }
        public bool WaitingForSampling { get; private set; }
        // Never expose a result computed against geometry which has since changed.
        public StaticNavigationPathResult Result => revision == graph.Revision ? result : null;

        public StaticNavigationSearch(IStaticNavigationGraph graph, BoundsInt bounds, Vector2Int start,
            Vector2Int goal, int totalBudget = StaticNavigationPathfinder.DefaultMaxExpandedNodes)
        {
            this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
            revision = graph.Revision;
            if (totalBudget < 0) throw new ArgumentOutOfRangeException(nameof(totalBudget));
            if (bounds.size.x <= 0 || bounds.size.y <= 0 || bounds.zMin != 0 || bounds.size.z != 1 ||
                (long)bounds.size.x * bounds.size.y > int.MaxValue || bounds.xMin < graph.Bounds.xMin ||
                bounds.yMin < graph.Bounds.yMin || (long)bounds.xMin + bounds.size.x > graph.Bounds.xMax ||
                (long)bounds.yMin + bounds.size.y > graph.Bounds.yMax) throw new ArgumentOutOfRangeException(nameof(bounds));
            this.bounds = bounds;
            this.start = start;
            this.goal = goal;
            this.totalBudget = totalBudget;
            int count = bounds.size.x * bounds.size.y;
            costs = new double[count]; parents = new int[count]; closed = new bool[count];
            for (int i = 0; i < count; i++) { costs[i] = double.PositiveInfinity; parents[i] = -1; }
            open = new SortedSet<int>(Comparer<int>.Create((a, b) =>
            {
                double ha = Heuristic(Cell(a), goal), hb = Heuristic(Cell(b), goal);
                int compare = (costs[a] + ha).CompareTo(costs[b] + hb);
                if (compare == 0) compare = ha.CompareTo(hb);
                return compare == 0 ? a.CompareTo(b) : compare;
            }));
        }

        // Returns expansions performed in this call. Result remains null until a terminal outcome.
        public int Step(int maxExpansions)
        {
            if (maxExpansions < 0) throw new ArgumentOutOfRangeException(nameof(maxExpansions));
            if (revision != graph.Revision)
            {
                open.Clear();
                for (int i = 0; i < costs.Length; i++)
                { costs[i] = double.PositiveInfinity; parents[i] = -1; closed[i] = false; }
                initialized = false;
                result = null;
                revision = graph.Revision;
                // Preserve ExpandedNodes: geometry changes cannot reset a request's total budget.
            }
            if (Result != null) return 0;
            WaitingForSampling = false;
            int before = ExpandedNodes;
            if (!initialized)
            {
                var startState = Contains(start) ? graph.GetCellState(start) : StaticNavigationSampleState.Blocked;
                if (startState == StaticNavigationSampleState.Blocked) { Fail(StaticNavigationPathStatus.InvalidStart); return 0; }
                var goalState = Contains(goal) ? graph.GetCellState(goal) : StaticNavigationSampleState.Blocked;
                if (startState == StaticNavigationSampleState.Unknown) { WaitingForSampling = true; return 0; }
                if (goalState == StaticNavigationSampleState.Blocked) { Fail(StaticNavigationPathStatus.InvalidGoal); return 0; }
                if (goalState == StaticNavigationSampleState.Unknown) { WaitingForSampling = true; return 0; }
                costs[Index(start)] = 0d;
                open.Add(Index(start));
                initialized = true;
            }
            while (Result == null)
            {
                if (open.Count == 0) { Fail(StaticNavigationPathStatus.NoPath); break; }
                int current = open.Min;
                if (Cell(current) == goal) { Complete(current); break; }
                if (ExpandedNodes >= totalBudget) { Fail(StaticNavigationPathStatus.BudgetExhausted); break; }
                if (ExpandedNodes - before >= maxExpansions) break;
                var cell = Cell(current);
                bool unknown = false;
                for (int d = 0; d < directions.Length; d++)
                {
                    var next = cell + directions[d];
                    neighborStates[d] = !Contains(next) || closed[Index(next)] ? StaticNavigationSampleState.Blocked
                        : graph.GetTraversalState(cell, next);
                    unknown |= neighborStates[d] == StaticNavigationSampleState.Unknown;
                }
                if (unknown) { WaitingForSampling = true; break; }
                open.Remove(current);
                closed[current] = true;
                ExpandedNodes++;
                for (int d = 0; d < directions.Length; d++)
                {
                    if (neighborStates[d] != StaticNavigationSampleState.Clear) continue;
                    int next = Index(cell + directions[d]);
                    double cost = costs[current] + (d < 4 ? 1d : diagonalCost);
                    if (cost >= costs[next]) continue;
                    open.Remove(next);
                    costs[next] = cost;
                    parents[next] = current;
                    open.Add(next);
                }
            }
            return ExpandedNodes - before;
        }

        private void Complete(int index)
        {
            double cost = costs[index];
            var cells = new List<Vector2Int>();
            while (index >= 0) { cells.Add(Cell(index)); index = parents[index]; }
            cells.Reverse();
            var points = new List<Vector2>(cells.Count);
            foreach (var cell in cells) points.Add(graph.CellToWorld(cell));
            result = new StaticNavigationPathResult(StaticNavigationPathStatus.PathFound, ExpandedNodes, cells, points, cost, revision);
        }
        private void Fail(StaticNavigationPathStatus status)
            => result = new StaticNavigationPathResult(status, ExpandedNodes, new List<Vector2Int>(), new List<Vector2>(), worldRevision: revision);
        private bool Contains(Vector2Int cell) => bounds.Contains(new Vector3Int(cell.x, cell.y, 0));
        private int Index(Vector2Int cell) => (cell.y - bounds.yMin) * bounds.size.x + cell.x - bounds.xMin;
        private Vector2Int Cell(int index) => new Vector2Int(index % bounds.size.x + bounds.xMin, index / bounds.size.x + bounds.yMin);
        private static double Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Math.Abs(a.x - b.x), dy = Math.Abs(a.y - b.y);
            return Math.Max(dx, dy) + (diagonalCost - 1d) * Math.Min(dx, dy);
        }
    }
}

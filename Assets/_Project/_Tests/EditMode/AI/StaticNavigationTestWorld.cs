using System;
using Castlebound.Gameplay.AI;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public sealed class StaticNavigationTestWorld : IEnemyNavigationWorld, IDisposable
    {
        private readonly Grid grid;
        private StaticNavigationPathResult completed;
        public Func<Vector2, Vector2, StaticNavigationSampleState> SegmentQuery;
        public Func<Vector2, StaticNavigationSampleState> PointQuery;
        public Func<Vector2, Vector2, StaticNavigationSampleState> SightQuery;
        public StaticNavigationWorldCache Cache { get; }
        public float BodyRadius => 0.87684506f;
        public int Submissions { get; private set; }
        public int LastAttempt { get; private set; }
        public Vector2Int LastStart { get; private set; }
        public Vector2Int LastGoal { get; private set; }
        public StaticNavigationTestWorld()
        {
            grid = new GameObject("Navigation fixture grid", typeof(Grid)).GetComponent<Grid>();
            Cache = new StaticNavigationWorldCache(new StaticNavigationLayout(grid, new BoundsInt(-64, -64, 0, 128, 128, 1)),
                p => StaticNavigationSampleState.Clear, (a, b) => StaticNavigationSampleState.Clear);
        }
        public StaticNavigationSampleState Point(Vector2 point) => PointQuery != null ? PointQuery(point) : StaticNavigationSampleState.Clear;
        public StaticNavigationSampleState Segment(Vector2 a, Vector2 b) => SegmentQuery != null ? SegmentQuery(a, b) : StaticNavigationSampleState.Clear;
        public StaticNavigationSampleState Sight(Vector2 a, Vector2 b) => SightQuery != null ? SightQuery(a, b) : StaticNavigationSampleState.Clear;
        public StaticNavigationSampleState Connect(Vector2 start, out Vector2Int cell)
        {
            Cache.TryWorldToCell(start, out cell); Cache.GetCellState(cell); Cache.SamplePending(128);
            return StaticNavigationSampleState.Clear;
        }
        public bool TrySubmit(Vector2Int start, Vector2Int goal, int attempt, out int id)
        { Submissions++; id = Submissions; LastStart = start; LastGoal = goal; LastAttempt = attempt; return true; }
        public bool TryResult(int id, out StaticNavigationPathResult result)
        { result = completed; completed = null; return result != null; }
        public void Abandon(int id) { completed = null; }
        public void Complete(bool success = true)
        {
            var eager = new StaticNavigationGrid(grid, new BoundsInt(-16, -16, 0, 32, 32, 1),
                p => success || p.x < 1f || p.x > 1.5f, (a, b) => true);
            completed = new StaticNavigationPathfinder().FindPath(eager, LastStart, LastGoal);
        }
        public void Dispose() => UnityEngine.Object.DestroyImmediate(grid.gameObject);
    }
}

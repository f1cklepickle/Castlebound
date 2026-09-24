using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class StaticNavigationRuntime : MonoBehaviour, IEnemyNavigationWorld
    {
        [SerializeField] private Grid worldGrid;
        [SerializeField] private Vector2Int minimumCell = new Vector2Int(-512, -512);
        [SerializeField] private Vector2Int sizeInCells = new Vector2Int(1024, 1024);
        [SerializeField] private float bodyRadius = 0.87684506f;
        [SerializeField] private float clearanceMargin = 0.02f;
        private readonly Dictionary<int, StaticNavigationPathResult> requests = new Dictionary<int, StaticNavigationPathResult>();
        private StaticNavigationWorld2D world;
        private StaticNavigationStartConnector connector;
        private StaticNavigationRuntimeQueries queries;
        private StaticNavigationRecoveryQueries recoveryQueries;
        private int lastTick = -1;
        public static StaticNavigationRuntime Instance { get; private set; }
        public StaticNavigationWorldCache Cache { get; private set; }
        public StaticNavigationScheduler Scheduler { get; private set; }
        public float BodyRadius => bodyRadius;
        public int SubmittedRequests { get; private set; }
        public int SamplingQueriesLastTick { get; private set; }
        public int LiveQueriesThisFrame => queries != null ? queries.Used : 0;

        private void Awake()
        {
            if (Instance != null && Instance != this) { enabled = false; return; }
            Instance = this;
            if (worldGrid == null) worldGrid = GetComponent<Grid>();
            if (worldGrid != null) Initialize(worldGrid, new BoundsInt(minimumCell.x, minimumCell.y, 0, sizeInCells.x, sizeInCells.y, 1));
        }
        public void Initialize(Grid grid, BoundsInt bounds)
        {
            connector?.Dispose();
            recoveryQueries?.Dispose();
            requests.Clear(); lastTick = -1;
            var layout = new StaticNavigationLayout(grid, bounds);
            if (!Mathf.Approximately(layout.CellSize, 0.25f))
                throw new System.ArgumentException("Runtime navigation requires the validated 0.25-unit lattice.");
            world = new StaticNavigationWorld2D(bodyRadius, clearanceMargin);
            Cache = new StaticNavigationWorldCache(layout, world);
            Scheduler = new StaticNavigationScheduler(Cache);
            connector = new StaticNavigationStartConnector(Cache, world);
            queries = new StaticNavigationRuntimeQueries(world, connector);
            recoveryQueries = new StaticNavigationRecoveryQueries(world);
        }
        private void Update() => Tick(Time.frameCount);
        private void OnEnable()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) enabled = false;
        }
        private void OnDisable() { if (Instance == this) Instance = null; }
        public void Tick(int frame)
        {
            if (Cache == null || frame <= lastTick) return;
            lastTick = frame; queries.BeginFrame(frame);
            // Goal/start sampling must progress even before there is an A* request.
            int samples = Cache.SamplePending(32);
            Scheduler.Tick(frame, StaticNavigationScheduler.QueriesPerTick - samples);
            SamplingQueriesLastTick = samples;
#if UNITY_EDITOR
            SamplingQueriesLastTick += Scheduler.DebugLastSamplingQueries;
#endif
            if (Scheduler.TryGetCompleted(out int id, out var result) && requests.ContainsKey(id)) requests[id] = result;
        }
        private bool Ready()
        {
            if (!isActiveAndEnabled || Cache == null) return false;
            queries.BeginFrame(Time.frameCount);
            return true;
        }
        public StaticNavigationSampleState Point(Vector2 point) => Ready() ? queries.Point(point) : StaticNavigationSampleState.Unknown;
        public StaticNavigationSampleState Segment(Vector2 start, Vector2 end) => Ready() ? queries.Segment(start, end) : StaticNavigationSampleState.Unknown;
        public StaticNavigationSampleState Sight(Vector2 start, Vector2 end) => Ready() ? queries.Sight(start, end) : StaticNavigationSampleState.Unknown;
        public StaticNavigationSampleState Connect(Vector2 start, out Vector2Int cell)
        {
            cell = default;
            return Ready() ? queries.Connect(start, Cache, out cell) : StaticNavigationSampleState.Unknown;
        }
        public bool TrySubmit(Vector2Int start, Vector2Int goal, int attempt, out int id)
        {
            id = 0;
            if (!Ready() || !StaticNavigationPlanningBounds.TryCreate(Cache.Bounds, start, goal, attempt, out var bounds) ||
                !Scheduler.TrySubmit(bounds, start, goal, out id)) return false;
            requests.Add(id, null); SubmittedRequests++; return true;
        }
        public bool TryResult(int id, out StaticNavigationPathResult result)
        {
            if (!requests.TryGetValue(id, out result) || result == null) return false;
            requests.Remove(id); return true;
        }
        public void Abandon(int id) => requests.Remove(id);
        public bool Recover(EnemyNavigationRecovery recovery, CircleCollider2D body, float speed, float dt,
            bool paused, out Vector2 velocity)
        {
            velocity = Vector2.zero;
            return !Ready() || recovery.Compute(recoveryQueries, body, speed, dt, Time.time,
                out velocity, queries.Spend, paused);
        }
        public Vector2 ConstrainRecovery(EnemyNavigationRecovery recovery, CircleCollider2D body, Vector2 displacement)
            => Ready() ? recovery.Constrain(recoveryQueries, body, displacement, queries.Spend) : Vector2.zero;
        public void Invalidate(Rect before, Rect after)
        {
            if (Cache == null) return;
            Physics2D.SyncTransforms();
            Cache.InvalidateWorldBounds(before, after);
            queries.Invalidate();
        }
        private void OnDestroy()
        {
            connector?.Dispose();
            recoveryQueries?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using Castlebound.Gameplay.AI;

public class EnemyController2D : MonoBehaviour
{
    public enum State
    {
        CHASE,
        HOLD
    }

    // Static registry for manager access
    internal static readonly List<EnemyController2D> All = new List<EnemyController2D>();

    public static bool ShouldHoldForBarrierTarget(
        float distanceToBarrier,
        bool barrierBroken,
        float holdRadius,
        float releaseMargin,
        int distTrend,
        int outrunFrames)
    {
        // For now, this is intentionally minimal:
        // - If the barrier is broken, enemies should *not* HOLD at the barrier.
        // - Otherwise, they may HOLD when inside the hold radius.
        if (barrierBroken)
            return false;

        return distanceToBarrier <= holdRadius;
    }

    [SerializeField] private Transform target;           // current orbit/spacing target (player)
    [SerializeField] private Transform player;           // authoritative player reference
    [SerializeField] private Transform barrier;
    [SerializeField] private Transform homeBarrier;      // per-enemy assigned barrier (nearest at spawn)
    [SerializeField] private Transform steerTarget;      // movement target (home barrier outside, player inside)
    [SerializeField] private float passThroughRadius = 0.6f; // distance to consider "at" the barrier opening

    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float holdRadius = 2.6f;    // R_in
    [SerializeField] private float releaseMargin = 0.6f; // R_out
    [SerializeField] private float orbitBase = 0.8f;
    [SerializeField] private float maxTangent = 2.5f;
    [SerializeField] private int outrunFrames = 8;
    [SerializeField] private float epsilonDist = 0.01f;
    [SerializeField] private float reseatBias = 0.3f;
    [SerializeField] private bool useBarrierTargeting = true;

    public Transform Target => target;
    private EnemyTargetType _currentTargetType = EnemyTargetType.None;
    public EnemyTargetType CurrentTargetType => _currentTargetType;

    // NEW: used by EnemyAttack to decide when we're close enough to attack.
    public bool IsInHoldRange()
    {
        return _state == State.HOLD;
    }

    private Rigidbody2D _rb;
    private State _state = State.CHASE;
    private float _prevDist;
    private int _distTrend;
    private float _gapCW;
    private float _gapCCW;
    // Last non-zero direction toward our current target (for pass-through).
    private Vector2 _lastNonZeroDir = Vector2.right;

    public void SetAngularGaps(float gapCW, float gapCCW)
    {
        _gapCW = gapCW;
        _gapCCW = gapCCW;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Ensure player reference is valid.
        EnsurePlayerReference();

        // Initialize target from player.
        if (player != null)
        {
            target = player;
            steerTarget = player;
            Vector2 pos = _rb != null ? _rb.position : (Vector2)transform.position;
            _prevDist = Vector2.Distance(pos, target.position);
        }
        else
        {
            target = null;
            steerTarget = null;
            _prevDist = 0f;
        }

        // Home barrier assignment deferred to Start to ensure barriers are registered.
    }

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        if (useBarrierTargeting && homeBarrier == null)
        {
            homeBarrier = CastleTargetSelector.AssignHomeBarrier(
                transform.position,
                GetAllBarrierTransforms());
            barrier = homeBarrier;
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        bool enemyInsideCastle = false;
        bool playerInsideCastle = false;

        if (player != null)
        {
            var region = CastleRegionTracker.Instance;

            if (useBarrierTargeting && region != null)
            {
                playerInsideCastle = region.PlayerInside;
                enemyInsideCastle = region.EnemyInside(this);
            }

            var decision = EvaluateTargetDecision(playerInsideCastle, enemyInsideCastle);
            steerTarget = decision.SteerTarget != null ? decision.SteerTarget : player;
            target = decision.AttackTarget != null ? decision.AttackTarget : player;
            _currentTargetType = decision.TargetType;

            if (useBarrierTargeting && _currentTargetType == EnemyTargetType.Barrier)
            {
                barrier = steerTarget;
            }
        }

        if (target == null) return;
        if (steerTarget == null) steerTarget = target;

        Vector2 pos = _rb.position;
        EnemyMovement.ComputeMovement(
            pos,
            steerTarget,
            barrier,
            holdRadius,
            releaseMargin,
            reseatBias,
            speed,
            orbitBase,
            maxTangent,
            outrunFrames,
            epsilonDist,
            _gapCW,
            _gapCCW,
            ref _state,
            ref _prevDist,
            ref _distTrend,
            ref _lastNonZeroDir,
            out Vector2 radial,
            out Vector2 tangent);

        float dt = Time.fixedDeltaTime;
        _rb.MovePosition(pos + (radial + tangent) * dt);
    }

    private Transform SelectTarget(bool playerInside, bool enemyInside)
    {
        var decision = EvaluateTargetDecision(playerInside, enemyInside);
        return decision.AttackTarget;
    }

    private Transform SelectSteerTarget(bool playerInside, bool enemyInside)
    {
        var decision = EvaluateTargetDecision(playerInside, enemyInside);
        return decision.SteerTarget;
    }

    public Transform Debug_SelectTarget(bool playerInside, bool enemyInside)
    {
        return SelectTarget(playerInside, enemyInside);
    }

    public Transform Debug_SteerTarget(bool playerInside, bool enemyInside)
    {
        return SelectSteerTarget(playerInside, enemyInside);
    }

    // Debug/test helper to ensure player reference via tag/lookup.
    public void Debug_EnsurePlayerReference()
    {
        EnsurePlayerReference();
    }

    // Test helper: force references for deterministic behavior in EditMode tests.
    public void Debug_SetupRefs(Transform playerRef, Transform homeRef = null)
    {
        player = playerRef;
        target = playerRef;
        steerTarget = playerRef;
        homeBarrier = homeRef;
        barrier = homeRef;

        if (homeBarrier == null && useBarrierTargeting)
        {
            homeBarrier = CastleTargetSelector.AssignHomeBarrier(
                transform.position,
                GetAllBarrierTransforms());
            barrier = homeBarrier;
        }
    }

#if UNITY_EDITOR
    // Editor-only validation helper to surface missing refs without relying on Unity lifecycle.
    public void Debug_ValidateRefs()
    {
        if (player == null)
        {
            Debug.LogWarning("[EnemyController2D] Player reference not found in scene. Enemy will have no target.", this);
        }

        if (useBarrierTargeting && homeBarrier == null)
        {
            Debug.LogWarning("[EnemyController2D] No home barrier found while barrier targeting is enabled.", this);
        }
    }
#endif

    private void EnsurePlayerReference()
    {
        if (player != null) return;

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
            target = player;
            steerTarget = player;
            return;
        }

        var pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            player = pc.transform;
            target = player;
            steerTarget = player;
        }
    }

    private void EnsureHomeBarrier(Transform[] gates)
    {
        if (homeBarrier != null) return;
        if (!useBarrierTargeting) return;

        homeBarrier = CastleTargetSelector.AssignHomeBarrier(
            transform.position,
            gates);
        barrier = homeBarrier;
    }

    private EnemyTargetSelector.Decision EvaluateTargetDecision(bool playerInside, bool enemyInside)
    {
        if (useBarrierTargeting)
        {
            EnsureHomeBarrier(GetAllBarrierTransforms());
        }

        return EnemyTargetSelector.Select(new EnemyTargetSelector.Input
        {
            EnemyPosition = transform.position,
            EnemyInside = enemyInside,
            PlayerInside = playerInside,
            Player = player,
            HomeBarrier = homeBarrier,
            PassThroughRadius = passThroughRadius
        });
    }

    private static Transform[] GetAllBarrierTransforms()
    {
        var all = BarrierHealth.All;
        if (all == null || all.Count == 0)
            return System.Array.Empty<Transform>();

        var result = new Transform[all.Count];
        for (int i = 0; i < all.Count; i++)
        {
            result[i] = all[i] != null ? all[i].transform : null;
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, holdRadius);
        Gizmos.color = new Color(0.9f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, holdRadius + releaseMargin);
    }

#if UNITY_EDITOR
    // Editor-only helper to compute movement vectors without moving.
    public void Debug_ComputeMovement(out Vector2 radial, out Vector2 tangent)
    {
        radial = Vector2.zero;
        tangent = Vector2.zero;
        if (_rb == null) return;

        EnemyMovement.ComputeMovement(
            _rb.position,
            steerTarget,
            barrier,
            holdRadius,
            releaseMargin,
            reseatBias,
            speed,
            orbitBase,
            maxTangent,
            outrunFrames,
            epsilonDist,
            _gapCW,
            _gapCCW,
            ref _state,
            ref _prevDist,
            ref _distTrend,
            ref _lastNonZeroDir,
            out radial,
            out tangent);
    }
#endif
}

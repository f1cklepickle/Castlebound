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

        // True while we are walking straight through a broken barrier.
        private bool _isPassingThroughBarrier = false;

        // Direction to walk while in pass-through phase.
        private Vector2 _passThroughDir = Vector2.right;

    public void SetAngularGaps(float gapCW, float gapCCW)
    {
        _gapCW = gapCW;
        _gapCCW = gapCCW;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        // Ensure player reference is valid.
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                player = playerGO.transform;
            else
            {
                var pc = FindObjectOfType<PlayerController>();
                if (pc != null)
                    player = pc.transform;
            }
        }

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

        if (player != null)
        {
            var region = CastleRegionTracker.Instance;

            if (useBarrierTargeting && region != null)
            {
                bool playerInside = region.PlayerInside;
                enemyInsideCastle = region.EnemyInside(this);

                // Determine steering target: outside → home barrier; inside → player.
                steerTarget = SelectSteerTarget(playerInside, enemyInsideCastle);

                // Orbit/spacing target remains the player.
                target = player;

                if (useBarrierTargeting)
                {
                    var targetBarrier = steerTarget != null ? steerTarget.GetComponent<BarrierHealth>() : null;
                    if (targetBarrier != null)
                    {
                        barrier = targetBarrier.transform;
                    }
                }
            }
            else
            {
                // Fallback: direct chase
                target = player;
                steerTarget = player;
            }
        }

        if (target == null) return;
        if (steerTarget == null) steerTarget = target;

        Vector2 pos = _rb.position;
        Vector2 toTarget = (Vector2)steerTarget.position - pos;
        float dist = toTarget.magnitude;

        Vector2 dir = dist > 1e-6f ? (toTarget / dist) : Vector2.zero;

        if (dir != Vector2.zero)
        {
            _lastNonZeroDir = dir;
        }

        if (dist > _prevDist + epsilonDist)
            _distTrend++;
        else if (dist < _prevDist - epsilonDist)
            _distTrend = 0;

        _prevDist = dist;

        bool isBarrierTarget = (barrier != null && steerTarget == barrier);
        bool barrierBroken = false;

        if (isBarrierTarget)
        {
            var barrierHealth = barrier.GetComponent<BarrierHealth>();
            if (barrierHealth != null)
            {
                barrierBroken = barrierHealth.IsBroken;
            }
        }

        // No early retarget on broken barrier; switch happens when enemyInside becomes true.

        float rIn = holdRadius;
        float rOut = holdRadius + releaseMargin;

        if (isBarrierTarget)
        {
            var holdBehavior = barrier.GetComponent<EnemyBarrierHoldBehavior>();
            float effectiveHoldRadius = holdRadius;
            float effectiveReleaseMargin = releaseMargin;
            float distToBarrier = dist;

            if (holdBehavior != null)
            {
                distToBarrier = holdBehavior.DistanceToAnchor(pos);
                effectiveHoldRadius = holdBehavior.HoldRadius;
                effectiveReleaseMargin = holdBehavior.ReleaseMargin;
            }

            // For barriers, we delegate to the helper so broken barriers
            // never produce HOLD, and intact ones may HOLD inside radius.
            bool shouldHold = ShouldHoldForBarrierTarget(
                distToBarrier,
                barrierBroken,
                effectiveHoldRadius,
                effectiveReleaseMargin,
                _distTrend,
                outrunFrames);

            _state = shouldHold ? State.HOLD : State.CHASE;
        }
        else
        {
            // Existing CHASE/HOLD logic for non-barrier targets (e.g. player).
            if (_state == State.CHASE)
            {
                if (dist <= rIn)
                    _state = State.HOLD;
            }
            else
            {
                if (dist >= rOut || _distTrend >= outrunFrames)
                    _state = State.CHASE;
            }
        }

        Vector2 radial = Vector2.zero;
        Vector2 tangent = Vector2.zero;

        if (_state == State.CHASE)
        {
            // CHASE: always move straight at the target.
            radial = dir * speed;
        }
        else
        {
            // HOLD behavior depends on whether we're holding at a barrier
            // or at a non-barrier target (e.g. the player).
            if (isBarrierTarget)
            {
                // HOLD at barrier: stand near the barrier and attack.
                // No orbiting; just a small reseat if we drift too far.
                if (dist > rIn)
                {
                    radial = dir * (reseatBias * speed);
                }

                tangent = Vector2.zero;
            }
            else
            {
                // Existing ring/orbit logic for non-barrier targets (player).
                if (dist > rIn)
                    radial = dir * (reseatBias * speed);

                float preference = _gapCCW - _gapCW;
                bool hasNoGaps = (_gapCW == 0f && _gapCCW == 0f);

                float sign = 0f;

                if (!hasNoGaps)
                {
                    if (preference > 0f)
                        sign = 1f;
                    else if (preference < 0f)
                        sign = -1f;
                }

                float tangentMag = 0f;

                if (dist > 0f && !hasNoGaps && sign != 0f)
                {
                    float orbitDist = Mathf.Max(dist, rIn);
                    tangentMag = orbitBase * (speed * orbitDist / Mathf.Max(rIn, 0.01f));
                    tangentMag = Mathf.Min(tangentMag, maxTangent);
                }

                if (dir != Vector2.zero)
                {
                    Vector2 perpCCW = new Vector2(-dir.y, dir.x);
                    tangent = perpCCW * (sign * tangentMag);
                }
            }
        }

        float dt = Time.fixedDeltaTime;
        _rb.MovePosition(pos + (radial + tangent) * dt);
    }

    private Transform SelectTarget(bool playerInside, bool enemyInside)
    {
        Transform[] gates = useBarrierTargeting
            ? GetAllBarrierTransforms()
            : System.Array.Empty<Transform>();

        EnsureHomeBarrier(gates);

        if (!enemyInside && playerInside && homeBarrier != null)
        {
            // If broken and we're effectively at the barrier opening, switch to player.
            var hbHealth = homeBarrier.GetComponent<BarrierHealth>();
            bool broken = hbHealth != null && hbHealth.IsBroken;
            float distToHome = Vector2.Distance(transform.position, homeBarrier.position);
            if (broken && distToHome <= passThroughRadius)
                return player;

            return homeBarrier;
        }

        return player;
    }

    private Transform SelectSteerTarget(bool playerInside, bool enemyInside)
    {
        return SelectTarget(playerInside, enemyInside);
    }

    public Transform Debug_SelectTarget(bool playerInside, bool enemyInside)
    {
        return SelectTarget(playerInside, enemyInside);
    }

    public Transform Debug_SteerTarget(bool playerInside, bool enemyInside)
    {
        return SelectSteerTarget(playerInside, enemyInside);
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

    private void EnsureHomeBarrier(Transform[] gates)
    {
        if (homeBarrier != null) return;
        if (!useBarrierTargeting) return;

        homeBarrier = CastleTargetSelector.AssignHomeBarrier(
            transform.position,
            gates);
        barrier = homeBarrier;
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
}

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

    [SerializeField] private Transform target;           // current chase target (player or barrier)
    [SerializeField] private Transform player;           // authoritative player reference
    [SerializeField] private Transform barrier;

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
        }

        // Initialize target from player.
        if (player != null)
        {
            target = player;
            Vector2 pos = _rb != null ? _rb.position : (Vector2)transform.position;
            _prevDist = Vector2.Distance(pos, target.position);
        }
        else
        {
            target = null;
            _prevDist = 0f;
        }
    }

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
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

                Transform[] gates = barrier != null
                    ? new[] { barrier }
                    : System.Array.Empty<Transform>();

                target = CastleTargetSelector.ChooseTarget(
                    transform.position,
                    enemyInsideCastle,
                    playerInside,
                    player,
                    gates);
            }
            else
            {
                // Fallback: direct chase
                target = player;
            }
        }

        if (target == null) return;

        Vector2 pos = _rb.position;
        Vector2 toTarget = (Vector2)target.position - pos;
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

        bool isBarrierTarget = (barrier != null && target == barrier);
        bool barrierBroken = false;

        if (isBarrierTarget)
        {
            var barrierHealth = barrier.GetComponent<BarrierHealth>();
            if (barrierHealth != null)
            {
                barrierBroken = barrierHealth.IsBroken;
            }
        }

        if (isBarrierTarget && barrierBroken && player != null)
        {
            // Broken barrier: retarget to player and recompute direction.
            target = player;
            isBarrierTarget = false;

            if (_state == State.HOLD)
                _state = State.CHASE;

            var toPlayer = (Vector2)player.position - pos;
            var playerDist = toPlayer.magnitude;

            toTarget = toPlayer;
            dist = playerDist;
            dir = playerDist > 1e-6f ? (toPlayer / playerDist) : Vector2.zero;

            if (dir != Vector2.zero)
            {
                _lastNonZeroDir = dir;
            }
        }

        float rIn = holdRadius;
        float rOut = holdRadius + releaseMargin;

        if (isBarrierTarget)
        {
            // For barriers, we delegate to the helper so broken barriers
            // never produce HOLD, and intact ones may HOLD inside radius.
            bool shouldHold = ShouldHoldForBarrierTarget(
                dist,
                barrierBroken,
                holdRadius,
                releaseMargin,
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, holdRadius);
        Gizmos.color = new Color(0.9f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, holdRadius + releaseMargin);
    }
}

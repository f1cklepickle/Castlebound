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

        if (player != null)
        {
            var region = CastleRegionTracker.Instance;

            if (useBarrierTargeting && region != null)
            {
                bool playerInside = region.PlayerInside;
                bool enemyInside = region.EnemyInside(this);

                Transform[] gates = barrier != null
                    ? new[] { barrier }
                    : System.Array.Empty<Transform>();

                target = CastleTargetSelector.ChooseTarget(
                    transform.position,
                    enemyInside,
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

        if (dist > _prevDist + epsilonDist)
            _distTrend++;
        else if (dist < _prevDist - epsilonDist)
            _distTrend = 0;

        _prevDist = dist;

        float rIn = holdRadius;
        float rOut = holdRadius + releaseMargin;

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

        Vector2 radial = Vector2.zero;
        Vector2 tangent = Vector2.zero;

        if (_state == State.CHASE)
        {
            radial = dir * speed;
        }
        else
        {
            if (dist > rIn)
                radial = dir * (reseatBias * speed);

            float preference = _gapCCW - _gapCW;
            bool hasNoGaps = (_gapCW == 0f && _gapCCW == 0f);

            float sign = 0f;
            if (!hasNoGaps)
            {
                if (preference > 0f) sign = +1f;
                else if (preference < 0f) sign = -1f;
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

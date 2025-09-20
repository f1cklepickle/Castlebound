using System.Collections.Generic;
using UnityEngine;

public class EnemyController2D : MonoBehaviour
{
    public enum State
    {
        CHASE,
        HOLD
    }

    // Static registry for manager access
    internal static readonly List<EnemyController2D> All = new List<EnemyController2D>();

    [SerializeField] private Transform target;           // auto-find by Tag "Player" if null
    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float holdRadius = 2.6f;    // R_in
    [SerializeField] private float releaseMargin = 0.6f; // R_out
    [SerializeField] private float orbitBase = 0.8f;
    [SerializeField] private float angularGain = 2.5f;
    [SerializeField] private float maxTangent = 2.5f;
    [SerializeField] private int outrunFrames = 8;
    [SerializeField] private float epsilonDist = 0.01f;
    [SerializeField] private float reseatBias = 0.3f;    // small inward nudge if slightly outside R_in

    // Public API
    public Transform Target => target;
    public bool IsInHoldRange()
    {
        if (target == null) return false;
        Vector2 pos = _rb != null ? _rb.position : (Vector2)transform.position;
        float dist = Vector2.Distance(pos, target.position);
        return dist <= holdRadius;
    }

    private Rigidbody2D _rb;

    // State
    private State _state = State.CHASE;

    // Outrun detection
    private float _prevDist;
    private int _distTrend; // consecutive increases

    // Angular gaps (radians) provided by manager
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

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        if (target != null)
        {
            Vector2 pos = _rb != null ? _rb.position : (Vector2)transform.position;
            _prevDist = Vector2.Distance(pos, target.position);
        }
        else
        {
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
        if (target == null) return;

        Vector2 pos = _rb.position;
        Vector2 toTarget = (Vector2)target.position - pos;
        float dist = toTarget.magnitude;

        Vector2 dir = dist > 1e-6f ? (toTarget / dist) : Vector2.zero;

        // Update outrun trend (consecutive increases)
        if (dist > _prevDist + epsilonDist)
        {
            _distTrend++;
        }
        else
        {
            _distTrend = 0;
        }
        _prevDist = dist;

        float rIn = holdRadius;
        float rOut = holdRadius + releaseMargin;

        // State transitions (hysteresis + outrun)
        if (_state == State.CHASE)
        {
            if (dist <= rIn)
                _state = State.HOLD;
        }
        else // HOLD
        {
            if (dist >= rOut || _distTrend >= outrunFrames)
                _state = State.CHASE;
        }

        Vector2 radial = Vector2.zero;
        Vector2 tangent = Vector2.zero;

        if (_state == State.CHASE)
        {
            radial = dir * speed;
            // tangent = Vector2.zero;
        }
        else // HOLD
        {
            // Small inward nudge only if outside R_in
            if (dist > rIn)
            {
                radial = dir * (reseatBias * speed);
            }

            // Determine tangential movement based on gap preference
            float preference = _gapCCW - _gapCW; // >0 => prefer CCW, <0 => prefer CW
            bool hasNoGaps = (_gapCW == 0f && _gapCCW == 0f);
            float sign = hasNoGaps ? 1f : (preference < 0f ? -1f : 1f);

            float normPref = Mathf.Clamp01(Mathf.Abs(preference) / Mathf.PI);
            float tangentMag = orbitBase + normPref * angularGain;
            if (tangentMag > maxTangent) tangentMag = maxTangent;

            if (dir != Vector2.zero)
            {
                // CCW perpendicular to dir: (-y, x)
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

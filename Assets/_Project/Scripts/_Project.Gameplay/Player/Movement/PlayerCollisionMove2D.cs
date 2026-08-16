using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[DefaultExecutionOrder(100)]
public class PlayerCollisionMove2D : MonoBehaviour
{
    private const int MaxSlideIterations = 4;
    private const int HitCapacity = 24;
    private const int ContactNormalCapacity = 8;
    private const float MinDisplacement = 0.00001f;
    private const float ContactMergeDistance = 0.001f;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private LayerMask solidMask;
    [SerializeField] private float skin = 0.02f;

    private Rigidbody2D _rb;
    private CircleCollider2D _col;
    private Vector2 _input;
    private Vector2 _velocityOverride;
    private bool _useVelocityOverride;
    private Vector2 _previousCenterOffset;
    private bool _hasPreviousCenterOffset;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0f, value);
    }

    private readonly RaycastHit2D[] _hits = new RaycastHit2D[HitCapacity];
    private readonly Vector2[] _contactNormals = new Vector2[ContactNormalCapacity];

    public void SetMoveInput(Vector2 input)
    {
        // Clamp per-axis to [-1, 1] without allocating
        _input.x = Mathf.Clamp(input.x, -1f, 1f);
        _input.y = Mathf.Clamp(input.y, -1f, 1f);
        _useVelocityOverride = false;
    }

    public void SetMoveVelocity(Vector2 velocity)
    {
        _velocityOverride = velocity;
        _useVelocityOverride = true;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CircleCollider2D>();

        if (_rb != null && _rb.bodyType != RigidbodyType2D.Kinematic)
            _rb.bodyType = RigidbodyType2D.Kinematic;

        RefreshCenterOffset();
    }

    private void Reset()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void FixedUpdate()
    {
        if (_rb == null || _col == null) return;

        float dt = Time.fixedDeltaTime;
        Vector2 delta = _useVelocityOverride
            ? _velocityOverride * dt
            : _input * moveSpeed * dt;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = solidMask;
        filter.useTriggers = false;

        Vector2 currentCenterOffset = GetCurrentCenterOffset();
        if (!_hasPreviousCenterOffset)
        {
            _previousCenterOffset = currentCenterOffset;
            _hasPreviousCenterOffset = true;
        }

        Vector2 startCenter = _rb.position + _previousCenterOffset;
        Vector2 requestedCenterDisplacement = delta + currentCenterOffset - _previousCenterOffset;
        float worldRadius = PlayerSweepSlideMath.GetWorldRadius(_col.radius, transform.lossyScale);
        Vector2 resolvedCenterDisplacement = ResolveCenterDisplacement(
            startCenter,
            worldRadius,
            requestedCenterDisplacement,
            filter);

        Vector2 resolvedBodyPosition = startCenter + resolvedCenterDisplacement - currentCenterOffset;
        _rb.MovePosition(resolvedBodyPosition);
        _previousCenterOffset = currentCenterOffset;
    }

    private Vector2 ResolveCenterDisplacement(
        Vector2 startCenter,
        float radius,
        Vector2 requestedDisplacement,
        ContactFilter2D filter)
    {
        Vector2 resolved = Vector2.zero;
        Vector2 remaining = requestedDisplacement;
        Vector2 virtualCenter = startCenter;
        float effectiveSkin = Mathf.Max(0f, skin);

        for (int iteration = 0; iteration < MaxSlideIterations; iteration++)
        {
            float distance = remaining.magnitude;
            if (distance <= MinDisplacement)
            {
                break;
            }

            Vector2 direction = remaining / distance;
            int hitCount = Physics2D.CircleCast(
                virtualCenter,
                radius,
                direction,
                filter,
                _hits,
                distance + effectiveSkin);

            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (!IsBlockingHit(hit))
                {
                    continue;
                }

                nearestDistance = Mathf.Min(nearestDistance, Mathf.Max(0f, hit.distance));
            }

            if (float.IsPositiveInfinity(nearestDistance))
            {
                resolved += remaining;
                break;
            }

            int normalCount = 0;
            float contactLimit = nearestDistance + ContactMergeDistance;
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = _hits[i];
                if (!IsBlockingHit(hit) || hit.distance > contactLimit)
                {
                    continue;
                }

                PlayerSweepSlideMath.AddSortedUniqueNormal(
                    _contactNormals,
                    ref normalCount,
                    hit.normal);
            }

            float travelDistance = PlayerSweepSlideMath.GetAllowedTravelDistance(
                nearestDistance,
                distance,
                effectiveSkin,
                direction,
                _contactNormals,
                normalCount);
            Vector2 advance = direction * travelDistance;
            resolved += advance;
            virtualCenter += advance;

            Vector2 untraveled = remaining - advance;
            Vector2 clipped = PlayerSweepSlideMath.ClipAgainstNormals(
                untraveled,
                _contactNormals,
                normalCount);
            if (clipped.sqrMagnitude <= MinDisplacement * MinDisplacement)
            {
                break;
            }

            if (travelDistance <= MinDisplacement &&
                (clipped - remaining).sqrMagnitude <= MinDisplacement * MinDisplacement)
            {
                break;
            }

            remaining = clipped;
        }

        return resolved;
    }

    private bool IsBlockingHit(RaycastHit2D hit)
    {
        return hit.collider != null && hit.rigidbody != _rb;
    }

    private Vector2 GetCurrentCenterOffset()
    {
        return PlayerSweepSlideMath.GetWorldCenterOffset(
            _col.offset,
            transform.lossyScale,
            transform.eulerAngles.z);
    }

    private void RefreshCenterOffset()
    {
        if (_col == null)
        {
            return;
        }

        _previousCenterOffset = GetCurrentCenterOffset();
        _hasPreviousCenterOffset = true;
    }
}

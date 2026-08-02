using System.Collections.Generic;
using UnityEngine;
using Castlebound.Gameplay.AI;

[RequireComponent(typeof(EnemyTargeting))]
[RequireComponent(typeof(EnemyLocomotion))]
[RequireComponent(typeof(EnemyFacing))]
[RequireComponent(typeof(EnemyEngagement))]
public class EnemyController2D : MonoBehaviour
{
    public enum State
    {
        CHASE,
        HOLD
    }

    // Static registry for manager access
    internal static readonly List<EnemyController2D> All = new List<EnemyController2D>();

    [SerializeField] private Transform barrier;
    [SerializeField] private EnemyRegionState regionState;
    [SerializeField] private EnemyTargeting targeting;
    [SerializeField] private EnemyLocomotion locomotion;
    [SerializeField] private EnemyFacing facing;
    [SerializeField] private EnemyEngagement engagement;
    [SerializeField] private EnemyAnimationPresenter animationPresenter;

    [SerializeField] private float speed = 3.5f;
    [SerializeField] private float orbitBase = 0.8f;
    [SerializeField] private float maxTangent = 2.5f;
    [SerializeField] private int outrunFrames = 8;
    [SerializeField] private float epsilonDist = 0.01f;
    [SerializeField] private float reseatBias = 0.3f;
    [SerializeField] private EnemyApproachSpread approachSpread;
    [SerializeField] private EnemySurroundEligibility surroundEligibility;

    private EnemyTargeting Targeting
    {
        get
        {
            if (targeting == null)
                targeting = GetComponent<EnemyTargeting>();

            return targeting;
        }
    }

    private EnemyLocomotion Locomotion
    {
        get
        {
            if (locomotion == null)
                locomotion = GetComponent<EnemyLocomotion>();

            return locomotion;
        }
    }

    private EnemyFacing Facing
    {
        get
        {
            if (facing == null)
                facing = GetComponent<EnemyFacing>();

            return facing;
        }
    }

    private EnemyEngagement Engagement
    {
        get
        {
            if (engagement == null)
                engagement = GetComponent<EnemyEngagement>();

            return engagement;
        }
    }

    public Transform Target => Targeting != null ? Targeting.AttackTarget : null;
    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0f, value);
    }

    public EnemyTargetType CurrentTargetType => Targeting != null
        ? Targeting.CurrentTargetType
        : EnemyTargetType.None;

    // NEW: used by EnemyAttack to decide when we're close enough to attack.
    public bool IsInHoldRange()
    {
        return Locomotion.IsInHoldRange;
    }

    private Rigidbody2D _rb;
    private float _gapCW;
    private float _gapCCW;
    private int _surroundParticipantCount;
    private Vector2 _approachSeparation;
    private bool _hasApproachNeighbors;
    private Vector2 _approachSpreadBias;
    public bool IsChaseRequested => Locomotion.IsChaseRequested;

    public void SetAngularGaps(float gapCW, float gapCCW)
    {
        _gapCW = gapCW;
        _gapCCW = gapCCW;
    }

    public void SetAngularGaps(float gapCW, float gapCCW, int participantCount)
    {
        SetAngularGaps(gapCW, gapCCW);
        _surroundParticipantCount = Mathf.Max(0, participantCount);
    }

    public void SetApproachSeparation(Vector2 separation, bool hasNeighbors)
    {
        _approachSeparation = separation;
        _hasApproachNeighbors = hasNeighbors;
    }

    public float ApproachSeparationRadius => approachSpread != null
        ? approachSpread.NeighborSeparationRadius
        : 0f;

    public void RequestChase()
    {
        Locomotion.RequestChase();
    }

    public void ClearChaseRequest()
    {
        Locomotion.ClearChaseRequest();
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        targeting = GetComponent<EnemyTargeting>();
        locomotion = GetComponent<EnemyLocomotion>();
        facing = GetComponent<EnemyFacing>();
        engagement = GetComponent<EnemyEngagement>();
        animationPresenter = GetComponent<EnemyAnimationPresenter>();
        if (approachSpread == null)
        {
            approachSpread = GetComponent<EnemyApproachSpread>();
        }
        if (surroundEligibility == null)
        {
            surroundEligibility = GetComponent<EnemySurroundEligibility>();
        }
        if (regionState == null)
        {
            regionState = GetComponent<EnemyRegionState>();
        }

        Targeting.Initialize();

        // Home barrier assignment deferred to Start to ensure barriers are registered.
    }

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
        if (_approachSpreadBias == Vector2.zero)
        {
            float angle = All.Count * 137.50776f * Mathf.Deg2Rad;
            _approachSpreadBias = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    private void OnDisable()
    {
        All.Remove(this);
        _gapCW = 0f;
        _gapCCW = 0f;
        _surroundParticipantCount = 0;
        _approachSeparation = Vector2.zero;
        _hasApproachNeighbors = false;
    }

    private void Start()
    {
        Targeting.RequestRetarget();
    }

    private void FixedUpdate()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null) return;
        }

        if (regionState == null)
        {
            regionState = GetComponent<EnemyRegionState>();
        }

        bool enemyInsideCastle = regionState != null && regionState.EnemyInside;
        bool playerInsideCastle = regionState != null && regionState.PlayerInside;

        Targeting.Refresh(_rb.position, playerInsideCastle, enemyInsideCastle);
        Transform steerTarget = Targeting.SteerTarget;
        Transform target = Targeting.AttackTarget;
        Transform player = Targeting.Player;
        if (CurrentTargetType == EnemyTargetType.Barrier)
            barrier = steerTarget;

        Vector2 pos = _rb.position;
        float dt = Time.fixedDeltaTime;
        Facing.FaceTarget(pos, target, dt);

        if (target == null)
        {
            bool moved = Locomotion.ExecuteMovement(_rb, Vector2.zero, Vector2.zero, dt);
            animationPresenter?.SetMovementRequested(moved);
            return;
        }
        if (steerTarget == null) steerTarget = target;
        float surfaceDistance = Engagement.SurfaceDistanceTo(target);
        Locomotion.ComputeBaseMovement(
            pos,
            steerTarget,
            barrier,
            surfaceDistance,
            Engagement.EngagementDistance,
            Engagement.ReleaseMargin,
            reseatBias,
            speed,
            orbitBase,
            maxTangent,
            outrunFrames,
            epsilonDist,
            _gapCW,
            _gapCCW,
            out Vector2 radial,
            out Vector2 tangent);

        if (Locomotion.CurrentState == State.CHASE &&
            !Locomotion.IsChaseRequested &&
            CurrentTargetType == EnemyTargetType.Player &&
            approachSpread != null &&
            surroundEligibility != null &&
            surroundEligibility.IsEligibleFor(player))
        {
            Vector2 toPlayer = player != null ? (Vector2)player.position - pos : Vector2.zero;
            float distanceToPlayer = toPlayer.magnitude;
            Vector2 directionToPlayer = distanceToPlayer > 0f ? toPlayer / distanceToPlayer : Vector2.zero;
            approachSpread.Compute(
                radial,
                directionToPlayer,
                _approachSeparation,
                _hasApproachNeighbors,
                _approachSpreadBias,
                surfaceDistance,
                Engagement.EngagementDistance,
                _gapCW,
                _gapCCW,
                _surroundParticipantCount > 1,
                speed,
                out radial,
                out tangent);
        }

        if (Locomotion.IsChaseRequested && steerTarget != null)
        {
            Vector2 toTarget = (Vector2)steerTarget.position - pos;
            radial = toTarget.sqrMagnitude > 0f ? toTarget.normalized * speed : Vector2.zero;
            tangent = Vector2.zero;
            Locomotion.SetMovementState(State.CHASE);
        }

        Locomotion.ApplyHoldMovementPolicy(
            pos,
            target,
            _approachSeparation,
            _hasApproachNeighbors,
            _approachSpreadBias,
            speed,
            ref radial,
            ref tangent);

        bool movementApplied = Locomotion.ExecuteMovement(_rb, radial, tangent, dt);
        animationPresenter?.SetMovementRequested(movementApplied);
    }

    private Transform SelectTarget(bool playerInside, bool enemyInside)
    {
        Targeting.Refresh(transform.position, playerInside, enemyInside);
        return Targeting.AttackTarget;
    }

    private Transform SelectSteerTarget(bool playerInside, bool enemyInside)
    {
        Targeting.Refresh(transform.position, playerInside, enemyInside);
        return Targeting.SteerTarget;
    }

    public Transform Debug_SelectTarget(bool playerInside, bool enemyInside)
    {
        Targeting.RequestRetarget();
        return SelectTarget(playerInside, enemyInside);
    }

    public Transform Debug_SteerTarget(bool playerInside, bool enemyInside)
    {
        Targeting.RequestRetarget();
        return SelectSteerTarget(playerInside, enemyInside);
    }

    // Debug/test helper to ensure player reference via tag/lookup.
    public void Debug_EnsurePlayerReference()
    {
        Targeting.Initialize();
    }

    // Test helper: force references for deterministic behavior in EditMode tests.
    public void Debug_SetupRefs(Transform playerRef, Transform homeRef = null)
    {
        Targeting.Debug_Setup(playerRef, homeRef);
        barrier = homeRef;
        barrier = Targeting.SelectedBarrier;
    }

    public void Debug_SetTargetDecision(Transform steer, Transform attack, EnemyTargetType targetType)
    {
        Targeting.Debug_SetDecision(steer, attack, targetType);
    }

    public void RequestTargetRefresh()
    {
        Targeting?.RequestRetarget();
    }

    public void Debug_SetBarrierTargeting(bool value)
    {
        Targeting.Debug_SetUseBarrierTargeting(value);
    }

#if UNITY_EDITOR
    // Editor-only validation helper to surface missing refs without relying on Unity lifecycle.
    public void Debug_ValidateRefs()
    {
        if (Targeting == null || Targeting.Player == null)
        {
            Debug.LogWarning("[EnemyController2D] Player reference not found in scene. Enemy will have no target.", this);
        }

        if (Targeting != null && Targeting.UsesBarrierTargeting && Targeting.SelectedBarrier == null)
        {
            Debug.LogWarning("[EnemyController2D] No barrier target found while barrier targeting is enabled.", this);
        }
    }
#endif

    private void OnDrawGizmosSelected()
    {
        if (Engagement == null)
            return;

        Gizmos.color = new Color(0.3f, 0.9f, 0.3f, 1f);
        Gizmos.DrawWireSphere(transform.position, Engagement.EngagementDistance);
        Gizmos.color = new Color(0.9f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireSphere(
            transform.position,
            Engagement.EngagementDistance + Engagement.ReleaseMargin);
    }

#if UNITY_EDITOR
    // Editor-only helper to compute movement vectors without moving.
    public void Debug_ComputeMovement(out Vector2 radial, out Vector2 tangent)
    {
        radial = Vector2.zero;
        tangent = Vector2.zero;
        if (_rb == null) return;

        Locomotion.ComputeBaseMovement(
            _rb.position,
            Targeting != null ? Targeting.SteerTarget : null,
            barrier,
            Engagement.SurfaceDistanceTo(Targeting != null ? Targeting.AttackTarget : null),
            Engagement.EngagementDistance,
            Engagement.ReleaseMargin,
            reseatBias,
            speed,
            orbitBase,
            maxTangent,
            outrunFrames,
            epsilonDist,
            _gapCW,
            _gapCCW,
            out radial,
            out tangent);
        Locomotion.ApplyHoldMovementPolicy(
            _rb.position,
            Targeting != null ? Targeting.AttackTarget : null,
            _approachSeparation,
            _hasApproachNeighbors,
            _approachSpreadBias,
            speed,
            ref radial,
            ref tangent);
    }
#endif
}

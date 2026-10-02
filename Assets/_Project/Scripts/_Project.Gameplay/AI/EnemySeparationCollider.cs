using Castlebound.Gameplay.AI;
using UnityEngine;

// The authored child owns geometry/registration only. Discovery does not depend on triggers.
[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class EnemySeparationCollider : MonoBehaviour
{
    [SerializeField] private CircleCollider2D separationCollider;
    private Rigidbody2D body;
    private EnemyController2D owner;
    private EnemyLocomotion locomotion;
    private EnemyRootReceiver root;
    private EnemyStaggerReceiver stagger;
    private EnemyNavigationChase navigation;
    private EnemyAnimationPresenter animationPresenter;
    private EnemySeparationWorldGuard worldGuard;
    private Vector2 desired, external;
    private bool pending;
    private bool reportedMovement;
    public CircleCollider2D Collider => separationCollider != null ? separationCollider : (separationCollider = GetComponent<CircleCollider2D>());
    public Rigidbody2D Body { get { Cache(); return body; } }
    public EnemyController2D Owner { get { Cache(); return owner; } }
    public bool Participates
    {
        get { Cache(); return isActiveAndEnabled && Collider.enabled && body != null && body.simulated && body.bodyType == RigidbodyType2D.Dynamic; }
    }
    private bool Locked => root != null && root.IsRooted || stagger != null && stagger.IsActionLocked;
#if UNITY_EDITOR
    // Kept for existing regression/trace consumers: direct contact correction is gone.
    public int DebugFallbackCorrectionCount => 0;
    public int DebugMovesLastStep { get; private set; }
    public Vector2 DebugRequestedStep { get; private set; }
    public Vector2 DebugAppliedStep { get; private set; }
    public bool DebugDiscoverySaturated { get; private set; }
    public void Debug_ResetFallbackCorrectionCount() { }
#endif

    private void Awake() => Cache();
    private void OnEnable() { Cache(); EnemySeparationCoordinator.Register(this); }
    private void OnDisable() { pending = reportedMovement = false; desired = external = Vector2.zero; EnemySeparationCoordinator.Unregister(this); }

    public bool Submit(Vector2 displacement, EnemyKnockbackReceiver knockback, float dt)
    {
        Cache();
        if (!Participates || Locked) { pending = false; desired = external = Vector2.zero; return false; }
        // Repeated submissions replace intent; an impulse is consumed only once for this application.
        if (!pending) external = knockback != null ? knockback.ConsumeDisplacement(dt) : Vector2.zero;
        desired = displacement; pending = true;
        return desired.sqrMagnitude + external.sqrMagnitude > Mathf.Epsilon;
    }

    public EnemySeparationBody Snapshot(float dt)
    {
        Cache();
        bool locked = Locked;
        float allowance = owner != null ? owner.Speed * dt : (pending ? desired.magnitude : 0f);
        // Settled HOLD does not acquire autonomous recovery movement. Explicit HOLD movement
        // policies retain their own proposed allowance; root/stagger always have zero authority.
        if (locomotion != null && locomotion.IsInHoldRange) allowance = Mathf.Min(allowance, pending ? desired.magnitude : 0f);
#if UNITY_EDITOR
        DebugMovesLastStep = 0; DebugRequestedStep = pending ? desired : Vector2.zero;
#endif
        return new EnemySeparationBody { Id = GetInstanceID(), Position = Collider.bounds.center,
            Radius = Collider.bounds.extents.x, Budget = locked ? 0f : Mathf.Max(0f, allowance), Locked = locked,
            Desired = pending && !locked ? desired : Vector2.zero, External = pending && !locked ? external : Vector2.zero };
    }

    public Vector2 Guard(Vector2 displacement)
    {
        // Navigation recovery has its own contact-start rules. Ordinary and standalone
        // recovery steps also use a primary-body sweep (including Player collision).
        Vector2 allowed = navigation != null && navigation.isActiveAndEnabled
            ? navigation.ConstrainFinalDisplacement(displacement) : displacement;
        if (navigation == null || !navigation.GuardMovement || !navigation.IsRecovering)
            allowed = worldGuard.Constrain(allowed);
        return allowed;
    }

    public void Apply(EnemySeparationBody step, bool saturated)
    {
        if (!Participates) { pending = false; desired = external = Vector2.zero; return; }
        Vector2 ordinary = Locked ? Vector2.zero : step.Displacement;
        Vector2 impulse = Locked ? Vector2.zero : step.External;
        body.MovePosition(body.position + ordinary + impulse);
        bool moved = (ordinary + impulse).sqrMagnitude > Mathf.Epsilon;
        // Own presentation only for submitted movement or recovery we actually applied.
        // Send one settling update after owned motion; idle registered sensors must not
        // overwrite independent animation requests on controller-disabled actors.
        if (pending || moved || reportedMovement) animationPresenter?.SetMovementRequested(moved);
        reportedMovement = moved;
#if UNITY_EDITOR
        DebugMovesLastStep = 1; DebugAppliedStep = ordinary; DebugDiscoverySaturated = saturated;
        if (navigation != null) navigation.Debug_RecordAppliedLocomotion(ordinary);
#endif
        pending = false; desired = external = Vector2.zero;
    }

    private void Cache()
    {
        if (body == null) body = Collider.attachedRigidbody;
        if (body == null) return;
        if (owner == null) owner = body.GetComponent<EnemyController2D>();
        if (locomotion == null) locomotion = body.GetComponent<EnemyLocomotion>();
        if (root == null) root = body.GetComponent<EnemyRootReceiver>();
        if (stagger == null) stagger = body.GetComponent<EnemyStaggerReceiver>();
        if (navigation == null) navigation = body.GetComponent<EnemyNavigationChase>();
        if (animationPresenter == null) animationPresenter = body.GetComponent<EnemyAnimationPresenter>();
        if (worldGuard == null) worldGuard = new EnemySeparationWorldGuard(body.GetComponent<CircleCollider2D>());
    }
}

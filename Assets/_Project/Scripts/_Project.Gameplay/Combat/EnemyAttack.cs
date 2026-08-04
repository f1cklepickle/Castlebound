using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using UnityEngine;

[RequireComponent(typeof(EnemyController2D))]
[RequireComponent(typeof(EnemyFacing))]
[RequireComponent(typeof(EnemyEngagement))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private MonoBehaviour attackDeliverySource;
    [SerializeField] private float windupSeconds = 0.3f;
    [SerializeField] private float cooldownSeconds = 0.8f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private string playerLayerName = "Player";

    private readonly AttackClock attackClock = new AttackClock();
    private EnemyController2D controller;
    private EnemyFacing facing;
    private EnemyEngagement engagement;
    private EnemyRegionState regionState;
    private EnemyRootReceiver rootReceiver;
    private EnemyAnimationPresenter animationPresenter;
    private EnemyEquipment equipment;
    private IEnemyAttackDelivery attackDelivery;
    private Transform lockedTarget;
    private EnemyEquipmentDefinition equipmentDefinitionSnapshot;
    private CombatEquipmentSnapshot combatEquipmentSnapshot;
    private bool impactDelivered;
    private static bool missingRegionStateWarningLogged;

    public int Damage
    {
        get
        {
            var meleeDelivery = EnemyAttackDeliveryResolver.ResolveMeleeForStats(
                gameObject,
                attackDeliverySource);
            return meleeDelivery != null ? meleeDelivery.Damage : 0;
        }
        set
        {
            var meleeDelivery = EnemyAttackDeliveryResolver.ResolveMeleeForStats(
                gameObject,
                attackDeliverySource);
            if (meleeDelivery != null)
                meleeDelivery.Damage = value;
        }
    }

    public float CooldownSeconds
    {
        get => cooldownSeconds;
        set => cooldownSeconds = Mathf.Max(0f, value);
    }

    public float CurrentAttackRate => attackClock.CurrentSwing.AttackRate;
    public float NormalizedAttackProgress => attackClock.NormalizedProgress;
    public bool IsAttackActive => attackClock.IsRunning;

    private void Awake()
    {
        controller = GetComponent<EnemyController2D>();
        facing = GetComponent<EnemyFacing>();
        engagement = GetComponent<EnemyEngagement>();
        regionState = GetComponent<EnemyRegionState>();
        rootReceiver = GetComponent<EnemyRootReceiver>();
        animationPresenter = GetComponent<EnemyAnimationPresenter>();
        equipment = GetComponent<EnemyEquipment>();
        attackDelivery = EnemyAttackDeliveryResolver.Resolve(gameObject, ref attackDeliverySource);
        EnsureTargetMask();
    }

    private void Update()
    {
        if (attackClock.IsRunning)
        {
            AdvanceAttack(Time.deltaTime);
            return;
        }

        if (TryBeginAttack())
            AdvanceAttack(Time.deltaTime);
    }

    private void OnDisable()
    {
        CancelCurrentAttack(requestChase: false);
    }

    private bool TryBeginAttack()
    {
        if (controller == null || IsRooted())
            return false;

        Transform selectedTarget = controller.Target;
        if (controller.IsChaseRequested)
        {
            if (selectedTarget == null || !IsTargetInReach(selectedTarget))
                return false;
            controller.ClearChaseRequest();
        }

        if (controller.CurrentTargetType == EnemyTargetType.Barrier)
        {
            GetRegionState(out bool enemyInside, out bool playerInside);
            if (!CanDamageBarrier(enemyInside, playerInside))
                return false;
        }

        if (selectedTarget == null)
            return false;

        bool isInReach = IsTargetInReach(selectedTarget);
        bool isAligned = facing != null && facing.IsAlignedWith(transform.position, selectedTarget);
        if (!IsAttackEligible(controller.IsInHoldRange(), isInReach, isAligned))
            return false;

        EnemyEquipmentDefinition nextDefinitionSnapshot = CaptureEquipmentSnapshot();
        CombatEquipmentSnapshot nextCombatSnapshot = CaptureCombatEquipmentSnapshot();
        if (attackDelivery == null ||
            !attackDelivery.CanDeliver(selectedTarget, nextDefinitionSnapshot, nextCombatSnapshot))
        {
            return false;
        }

        lockedTarget = selectedTarget;
        equipmentDefinitionSnapshot = nextDefinitionSnapshot;
        combatEquipmentSnapshot = nextCombatSnapshot;
        impactDelivered = false;
        attackClock.Start(
            combatEquipmentSnapshot.AttackRate,
            new AttackPhaseProfile(windupSeconds, 0f, cooldownSeconds));
        animationPresenter?.PlayAttack(
            attackClock.CurrentSwing.WindupDuration,
            attackClock.CurrentSwing.Duration);
        return true;
    }

    private void AdvanceAttack(float deltaTime)
    {
        float remainingDelta = NormalizeDelta(deltaTime);
        while (attackClock.IsRunning)
        {
            if (!impactDelivered && !IsPreImpactStateValid())
            {
                CancelCurrentAttack(requestChase: true);
                return;
            }

            AttackClockStep step = attackClock.Advance(remainingDelta);
            animationPresenter?.ApplyAttackProgress(attackClock.NormalizedProgress);
            if (step.ImpactOccurred)
            {
                if (!IsPreImpactStateValid() ||
                    attackDelivery == null ||
                    !attackDelivery.TryDeliver(
                        lockedTarget,
                        equipmentDefinitionSnapshot,
                        combatEquipmentSnapshot))
                {
                    CancelCurrentAttack(requestChase: true);
                    return;
                }

                impactDelivered = true;
            }

            if (!step.SwingCompleted)
                return;

            CompleteCurrentAttack();
            remainingDelta = step.UnusedDeltaTime;
            if (remainingDelta <= 0f || !TryBeginAttack())
                return;
        }
    }

    private bool IsPreImpactStateValid()
    {
        bool targetInReach = IsTargetInReach(lockedTarget);
        bool targetAligned = facing != null && facing.IsAlignedWith(transform.position, lockedTarget);
        return !IsRooted()
            && IsLockedTargetValid(
                lockedTarget,
                controller != null ? controller.Target : null,
                targetInReach)
            && targetAligned;
    }

    private void CompleteCurrentAttack()
    {
        animationPresenter?.CompleteAttack();
        ClearAttackSnapshot();
    }

    private void CancelCurrentAttack(bool requestChase)
    {
        if (attackClock.IsRunning)
            animationPresenter?.CancelAttack();

        attackClock.Cancel();
        ClearAttackSnapshot();
        if (requestChase)
            controller?.RequestChase();
    }

    private void ClearAttackSnapshot()
    {
        lockedTarget = null;
        equipmentDefinitionSnapshot = null;
        combatEquipmentSnapshot = default;
        impactDelivered = false;
    }

    public static float CalculateBaseAttackRate(float windupDuration, float cooldownDuration)
    {
        float cycleDuration = Mathf.Max(0f, windupDuration) + Mathf.Max(0f, cooldownDuration);
        return AttackRatePolicy.Normalize(1f / cycleDuration);
    }

    public static bool IsLockedTargetValid(Transform lockedAttackTarget, Transform selectedTarget, bool isInReach)
    {
        return lockedAttackTarget != null && lockedAttackTarget == selectedTarget && isInReach;
    }

    public static bool RequiresCompletedCooldown(bool attackCompleted) => attackCompleted;

    public static bool IsAttackEligible(bool isInHoldRange, bool isInReach, bool isAligned)
    {
        return isInHoldRange && isInReach && isAligned;
    }

    private bool IsTargetInReach(Transform selectedTarget)
    {
        return engagement != null && engagement.IsWithinEngagementDistance(selectedTarget);
    }

    private void OnDrawGizmosSelected()
    {
        if (engagement == null)
            engagement = GetComponent<EnemyEngagement>();
        if (engagement == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagement.EngagementDistance);
    }

    public void DealDamage(IDamageable target)
    {
        EnemyAttackDeliveryResolver.GetOrCreateMelee(gameObject).TryDealDamage(target);
    }

    public EnemyEquipmentDefinition CaptureEquipmentSnapshot()
    {
        if (equipment == null)
            equipment = GetComponent<EnemyEquipment>();

        return equipment != null ? equipment.ActiveEquipment : null;
    }

    public CombatEquipmentSnapshot CaptureCombatEquipmentSnapshot()
    {
        if (attackDelivery == null)
        {
            attackDelivery = EnemyAttackDeliveryResolver.Resolve(
                gameObject,
                ref attackDeliverySource);
        }

        float baseRate = CalculateBaseAttackRate(windupSeconds, cooldownSeconds);
        EnemyAttackRole attackRole = attackDelivery != null
            ? attackDelivery.AttackRole
            : EnemyAttackRole.None;
        return EnemyAttackEquipmentSnapshotResolver.Resolve(
            CaptureEquipmentSnapshot(),
            Damage,
            baseRate,
            attackRole);
    }

    public MonoBehaviour AttackDeliverySource
    {
        get
        {
            if (attackDeliverySource == null)
                attackDelivery = EnemyAttackDeliveryResolver.Resolve(
                    gameObject,
                    ref attackDeliverySource);
            return attackDeliverySource;
        }
        set
        {
            attackDeliverySource = value;
            attackDelivery = value as IEnemyAttackDelivery;
        }
    }

    public static bool CanDamageBarrier(bool enemyInside, bool playerInside)
    {
        if (!enemyInside)
            return true;
        return !playerInside;
    }

    private void GetRegionState(out bool enemyInside, out bool playerInside)
    {
        if (regionState == null)
            regionState = GetComponent<EnemyRegionState>();

        if (regionState == null)
        {
            if (!missingRegionStateWarningLogged)
            {
                Debug.LogWarning(
                    "[EnemyAttack] EnemyRegionState is missing; treating enemy/player as outside for barrier gating.",
                    this);
                missingRegionStateWarningLogged = true;
            }
            enemyInside = false;
            playerInside = false;
            return;
        }

        enemyInside = regionState.EnemyInside;
        playerInside = regionState.PlayerInside;
    }

    private bool IsRooted()
    {
        if (rootReceiver == null)
            rootReceiver = GetComponent<EnemyRootReceiver>();
        return rootReceiver != null && rootReceiver.IsRooted;
    }

    private void EnsureTargetMask()
    {
        if (targetMask.value != 0)
            return;

        int layer = LayerMask.NameToLayer(playerLayerName);
        if (layer >= 0)
            targetMask = LayerMask.GetMask(playerLayerName);
    }

    private static float NormalizeDelta(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f)
            return 0f;
        return deltaTime;
    }

#if UNITY_EDITOR
    public void Debug_GetRegionState(out bool enemyInside, out bool playerInside) =>
        GetRegionState(out enemyInside, out playerInside);
    public static void Debug_ResetMissingRegionWarning() => missingRegionStateWarningLogged = false;
    public void Debug_EnsureTargetMask() => EnsureTargetMask();
#endif
}

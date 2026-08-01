using UnityEngine;
using System.Collections;
using Castlebound.Gameplay.AI;

[RequireComponent(typeof(EnemyController2D))]
[RequireComponent(typeof(EnemyFacing))]
[RequireComponent(typeof(EnemyEngagement))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] MonoBehaviour attackDeliverySource;
    public int Damage
    {
        get
        {
            var meleeDelivery = ResolveMeleeDeliveryForStats();
            return meleeDelivery != null ? meleeDelivery.Damage : 0;
        }
        set
        {
            var meleeDelivery = ResolveMeleeDeliveryForStats();
            if (meleeDelivery != null)
            {
                meleeDelivery.Damage = value;
            }
        }
    }
    [SerializeField] float windupSeconds = 0.3f;   // time before damage applies
    [SerializeField] float cooldownSeconds = 0.8f; // time between attacks
    public float CooldownSeconds
    {
        get => cooldownSeconds;
        set => cooldownSeconds = Mathf.Max(0f, value);
    }

    [SerializeField] LayerMask targetMask;         // set to Player layer in Inspector
    [SerializeField] string playerLayerName = "Player";

    EnemyController2D controller;
    EnemyFacing facing;
    EnemyEngagement engagement;
    EnemyRegionState regionState;
    EnemyRootReceiver rootReceiver;
    EnemyAnimationPresenter animationPresenter;
    EnemyEquipment equipment;
    IEnemyAttackDelivery attackDelivery;
    static bool missingRegionStateWarningLogged;
    bool onCooldown;

    void Awake()
    {
        controller = GetComponent<EnemyController2D>();
        facing = GetComponent<EnemyFacing>();
        engagement = GetComponent<EnemyEngagement>();
        regionState = GetComponent<EnemyRegionState>();
        rootReceiver = GetComponent<EnemyRootReceiver>();
        animationPresenter = GetComponent<EnemyAnimationPresenter>();
        equipment = GetComponent<EnemyEquipment>();
        attackDelivery = ResolveAttackDelivery();
        if (targetMask.value == 0) {
            int lm = LayerMask.NameToLayer(playerLayerName);
            if (lm >= 0) {
                targetMask = LayerMask.GetMask(playerLayerName);
            }
        }
    }

    void Update()
    {
        if (onCooldown || controller == null) return;
        if (IsRooted()) return;

        Transform selectedTarget = controller.Target;
        if (controller.IsChaseRequested)
        {
            if (selectedTarget == null || !IsTargetInReach(selectedTarget)) return;
            controller.ClearChaseRequest();
        }

        // Gate barrier damage by inside/outside state when targeting a barrier.
        if (controller.CurrentTargetType == EnemyTargetType.Barrier)
        {
            GetRegionState(out bool enemyInside, out bool playerInside);
            if (!CanDamageBarrier(enemyInside, playerInside))
                return;
        }

        if (selectedTarget == null) return;

        bool isInReach = IsTargetInReach(selectedTarget);
        bool isAligned = facing != null && facing.IsAlignedWith(transform.position, selectedTarget);
        if (!IsAttackEligible(controller.IsInHoldRange(), isInReach, isAligned)) return;

        var equipmentSnapshot = CaptureEquipmentSnapshot();
        if (attackDelivery == null || !attackDelivery.CanDeliver(selectedTarget, equipmentSnapshot)) return;

        StartCoroutine(AttackRoutine(selectedTarget, equipmentSnapshot));
    }

    IEnumerator AttackRoutine(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        onCooldown = true;

        animationPresenter?.PlayAttack(windupSeconds);

        yield return new WaitForSeconds(windupSeconds);

        bool targetInReach = IsTargetInReach(lockedTarget);
        bool targetAligned = facing != null && facing.IsAlignedWith(transform.position, lockedTarget);
        if (!IsLockedTargetValid(lockedTarget, controller != null ? controller.Target : null, targetInReach) ||
            !targetAligned)
        {
            CancelWindup();
            yield break;
        }

        if (attackDelivery == null || !attackDelivery.TryDeliver(lockedTarget, equipmentSnapshot))
        {
            CancelWindup();
            yield break;
        }

        if (RequiresCompletedCooldown(attackCompleted: true))
            yield return new WaitForSeconds(cooldownSeconds);

        animationPresenter?.CompleteAttack();
        onCooldown = false;
    }

    public static bool IsLockedTargetValid(Transform lockedTarget, Transform selectedTarget, bool isInReach)
    {
        return lockedTarget != null && lockedTarget == selectedTarget && isInReach;
    }

    public static bool RequiresCompletedCooldown(bool attackCompleted)
    {
        return attackCompleted;
    }

    public static bool IsAttackEligible(bool isInHoldRange, bool isInReach, bool isAligned)
    {
        return isInHoldRange && isInReach && isAligned;
    }

    private bool IsTargetInReach(Transform selectedTarget)
    {
        return engagement != null && engagement.IsWithinEngagementDistance(selectedTarget);
    }

    private void CancelWindup()
    {
        animationPresenter?.CancelAttack();

        onCooldown = false;
        controller?.RequestChase();
    }

    void OnDrawGizmosSelected()
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
        GetOrCreateMeleeDelivery().TryDealDamage(target);
    }

    public EnemyEquipmentDefinition CaptureEquipmentSnapshot()
    {
        if (equipment == null)
        {
            equipment = GetComponent<EnemyEquipment>();
        }

        return equipment != null ? equipment.ActiveEquipment : null;
    }

    public MonoBehaviour AttackDeliverySource
    {
        get
        {
            if (attackDeliverySource == null)
            {
                attackDelivery = ResolveAttackDelivery();
            }

            return attackDeliverySource;
        }
        set
        {
            attackDeliverySource = value;
            attackDelivery = value as IEnemyAttackDelivery;
        }
    }

    // Barrier damage gate: allow if enemy outside, or enemy inside while player is outside.
    public static bool CanDamageBarrier(bool enemyInside, bool playerInside)
    {
        if (!enemyInside)
            return true;

        return !playerInside;
    }

    private void GetRegionState(out bool enemyInside, out bool playerInside)
    {
        if (regionState == null)
        {
            regionState = GetComponent<EnemyRegionState>();
        }

        if (regionState == null)
        {
            if (!missingRegionStateWarningLogged)
            {
                Debug.LogWarning("[EnemyAttack] EnemyRegionState is missing; treating enemy/player as outside for barrier gating.", this);
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
        {
            rootReceiver = GetComponent<EnemyRootReceiver>();
        }

        return rootReceiver != null && rootReceiver.IsRooted;
    }

    private IEnemyAttackDelivery ResolveAttackDelivery()
    {
        if (attackDeliverySource is IEnemyAttackDelivery configuredDelivery)
        {
            return configuredDelivery;
        }

        var behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IEnemyAttackDelivery delivery)
            {
                attackDeliverySource = behaviours[i];
                return delivery;
            }
        }

        var meleeDelivery = GetOrCreateMeleeDelivery();
        attackDeliverySource = meleeDelivery;
        return meleeDelivery;
    }

    private EnemyMeleeAttackDelivery GetOrCreateMeleeDelivery()
    {
        var meleeDelivery = GetComponent<EnemyMeleeAttackDelivery>();
        return meleeDelivery != null ? meleeDelivery : gameObject.AddComponent<EnemyMeleeAttackDelivery>();
    }

    private EnemyMeleeAttackDelivery ResolveMeleeDeliveryForStats()
    {
        if (attackDeliverySource is IEnemyAttackDelivery && !(attackDeliverySource is EnemyMeleeAttackDelivery))
        {
            return null;
        }

        return GetOrCreateMeleeDelivery();
    }

#if UNITY_EDITOR
    // Test helpers (Editor-only)
    public void Debug_GetRegionState(out bool enemyInside, out bool playerInside) => GetRegionState(out enemyInside, out playerInside);
    public static void Debug_ResetMissingRegionWarning() => missingRegionStateWarningLogged = false;
    public void Debug_EnsureTargetMask()
    {
        if (targetMask.value == 0)
        {
            int lm = LayerMask.NameToLayer(playerLayerName);
            if (lm >= 0)
            {
                targetMask = LayerMask.GetMask(playerLayerName);
            }
        }
    }
#endif
}

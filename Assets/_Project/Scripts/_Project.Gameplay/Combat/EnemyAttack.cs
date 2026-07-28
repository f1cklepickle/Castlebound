using UnityEngine;
using System.Collections;
using Castlebound.Gameplay.AI;

[RequireComponent(typeof(EnemyController2D))]
[RequireComponent(typeof(EnemyFacing))]
[RequireComponent(typeof(EnemyEngagement))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int damage = 1;
    public int Damage
    {
        get => damage;
        set => damage = value;
    }
    [SerializeField] float windupSeconds = 0.15f;  // time before damage applies
    [SerializeField] float cooldownSeconds = 0.8f; // time between attacks
    public float CooldownSeconds
    {
        get => cooldownSeconds;
        set => cooldownSeconds = Mathf.Max(0f, value);
    }

    [SerializeField] LayerMask targetMask;         // set to Player layer in Inspector
    [SerializeField] Animator animator;            // optional, can be null
    [SerializeField] FeedbackEventChannel enemyHitBarrierFeedbackChannel;
    [SerializeField] string playerLayerName = "Player";
    [SerializeField] string attackTriggerName = "Attack"; // matches goblin anim if you add one

    EnemyController2D controller;
    EnemyFacing facing;
    EnemyEngagement engagement;
    EnemyRegionState regionState;
    EnemyRootReceiver rootReceiver;
    static bool missingRegionStateWarningLogged;
    bool onCooldown;

    void Awake()
    {
        controller = GetComponent<EnemyController2D>();
        facing = GetComponent<EnemyFacing>();
        engagement = GetComponent<EnemyEngagement>();
        regionState = GetComponent<EnemyRegionState>();
        rootReceiver = GetComponent<EnemyRootReceiver>();
        if (!animator) animator = GetComponentInChildren<Animator>();
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

        StartCoroutine(AttackRoutine(selectedTarget));
    }

    IEnumerator AttackRoutine(Transform lockedTarget)
    {
        onCooldown = true;

        if (animator && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        yield return new WaitForSeconds(windupSeconds);

        bool targetInReach = IsTargetInReach(lockedTarget);
        bool targetAligned = facing != null && facing.IsAlignedWith(transform.position, lockedTarget);
        if (!IsLockedTargetValid(lockedTarget, controller != null ? controller.Target : null, targetInReach) ||
            !targetAligned)
        {
            CancelWindup();
            yield break;
        }

        IDamageable damageable = ResolveDamageable(lockedTarget);
        if (damageable == null)
        {
            CancelWindup();
            yield break;
        }

        Debug.Log($"[EnemyAttack] Hit locked target: {lockedTarget.name}, damage: {Damage}", this);
        DealDamage(damageable);

        if (RequiresCompletedCooldown(attackCompleted: true))
            yield return new WaitForSeconds(cooldownSeconds);

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

    private static IDamageable ResolveDamageable(Transform lockedTarget)
    {
        if (lockedTarget == null)
            return null;

        var damageable = lockedTarget.GetComponent<IDamageable>();
        if (damageable != null)
            return damageable;

        damageable = lockedTarget.GetComponentInParent<IDamageable>();
        return damageable ?? lockedTarget.GetComponentInChildren<IDamageable>();
    }

    private void CancelWindup()
    {
        if (animator && !string.IsNullOrEmpty(attackTriggerName))
            animator.ResetTrigger(attackTriggerName);

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
        if (target == null || Damage <= 0 || IsRooted())
        {
            return;
        }

        if (target is BarrierHealth)
        {
            GetRegionState(out bool enemyInside, out bool playerInside);
            if (!CanDamageBarrier(enemyInside, playerInside))
            {
                return;
            }
        }

        target.TakeDamage(Damage);

        if (enemyHitBarrierFeedbackChannel != null && target is BarrierHealth barrier)
        {
            enemyHitBarrierFeedbackChannel.Raise(new FeedbackCue(FeedbackCueType.EnemyHitBarrier, barrier.transform.position, barrier.gameObject.GetInstanceID()));
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

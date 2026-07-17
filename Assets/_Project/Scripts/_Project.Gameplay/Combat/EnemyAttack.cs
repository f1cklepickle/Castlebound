using UnityEngine;
using System.Collections;
using Castlebound.Gameplay.AI;

[RequireComponent(typeof(EnemyController2D))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int damage = 1;
    public int Damage
    {
        get => damage;
        set => damage = value;
    }
    [SerializeField] float attackRange = 1.2f;     // should be >= stopDistance
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
    EnemyRegionState regionState;
    EnemyRootReceiver rootReceiver;
    static bool missingRegionStateWarningLogged;
    bool onCooldown;

    void Awake()
    {
        controller = GetComponent<EnemyController2D>();
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

        // Only attack while we're holding position near the player
        if (!controller.IsInHoldRange()) return;

        // Gate barrier damage by inside/outside state when targeting a barrier.
        if (controller.CurrentTargetType == EnemyTargetType.Barrier)
        {
            GetRegionState(out bool enemyInside, out bool playerInside);
            if (!CanDamageBarrier(enemyInside, playerInside))
                return;
        }

        if (selectedTarget == null || !IsTargetInReach(selectedTarget)) return;

        StartCoroutine(AttackRoutine(selectedTarget));
    }

    IEnumerator AttackRoutine(Transform lockedTarget)
    {
        onCooldown = true;

        if (animator && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        yield return new WaitForSeconds(windupSeconds);

        bool targetInReach = IsTargetInReach(lockedTarget);
        if (!IsLockedTargetValid(lockedTarget, controller != null ? controller.Target : null, targetInReach))
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

    private bool IsTargetInReach(Transform selectedTarget)
    {
        if (selectedTarget == null)
            return false;

        var targetColliders = selectedTarget.GetComponentsInChildren<Collider2D>();
        if (targetColliders.Length == 0)
        {
            return Vector2.Distance(transform.position, selectedTarget.position) <= attackRange;
        }

        for (int i = 0; i < targetColliders.Length; i++)
        {
            var targetCollider = targetColliders[i];
            if (targetCollider == null || !targetCollider.enabled)
                continue;

            Vector2 closestPoint = targetCollider.ClosestPoint(transform.position);
            if (Vector2.Distance(transform.position, closestPoint) <= attackRange)
                return true;
        }

        return false;
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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

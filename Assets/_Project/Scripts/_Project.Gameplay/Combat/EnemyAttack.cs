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
    [SerializeField] LayerMask targetMask;         // set to Player layer in Inspector
    [SerializeField] Animator animator;            // optional, can be null
    [SerializeField] string playerLayerName = "Player";
    [SerializeField] string attackTriggerName = "Attack"; // matches goblin anim if you add one

    EnemyController2D controller;
    CastleRegionTracker region;

    private CastleRegionTracker Region
    {
        get
        {
            if (region == null)
            {
                region = CastleRegionTracker.Instance;
            }

            return region;
        }
    }
    static bool missingRegionTrackerWarningLogged;
    bool onCooldown;

    static readonly Collider2D[] hits = new Collider2D[4];

    void Awake()
    {
        controller = GetComponent<EnemyController2D>();
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

        // Only attack while we're holding position near the player
        if (!controller.IsInHoldRange()) return;

        // Gate barrier damage by inside/outside state when targeting a barrier.
        if (controller.CurrentTargetType == EnemyTargetType.Barrier)
        {
            GetRegionState(out bool enemyInside, out bool playerInside);
            if (!CanDamageBarrier(enemyInside, playerInside))
                return;
        }

        // Confirm target really is in range (extra safety)
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, hits, targetMask);
        if (count == 0 && controller != null && controller.Target != null && targetMask.value == 0) {
            // Fallback when mask is unset: distance check to the intended target
            float dist = Vector2.Distance(transform.position, controller.Target.position);
            if (dist <= attackRange) count = 1;
        }
        if (count > 0) StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        onCooldown = true;

        if (animator && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        yield return new WaitForSeconds(windupSeconds);

        // Apply damage once per unique IDamageable in range (avoid double hits on multi-collider targets).
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, hits, targetMask);
        var uniqueTargets = new System.Collections.Generic.HashSet<IDamageable>();

        for (int i = 0; i < count; i++)
        {
            var c = hits[i];
            if (!c) continue;

            IDamageable dmg = null;
            if (c.TryGetComponent<IDamageable>(out var selfDmg))
            {
                dmg = selfDmg;
            }
            else
            {
                dmg = c.GetComponentInParent<IDamageable>();
            }

            if (dmg == null)
                continue;

            // Skip barrier damage if gate logic disallows it.
            var barrierHealth = c.GetComponentInParent<BarrierHealth>();
            if (barrierHealth != null && controller.CurrentTargetType == EnemyTargetType.Barrier)
            {
                GetRegionState(out bool enemyInside, out bool playerInside);
                if (!CanDamageBarrier(enemyInside, playerInside))
                    continue;
            }

            if (!uniqueTargets.Add(dmg))
                continue;

            Debug.Log($"[EnemyAttack] Hit IDamageable target: {c.name}, tag: {c.tag}, damage: {Damage}", this);
            DealDamage(dmg);
        }

        yield return new WaitForSeconds(cooldownSeconds);
        onCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void DealDamage(IDamageable target)
    {
        if (target == null || Damage <= 0)
        {
            return;
        }

        target.TakeDamage(Damage);
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
        var reg = Region;
        if (reg == null)
        {
            if (!missingRegionTrackerWarningLogged)
            {
                Debug.LogWarning("[EnemyAttack] CastleRegionTracker.Instance is missing; treating enemy/player as outside for barrier gating.", this);
                missingRegionTrackerWarningLogged = true;
            }
            enemyInside = false;
            playerInside = false;
            return;
        }

        enemyInside = reg.EnemyInside(controller);
        playerInside = reg.PlayerInside;
    }

#if UNITY_EDITOR
    // Test helpers (Editor-only)
    public void Debug_GetRegionState(out bool enemyInside, out bool playerInside) => GetRegionState(out enemyInside, out playerInside);
    public static void Debug_ResetMissingRegionWarning() => missingRegionTrackerWarningLogged = false;
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

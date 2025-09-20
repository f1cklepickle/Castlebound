using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyController2D))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int damage = 1;
    [SerializeField] float attackRange = 1.2f;     // should be >= stopDistance
    [SerializeField] float windupSeconds = 0.15f;  // time before damage applies
    [SerializeField] float cooldownSeconds = 0.8f; // time between attacks
    [SerializeField] LayerMask targetMask;         // set to Player layer in Inspector
    [SerializeField] Animator animator;            // optional, can be null
    [SerializeField] string attackTriggerName = "Attack"; // matches goblin anim if you add one

    EnemyController2D controller;
    bool onCooldown;

    static readonly Collider2D[] hits = new Collider2D[4];

    void Awake()
    {
        controller = GetComponent<EnemyController2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (onCooldown || controller == null) return;

        // Only attack while we're holding position near the player
        if (!controller.IsInHoldRange()) return;

        // Confirm target really is in range (extra safety)
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, hits, targetMask);
        if (count > 0) StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        onCooldown = true;

        if (animator && !string.IsNullOrEmpty(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        yield return new WaitForSeconds(windupSeconds);

        // Apply damage to everything in range that can be damaged (usually just the Player)
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, attackRange, hits, targetMask);
        for (int i = 0; i < count; i++)
        {
            var c = hits[i];
            if (!c) continue;
            // Look for IDamageable on the collider or its parent
            if (c.TryGetComponent<IDamageable>(out var dmg))
                dmg.TakeDamage(damage);
            else if (c.GetComponentInParent<IDamageable>() is IDamageable parentDmg)
                parentDmg.TakeDamage(damage);
        }

        yield return new WaitForSeconds(cooldownSeconds);
        onCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

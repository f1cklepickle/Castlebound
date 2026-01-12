using System.Collections.Generic;
using Castlebound.Gameplay.Combat;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    Collider2D col;
    bool activeWindow;
    readonly HashSet<Collider2D> hitThisSwing = new HashSet<Collider2D>();
    [SerializeField] FeedbackEventChannel playerHitEnemyFeedbackChannel;
    PlayerWeaponController weaponController;
    Vector2 lastHitboxSize = Vector2.zero;

    void Awake()
    {
        EnsureReferences();
        if (col != null)
        {
            col.enabled = false;        // collider stays off until attack frames
        }
        activeWindow = false;
    }

    // Called by Player (via Animation Event at swing start)
    public void Activate()
    {
        hitThisSwing.Clear();
        EnsureReferences();
        UpdateColliderSize();
        if (col != null)
        {
            col.enabled = true;
        }
        activeWindow = true;
    }

    // Called by Player (via Animation Event at swing end)
    public void Deactivate()
    {
        activeWindow = false;
        if (col != null)
        {
            col.enabled = false;
        }
        hitThisSwing.Clear();
    }

    void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
    void OnTriggerStay2D(Collider2D other) { TryHit(other); } // catches �already inside� cases

    void TryHit(Collider2D other)
    {
        if (!activeWindow) return;
        if (!other.CompareTag("Enemy")) return;
        if (hitThisSwing.Contains(other)) return; // prevent multi-hits per swing

        hitThisSwing.Add(other);
        Debug.Log("Hit enemy: " + other.name);

        // Optional: damage interface or Health component
        int damage = 1;
        if (weaponController != null)
        {
            damage = Mathf.Max(1, weaponController.CurrentWeaponStats.Damage);
        }

        var health = other.GetComponent<Health>();
        if (health != null) health.TakeDamage(damage);

        if (playerHitEnemyFeedbackChannel != null)
        {
            int targetId = other.gameObject.GetInstanceID();
            playerHitEnemyFeedbackChannel.Raise(new FeedbackCue(FeedbackCueType.PlayerHitEnemy, other.transform.position, targetId));
        }
    }

    void UpdateColliderSize()
    {
        if (weaponController == null || col == null)
        {
            return;
        }

        var size = weaponController.CurrentWeaponStats.HitboxSize;
        if (size == Vector2.zero || size == lastHitboxSize)
        {
            return;
        }

        lastHitboxSize = size;
        if (col is BoxCollider2D box)
        {
            box.size = size;
            box.offset = weaponController.CurrentWeaponStats.HitboxOffset;
        }
        else if (col is CircleCollider2D circle)
        {
            circle.radius = Mathf.Max(size.x, size.y) * 0.5f;
        }
    }

    void EnsureReferences()
    {
        if (col == null)
        {
            col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        if (weaponController == null)
        {
            weaponController = GetComponentInParent<PlayerWeaponController>();
        }
    }
}

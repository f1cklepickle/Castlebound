using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    Collider2D col;
    bool activeWindow;
    readonly HashSet<Collider2D> hitThisSwing = new HashSet<Collider2D>();
    [SerializeField] FeedbackEventChannel playerHitEnemyFeedbackChannel;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;        // collider stays off until attack frames
        activeWindow = false;
    }

    // Called by Player (via Animation Event at swing start)
    public void Activate()
    {
        hitThisSwing.Clear();
        col.enabled = true;
        activeWindow = true;
    }

    // Called by Player (via Animation Event at swing end)
    public void Deactivate()
    {
        activeWindow = false;
        col.enabled = false;
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
        var health = other.GetComponent<Health>();
        if (health != null) health.TakeDamage(1);

        if (playerHitEnemyFeedbackChannel != null)
        {
            int targetId = other.gameObject.GetInstanceID();
            playerHitEnemyFeedbackChannel.Raise(new FeedbackCue(FeedbackCueType.PlayerHitEnemy, other.transform.position, targetId));
        }
    }
}


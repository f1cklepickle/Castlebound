using System;
using System.Collections.Generic;
using UnityEngine;

public class BarrierHealth : MonoBehaviour, IDamageable
{
    private static readonly List<BarrierHealth> _all = new List<BarrierHealth>();
    public static IReadOnlyList<BarrierHealth> All => _all;
    private static readonly Collider2D[] _overlapBuffer = new Collider2D[24];

    public event Action OnBroken;

    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth = 10;
    [SerializeField] private float enemyPushInDistance = 0.5f;
    private Collider2D barrierCollider;
    private SpriteRenderer barrierSprite;

    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = Mathf.Max(0, value);
    }

    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, MaxHealth);
    }

    public bool IsBroken { get; private set; }
    public float EnemyPushInDistance => enemyPushInDistance;

    private void OnEnable()
    {
        barrierCollider = GetComponent<Collider2D>();
        barrierSprite = GetComponent<SpriteRenderer>();
        UpdateBrokenState();
        ResolveActiveOverlaps();

        if (!_all.Contains(this))
        {
            _all.Add(this);
        }
    }

    private void OnDisable()
    {
        _all.Remove(this);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsBroken)
        {
            return;
        }

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            IsBroken = true;
        }

        UpdateBrokenState();

        if (IsBroken)
        {
            OnBroken?.Invoke();
        }
    }

    public void Repair()
    {
        // If max health is zero, there's nothing to repair.
        if (MaxHealth <= 0)
        {
            return;
        }

        // Restore health and clear broken flag.
        CurrentHealth = MaxHealth;
        IsBroken = false;

        // Ensure collider + sprite are re-enabled.
        UpdateBrokenState();
        ResolveActiveOverlaps();
    }

    public void ReviveIfNeeded()
    {
        if (!IsBroken || CurrentHealth <= 0)
        {
            return;
        }

        IsBroken = false;
        UpdateBrokenState();
        ResolveActiveOverlaps();
    }

    private void UpdateBrokenState()
    {
        if (barrierCollider == null)
        {
            barrierCollider = GetComponent<Collider2D>();
        }

        if (barrierSprite == null)
        {
            barrierSprite = GetComponent<SpriteRenderer>();
        }

        bool broken = IsBroken;

        if (barrierCollider != null)
        {
            barrierCollider.enabled = !broken;
        }

        if (barrierSprite != null)
        {
            barrierSprite.enabled = !broken;
        }
    }

    private void ResolveActiveOverlaps()
    {
        if (barrierCollider == null || !barrierCollider.enabled)
        {
            return;
        }

        Physics2D.SyncTransforms();

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                ColliderDistance2D dist = Physics2D.Distance(barrierCollider, playerCollider);
                if (dist.isOverlapped)
                {
                    BarrierOverlapResolver.ResolveOverlap(barrierCollider, playerCollider, true);
                }
            }
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = false;

        int count = barrierCollider.OverlapCollider(filter, _overlapBuffer);
        for (int i = 0; i < count; i++)
        {
            var other = _overlapBuffer[i];
            if (other == null)
            {
                continue;
            }

            if (other.GetComponentInParent<EnemyController2D>() != null)
            {
                BarrierOverlapResolver.ResolveOverlap(barrierCollider, other, false);
            }
        }
    }

    public static Transform[] GetActiveGateTransforms()
    {
        if (_all.Count == 0)
            return System.Array.Empty<Transform>();

        var active = new List<Transform>(_all.Count);
        for (int i = 0; i < _all.Count; i++)
        {
            var barrier = _all[i];
            if (barrier == null || barrier.IsBroken)
                continue;

            active.Add(barrier.transform);
        }

        return active.ToArray();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Castlebound.Gameplay.Stats;
using Castlebound.Gameplay.AI;

public class BarrierHealth : MonoBehaviour, IDamageable
{
    private static readonly List<BarrierHealth> _all = new List<BarrierHealth>();
    public static IReadOnlyList<BarrierHealth> All => _all;
    private static readonly Collider2D[] _overlapBuffer = new Collider2D[24];

    public event Action OnBroken;
    public event Action OnRepaired;

    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth = 10;
    [SerializeField] private float enemyPushInDistance = 0.5f;
    [SerializeField] private SpriteRenderer barrierGateRenderer;
    private Collider2D barrierCollider;
    private SpriteRenderer barrierSprite;
    private Rect navigationBounds;
    private bool navigationSolid;

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
    public bool IsDamaged => MaxHealth > 0 && CurrentHealth < MaxHealth;
    public bool CanRepair => IsDamaged;
    public float EnemyPushInDistance => enemyPushInDistance;

    private void OnEnable()
    {
        barrierCollider = GetComponent<Collider2D>();
        CacheRenderers();
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
        if (navigationSolid) StaticNavigationRuntime.Instance?.Invalidate(navigationBounds, navigationBounds);
        navigationSolid = false;
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

    public bool Repair()
    {
        if (!CanRepair)
        {
            return false;
        }

        CurrentHealth = MaxHealth;
        IsBroken = false;

        UpdateBrokenState();
        ResolveActiveOverlaps();
        OnRepaired?.Invoke();
        RunStatsEvents.RaiseRepairPerformed();
        return true;
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
        OnRepaired?.Invoke();
    }

    private void UpdateBrokenState()
    {
        if (barrierCollider == null)
        {
            barrierCollider = GetComponent<Collider2D>();
        }

        CacheRenderers();

        bool broken = IsBroken;

        if (barrierCollider != null)
        {
            barrierCollider.enabled = !broken;
        }

        if (barrierSprite != null)
        {
            barrierSprite.enabled = !broken;
        }

        if (barrierGateRenderer != null)
        {
            barrierGateRenderer.enabled = !broken;
        }
        RefreshNavigationBounds();
    }

    private void LateUpdate() => RefreshNavigationBounds();

    private void RefreshNavigationBounds()
    {
        bool solid = barrierCollider != null && barrierCollider.enabled && barrierCollider.gameObject.activeInHierarchy;
        Rect current = navigationBounds;
        if (solid)
        {
            Bounds bounds = barrierCollider.bounds;
            current = Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
        }
        if (solid != navigationSolid || solid && current != navigationBounds)
            StaticNavigationRuntime.Instance?.Invalidate(navigationSolid ? navigationBounds : current, current);
        navigationBounds = current; navigationSolid = solid;
    }

    private void CacheRenderers()
    {
        if (barrierSprite == null)
        {
            barrierSprite = GetComponent<SpriteRenderer>();
        }

        if (barrierGateRenderer == null)
        {
            var binder = GetComponent<Castlebound.Gameplay.Castle.BarrierVisualBinder>();
            if (binder != null)
            {
                barrierGateRenderer = binder.GateRenderer;
            }
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
                var mover = player.GetComponent<PlayerCollisionMove2D>();
                mover?.ReconcileExternalPosition(barrierCollider);

                Physics2D.SyncTransforms();
                ColliderDistance2D dist = Physics2D.Distance(barrierCollider, playerCollider);
                if (dist.isOverlapped)
                {
                    BarrierOverlapResolver.ResolveOverlap(barrierCollider, playerCollider, true);
                    mover?.ReconcileExternalPosition();
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
            if (other == null || other.GetComponent<EnemySeparationCollider>() != null)
            {
                continue;
            }

            var enemy = other.GetComponentInParent<EnemyController2D>();
            if (enemy != null)
            {
                bool pushedOutside = BarrierOverlapResolver.ResolveOverlap(barrierCollider, other, false);
                if (pushedOutside)
                {
                    CastleRegionTracker.Instance?.ReconcileEnemyOutsideAfterBarrierRepair(enemy);
                }
            }
        }
    }

}

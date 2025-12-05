using UnityEngine;
using System.Collections.Generic;

public class BarrierHealth : MonoBehaviour, IDamageable
{
    private static readonly List<BarrierHealth> _all = new List<BarrierHealth>();
    public static IReadOnlyList<BarrierHealth> All => _all;

    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth = 10;
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

    private void OnEnable()
    {
        barrierCollider = GetComponent<Collider2D>();
        barrierSprite = GetComponent<SpriteRenderer>();
        UpdateBrokenState();

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

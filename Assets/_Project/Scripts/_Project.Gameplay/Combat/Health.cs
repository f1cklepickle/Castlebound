// Assets/Scripts/Combat/Health.cs
using System;
using UnityEngine;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Stats;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] FeedbackEventChannel playerHitFeedbackChannel;
    [SerializeField] MonoBehaviour damageImmunitySource;
    int current;
    IDamageImmunitySource damageImmunity;
    bool damageImmunityResolved;

    public int Current => current;
    public int Max => maxHealth;
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDied;

    public void ConfigureMaxHealth(int value, bool refill)
    {
        maxHealth = Mathf.Max(0, value);
        current = refill ? maxHealth : Mathf.Clamp(current, 0, maxHealth);
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    void Awake()
    {
        ResolveDamageImmunity();
        current = maxHealth;
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        ApplyDamage(amount);
    }

    public int ApplyDamage(int amount)
    {
        ResolveDamageImmunity();
        if (current <= 0 || amount <= 0 || (damageImmunity != null && damageImmunity.IsDamageImmune))
            return 0;
        int previous = current;
        current = Mathf.Max(0, current - amount);
        int appliedDamage = previous - current;
        OnHealthChanged?.Invoke(current, maxHealth);

        if (CompareTag("Enemy"))
            RunStatsEvents.RaiseDamageDealt(appliedDamage);
        else if (CompareTag("Player"))
            RunStatsEvents.RaiseDamageTaken(appliedDamage);

        if (CompareTag("Player") && playerHitFeedbackChannel != null)
        {
            playerHitFeedbackChannel.Raise(new FeedbackCue(FeedbackCueType.PlayerHit, transform.position));
        }

        if (current <= 0) Die();
        return appliedDamage;
    }

    public void Heal(int amount)
    {
        if (current <= 0 || amount <= 0) return;
        int previous = current;
        current = Mathf.Min(maxHealth, current + amount);
        OnHealthChanged?.Invoke(current, maxHealth);

        if (CompareTag("Player"))
            RunStatsEvents.RaiseHealthRestored(current - previous);
    }

    void Die()
    {
        OnDied?.Invoke();
        if (CompareTag("Enemy"))
            RunStatsEvents.RaiseEnemyKilled();
        if (CompareTag("Player"))
            GameManager.I?.OnPlayerDied();

        if (Application.isPlaying)
            Destroy(gameObject);
        else
            DestroyImmediate(gameObject);
    }

    private void ResolveDamageImmunity()
    {
        if (damageImmunityResolved)
            return;

        damageImmunityResolved = true;
        damageImmunity = damageImmunitySource as IDamageImmunitySource;
        if (damageImmunity != null)
            return;

        var sources = GetComponents<MonoBehaviour>();
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] is IDamageImmunitySource immunitySource)
            {
                damageImmunitySource = sources[i];
                damageImmunity = immunitySource;
                return;
            }
        }
    }
}

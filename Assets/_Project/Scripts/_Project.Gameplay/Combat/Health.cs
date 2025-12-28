// Assets/Scripts/Combat/Health.cs
using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] FeedbackEventChannel playerHitFeedbackChannel;
    int current;

    public int Current => current;
    public int Max => maxHealth;
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDied;

    void Awake()
    {
        current = maxHealth;
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (current <= 0) return;
        current = Mathf.Max(0, current - amount);
        OnHealthChanged?.Invoke(current, maxHealth);

        if (CompareTag("Player") && playerHitFeedbackChannel != null)
        {
            playerHitFeedbackChannel.Raise(new FeedbackCue(FeedbackCueType.PlayerHit, transform.position));
        }

        if (current <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (current <= 0) return;
        current = Mathf.Min(maxHealth, current + amount);
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    void Die()
    {
        OnDied?.Invoke();
        if (CompareTag("Player"))
            GameManager.I?.OnPlayerDied();

        Destroy(gameObject);
    }
}

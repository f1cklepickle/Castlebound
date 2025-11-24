using UnityEngine;

public class BarrierHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currentHealth = 10;

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
    }
}

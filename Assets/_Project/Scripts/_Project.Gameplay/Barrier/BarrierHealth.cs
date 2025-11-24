using UnityEngine;

public class BarrierHealth : MonoBehaviour, IDamageable
{
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
}

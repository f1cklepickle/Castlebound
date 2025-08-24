using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHealth = 10;
    int current;

    void Awake() => current = maxHealth;

    public void TakeDamage(int amount)
    {
        current -= amount;
        if (current <= 0)
        {
            // TODO: death anim/effects
            Destroy(gameObject);
        }
    }
}
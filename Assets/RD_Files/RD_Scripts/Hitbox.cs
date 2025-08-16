using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // For now, we'll just log a message to the console.
        // In the future, you would replace this with code
        // that damages the enemy.
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit an enemy!");
            // other.GetComponent<EnemyHealth>().TakeDamage(damage);
        }
    }
}
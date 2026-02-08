using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyKnockbackReceiver : MonoBehaviour
    {
        Vector2 knockbackVelocity;
        float decayPerSecond;

        public void AddKnockback(Vector2 velocity, float decay)
        {
            knockbackVelocity += velocity;
            decayPerSecond = Mathf.Max(decayPerSecond, decay);
        }

        public Vector2 ConsumeDisplacement(float dt)
        {
            if (knockbackVelocity.sqrMagnitude <= 0f || dt <= 0f)
            {
                return Vector2.zero;
            }

            Vector2 displacement = knockbackVelocity * dt;
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, decayPerSecond * dt);
            return displacement;
        }
    }
}

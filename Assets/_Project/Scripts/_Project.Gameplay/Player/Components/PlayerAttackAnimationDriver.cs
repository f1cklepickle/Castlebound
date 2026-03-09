using UnityEngine;

public class PlayerAttackAnimationDriver : MonoBehaviour
{
    [SerializeField] private string attackSpeedParameter = "AttackSpeedMultiplier";
    [SerializeField] private float minAttackSpeedMultiplier = 0.25f;
    [SerializeField] private float maxAttackSpeedMultiplier = 3f;

    public void ApplyAttackSpeed(Animator animator, float effectiveAttackRate, float baseAttackRate)
    {
        if (animator == null || string.IsNullOrWhiteSpace(attackSpeedParameter))
            return;

        float safeBaseRate = Mathf.Max(baseAttackRate, 0.0001f);
        float rawMultiplier = effectiveAttackRate / safeBaseRate;
        float clampedMultiplier = Mathf.Clamp(rawMultiplier, minAttackSpeedMultiplier, maxAttackSpeedMultiplier);
        animator.SetFloat(attackSpeedParameter, clampedMultiplier);
    }
}

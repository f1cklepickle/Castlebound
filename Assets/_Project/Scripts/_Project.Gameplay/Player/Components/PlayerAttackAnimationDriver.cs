using UnityEngine;

public class PlayerAttackAnimationDriver : MonoBehaviour
{
    [SerializeField] private string attackSpeedParameter = "AttackSpeedMultiplier";
    [SerializeField] private string normalizedProgressParameter = "AttackProgress";
    [SerializeField] private string loopActiveParameter = "AttackLoopActive";
    [SerializeField] private float minAttackSpeedMultiplier = 0.25f;
    [SerializeField] private float maxAttackSpeedMultiplier = 12f;

    public void ApplyAttackSpeed(Animator animator, float effectiveAttackRate, float baseAttackRate)
    {
        if (animator == null || string.IsNullOrWhiteSpace(attackSpeedParameter))
            return;
        if (!HasParameter(animator, attackSpeedParameter, AnimatorControllerParameterType.Float))
            return;

        float safeBaseRate = Mathf.Max(baseAttackRate, 0.0001f);
        float rawMultiplier = effectiveAttackRate / safeBaseRate;
        float clampedMultiplier = Mathf.Clamp(rawMultiplier, minAttackSpeedMultiplier, maxAttackSpeedMultiplier);
        animator.SetFloat(attackSpeedParameter, clampedMultiplier);
    }

    public void ApplyLoopPresentation(
        Animator animator,
        bool isSwingActive,
        float normalizedSwingProgress,
        float effectiveAttackRate,
        float baseAttackRate)
    {
        if (animator == null)
            return;

        ApplyAttackSpeed(animator, effectiveAttackRate, baseAttackRate);

        if (!string.IsNullOrWhiteSpace(loopActiveParameter) &&
            HasParameter(animator, loopActiveParameter, AnimatorControllerParameterType.Bool))
            animator.SetBool(loopActiveParameter, isSwingActive);

        if (!string.IsNullOrWhiteSpace(normalizedProgressParameter) &&
            HasParameter(animator, normalizedProgressParameter, AnimatorControllerParameterType.Float))
            animator.SetFloat(normalizedProgressParameter, Mathf.Clamp01(normalizedSwingProgress));
    }

    private static bool HasParameter(
        Animator animator,
        string parameterName,
        AnimatorControllerParameterType expectedType)
    {
        var parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (p.type == expectedType && p.name == parameterName)
                return true;
        }

        return false;
    }
}

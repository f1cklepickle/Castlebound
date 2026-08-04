using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public static class AttackRatePolicy
    {
        public const float MinimumAttackRate = 0.1f;
        public const float MaximumAttackRate = 33.333333f;

        public static float Normalize(float attacksPerSecond)
        {
            if (float.IsNaN(attacksPerSecond) || float.IsNegativeInfinity(attacksPerSecond))
                return MinimumAttackRate;

            if (float.IsPositiveInfinity(attacksPerSecond))
                return MaximumAttackRate;

            return Mathf.Clamp(attacksPerSecond, MinimumAttackRate, MaximumAttackRate);
        }
    }
}

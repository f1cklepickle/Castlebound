using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public static class PlayerDefenseHitResolver
    {
        private const float DirectionEpsilon = 0.000001f;
        private const float AngleTolerance = 0.0001f;

        public static PlayerHitResult Resolve(
            PlayerHitRequest request,
            PlayerDefenseState defenseState,
            Vector2 playerPosition,
            Vector2 facingDirection,
            float blockArcDegrees)
        {
            if (request.DamageType != CombatDamageType.Melee
                || !IsGuarding(defenseState)
                || !IsWithinBlockArc(
                    playerPosition,
                    facingDirection,
                    request.AttackOrigin,
                    blockArcDegrees))
            {
                return new PlayerHitResult(request, PlayerHitOutcome.Damaged, request.Damage);
            }

            PlayerHitOutcome outcome = defenseState == PlayerDefenseState.ParryWindow
                ? PlayerHitOutcome.Parried
                : PlayerHitOutcome.Blocked;
            return new PlayerHitResult(request, outcome, 0);
        }

        public static bool IsWithinBlockArc(
            Vector2 playerPosition,
            Vector2 facingDirection,
            Vector2 attackOrigin,
            float blockArcDegrees)
        {
            Vector2 directionToAttacker = attackOrigin - playerPosition;
            if (facingDirection.sqrMagnitude <= DirectionEpsilon
                || directionToAttacker.sqrMagnitude <= DirectionEpsilon)
            {
                return false;
            }

            float safeArc = NormalizeArc(blockArcDegrees);
            float angle = Vector2.Angle(facingDirection, directionToAttacker);
            return angle <= safeArc * 0.5f + AngleTolerance;
        }

        private static bool IsGuarding(PlayerDefenseState defenseState)
        {
            return defenseState == PlayerDefenseState.ParryWindow
                || defenseState == PlayerDefenseState.Blocking;
        }

        private static float NormalizeArc(float blockArcDegrees)
        {
            if (float.IsNaN(blockArcDegrees) || blockArcDegrees <= 0f)
                return 0f;
            if (float.IsPositiveInfinity(blockArcDegrees))
                return 360f;
            return Mathf.Clamp(blockArcDegrees, 0f, 360f);
        }
    }
}

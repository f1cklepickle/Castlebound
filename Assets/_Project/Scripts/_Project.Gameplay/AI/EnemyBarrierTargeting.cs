using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    /// <summary>
    /// Helper for checking if a barrier blocks the direct path
    /// between an enemy and the player.
    /// </summary>
    public static class EnemyBarrierTargeting
    {
        private const float Tolerance = 0.1f;

        public static bool ShouldTargetBarrier(Vector2 enemyPos, Vector2 playerPos, Vector2 barrierPos)
        {
            Vector2 toPlayer = playerPos - enemyPos;
            float sqrLen = toPlayer.sqrMagnitude;
            if (sqrLen < Mathf.Epsilon)
                return false;

            Vector2 toBarrier = barrierPos - enemyPos;

            float t = Vector2.Dot(toBarrier, toPlayer) / sqrLen;
            if (t <= 0f || t >= 1f)
                return false;

            Vector2 closest = enemyPos + t * toPlayer;
            return Vector2.Distance(barrierPos, closest) <= Tolerance;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    /// <summary>
    /// Simple stub for castle targeting logic.
    /// Only returns player for now so the first test can go green.
    /// Will be expanded as more tests are implemented.
    /// </summary>
    public static class CastleTargetSelector
    {
        public static Transform ChooseTarget(
            Vector2 enemyPosition,
            bool enemyInside,
            bool playerInside,
            Transform player,
            IReadOnlyList<Transform> gates)
        {
            // If we have no player reference, nothing to do.
            if (player == null)
                return null;

            // If player is outside the castle, ALWAYS chase the player,
            // regardless of how many gates or barriers exist.
            if (!playerInside)
                return player;

            // At this point, playerInside == true.

            // If the enemy is outside and there are gates, pick the nearest INTACT gate.
            if (!enemyInside && gates != null && gates.Count > 0)
            {
                Transform nearestGate = null;
                float bestSqrDist = float.MaxValue;

                for (int i = 0; i < gates.Count; i++)
                {
                    var gate = gates[i];
                    if (gate == null)
                        continue;

                    // Skip broken barriers.
                    var barrierHealth = gate.GetComponent<BarrierHealth>();
                    bool barrierBroken = barrierHealth != null && barrierHealth.IsBroken;
                    if (barrierBroken)
                        continue;

                    float sqrDist = ((Vector2)gate.position - enemyPosition).sqrMagnitude;
                    if (sqrDist < bestSqrDist)
                    {
                        bestSqrDist = sqrDist;
                        nearestGate = gate;
                    }
                }

                if (nearestGate != null)
                    return nearestGate;
            }

            // Default: target player.
            // - Enemy outside but no intact gates / gates list empty.
            // - Enemy inside with player inside.
            // - Any other fallback condition.
            return player;
        }
    }
}

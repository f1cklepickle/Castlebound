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
            // If player is outside, always chase player.
            if (!playerInside)
            {
                return player;
            }

            // Player inside, enemy outside → prefer nearest gate if any.
            if (playerInside && !enemyInside && gates != null && gates.Count > 0)
            {
                Transform nearestGate = gates[0];
                float bestSqrDist = ((Vector2)gates[0].position - enemyPosition).sqrMagnitude;

                for (int i = 1; i < gates.Count; i++)
                {
                    var gate = gates[i];
                    float sqrDist = ((Vector2)gate.position - enemyPosition).sqrMagnitude;
                    if (sqrDist < bestSqrDist)
                    {
                        bestSqrDist = sqrDist;
                        nearestGate = gate;
                    }
                }

                return nearestGate;
            }

            // Default: target player (both inside, enemy inside/player outside, or no gates).
            return player;
        }
    }
}

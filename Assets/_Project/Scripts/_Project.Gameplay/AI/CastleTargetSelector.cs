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
            // If player is outside, always chase player (ignore gates).
            if (!playerInside)
            {
                return player;
            }

            // Enemy outside → prefer nearest gate if any.
            if (!enemyInside && gates != null && gates.Count > 0)
            {
                Transform nearestGate = null;
                float bestSqrDist = float.MaxValue;

                for (int i = 0; i < gates.Count; i++)
                {
                    var gate = gates[i];
                    var barrierHealth = gate != null ? gate.GetComponent<BarrierHealth>() : null;
                    bool barrierBroken = barrierHealth != null && barrierHealth.IsBroken;

                    if (barrierBroken || gate == null)
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

            // Default: target player (both inside or no gates available).
            return player;
        }
    }
}

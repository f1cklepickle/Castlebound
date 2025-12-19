using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public enum EnemyTargetType
    {
        None,
        Player,
        Barrier
    }

    public static class EnemyTargetSelector
    {
        public struct Input
        {
            public Vector2 EnemyPosition;
            public bool EnemyInside;
            public bool PlayerInside;
            public Transform Player;
            public Transform HomeBarrier;
            public float PassThroughRadius;
        }

        public struct Decision
        {
            public Transform SteerTarget;
            public Transform AttackTarget;
            public EnemyTargetType TargetType;
        }

        public static Decision Select(Input input)
        {
            var decision = new Decision
            {
                SteerTarget = null,
                AttackTarget = null,
                TargetType = EnemyTargetType.None
            };

            if (input.Player == null)
            {
                return decision;
            }

            // Player outside: chase player.
            if (!input.PlayerInside)
            {
                decision.SteerTarget = input.Player;
                decision.AttackTarget = input.Player;
                decision.TargetType = EnemyTargetType.Player;
                return decision;
            }

            // Player inside.
            if (input.EnemyInside)
            {
                decision.SteerTarget = input.Player;
                decision.AttackTarget = input.Player;
                decision.TargetType = EnemyTargetType.Player;
                return decision;
            }

            // Enemy outside, player inside: go for home barrier if known.
            if (input.HomeBarrier != null)
            {
                var health = input.HomeBarrier.GetComponent<BarrierHealth>();
                bool broken = health != null && health.IsBroken;

                // If broken and we're effectively at the barrier opening, switch to player.
                if (broken && input.PassThroughRadius > 0f)
                {
                    float distToHome = Vector2.Distance(input.EnemyPosition, input.HomeBarrier.position);
                    if (distToHome <= input.PassThroughRadius)
                    {
                        decision.SteerTarget = input.Player;
                        decision.AttackTarget = input.Player;
                        decision.TargetType = EnemyTargetType.Player;
                        return decision;
                    }
                }

                decision.SteerTarget = input.HomeBarrier;
                decision.AttackTarget = input.HomeBarrier;
                decision.TargetType = EnemyTargetType.Barrier;
                return decision;
            }

            // Fallback: chase player.
            decision.SteerTarget = input.Player;
            decision.AttackTarget = input.Player;
            decision.TargetType = EnemyTargetType.Player;
            return decision;
        }
    }
}

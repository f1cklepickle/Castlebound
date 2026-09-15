using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Supplies local observations to the sampler; targeting and locomotion remain authoritative.
    public sealed class EnemyPredictiveChase
    {
        public const float NeighborRange = 4.5f;
        private readonly List<EnemyAvoidanceNeighbor> neighbors = new List<EnemyAvoidanceNeighbor>(16);
        private readonly EnemyPredictiveAvoidance solver = new EnemyPredictiveAvoidance();
        private Transform previousPlayer;

        public void Reset()
        {
            previousPlayer = null;
            neighbors.Clear();
            solver.Reset();
        }

        public static bool IsEligible(EnemyController2D owner, Transform player, EnemyLocomotion movement)
        {
            if (!IsRelevant(owner, player) || movement == null || !movement.isActiveAndEnabled ||
                movement.CurrentState != EnemyController2D.State.CHASE ||
                owner.GetComponent<EnemyTargeting>().SteerTarget != player)
                return false;
            var root = owner.GetComponent<EnemyRootReceiver>();
            var stagger = owner.GetComponent<EnemyStaggerReceiver>();
            return (root == null || !root.IsRooted) && (stagger == null || !stagger.IsActionLocked);
        }

        public Vector2 Compute(EnemyController2D owner, Transform player, float speed)
        {
            if (previousPlayer != player)
            {
                solver.Reset();
                previousPlayer = player;
            }
            var body = owner.GetComponent<Rigidbody2D>();
            var ownSensor = owner.GetComponentInChildren<EnemySeparationCollider>();
            Vector2 desired = ((Vector2)player.position - body.position).normalized * Mathf.Max(0f, speed);
            if (ownSensor == null || !ownSensor.isActiveAndEnabled || !ownSensor.Collider.enabled)
                return desired;

            Vector2 position = ownSensor.Collider.bounds.center;
            neighbors.Clear();
            foreach (var other in EnemyController2D.All)
            {
                if (other == owner || !IsRelevant(other, player)) continue;
                var sensor = other.GetComponentInChildren<EnemySeparationCollider>();
                if (sensor == null || !sensor.isActiveAndEnabled || !sensor.Collider.enabled) continue;
                var otherBody = sensor.Collider.attachedRigidbody;
                if (otherBody == null || !otherBody.simulated) continue;
                Vector2 otherPosition = sensor.Collider.bounds.center;
                if ((otherPosition - position).sqrMagnitude > NeighborRange * NeighborRange) continue;
                var movement = other.GetComponent<EnemyLocomotion>();
                var root = other.GetComponent<EnemyRootReceiver>();
                var stagger = other.GetComponent<EnemyStaggerReceiver>();
                bool stationary = movement.IsInHoldRange || (root != null && root.IsRooted) ||
                    (stagger != null && stagger.IsActionLocked);
                neighbors.Add(new EnemyAvoidanceNeighbor(otherPosition,
                    stationary ? Vector2.zero : otherBody.velocity, sensor.Collider.bounds.extents.x));
            }
            return solver.SelectVelocity(position, desired, ownSensor.Collider.bounds.extents.x, neighbors);
        }

        private static bool IsRelevant(EnemyController2D candidate, Transform player)
        {
            if (candidate == null || !candidate.isActiveAndEnabled || player == null ||
                !player.CompareTag("Player") || candidate.CurrentTargetType != EnemyTargetType.Player ||
                candidate.Target != player)
                return false;
            var health = candidate.GetComponent<Health>();
            var movement = candidate.GetComponent<EnemyLocomotion>();
            return health != null && health.Current > 0 && movement != null &&
                movement.HoldMovementPolicySource == null;
        }
    }
}

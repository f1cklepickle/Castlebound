using UnityEngine;

namespace Castlebound.Gameplay.Projectile
{
    public readonly struct ProjectileLaunchRequest
    {
        public ProjectileLaunchRequest(
            ProjectileRuntime projectilePrefab,
            Vector2 origin,
            Vector2 targetPoint,
            Transform owner,
            float speed,
            int damage,
            float lifetime,
            LayerMask targetLayerMask,
            float visualAngleOffsetDegrees)
        {
            ProjectilePrefab = projectilePrefab;
            Origin = origin;
            TargetPoint = targetPoint;
            Owner = owner;
            Speed = speed;
            Damage = damage;
            Lifetime = lifetime;
            TargetLayerMask = targetLayerMask;
            VisualAngleOffsetDegrees = visualAngleOffsetDegrees;
        }

        public ProjectileRuntime ProjectilePrefab { get; }
        public Vector2 Origin { get; }
        public Vector2 TargetPoint { get; }
        public Transform Owner { get; }
        public float Speed { get; }
        public int Damage { get; }
        public float Lifetime { get; }
        public LayerMask TargetLayerMask { get; }
        public float VisualAngleOffsetDegrees { get; }
    }
}

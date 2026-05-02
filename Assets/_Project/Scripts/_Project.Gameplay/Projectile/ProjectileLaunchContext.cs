using UnityEngine;

namespace Castlebound.Gameplay.Projectile
{
    public readonly struct ProjectileLaunchContext
    {
        public ProjectileLaunchContext(
            Vector2 origin,
            Vector2 direction,
            Transform owner,
            float speed,
            int damage,
            float lifetime,
            LayerMask targetLayerMask)
        {
            Origin = origin;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
            Owner = owner;
            Speed = speed;
            Damage = damage;
            Lifetime = lifetime;
            TargetLayerMask = targetLayerMask;
        }

        public Vector2 Origin { get; }
        public Vector2 Direction { get; }
        public Transform Owner { get; }
        public float Speed { get; }
        public int Damage { get; }
        public float Lifetime { get; }
        public LayerMask TargetLayerMask { get; }

        public static ProjectileLaunchContext Directional(
            Vector2 origin,
            Vector2 direction,
            Transform owner,
            float speed,
            int damage,
            float lifetime,
            LayerMask targetLayerMask)
        {
            return new ProjectileLaunchContext(origin, direction, owner, speed, damage, lifetime, targetLayerMask);
        }

        public static ProjectileLaunchContext TowardPoint(
            Vector2 origin,
            Vector2 targetPoint,
            Transform owner,
            float speed,
            int damage,
            float lifetime,
            LayerMask targetLayerMask)
        {
            return new ProjectileLaunchContext(origin, targetPoint - origin, owner, speed, damage, lifetime, targetLayerMask);
        }
    }
}

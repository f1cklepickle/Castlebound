using UnityEngine;

namespace Castlebound.Gameplay.Projectile
{
    public static class ProjectileLauncher
    {
        public static ProjectileRuntime Launch(ProjectileLaunchRequest request)
        {
            if (request.ProjectilePrefab == null)
            {
                return null;
            }

            var direction = ResolveDirection(request.Origin, request.TargetPoint, request.Owner);
            var rotation = CreateRotation(direction, request.VisualAngleOffsetDegrees);
            var projectile = Object.Instantiate(request.ProjectilePrefab, request.Origin, rotation);
            var context = ProjectileLaunchContext.Directional(
                request.Origin,
                direction,
                request.Owner,
                request.Speed,
                request.Damage,
                request.Lifetime,
                request.TargetLayerMask);

            projectile.Launch(context);
            return projectile;
        }

        private static Vector2 ResolveDirection(Vector2 origin, Vector2 targetPoint, Transform owner)
        {
            var direction = targetPoint - origin;
            if (direction.sqrMagnitude > 0f)
            {
                return direction.normalized;
            }

            return owner != null ? (Vector2)owner.up : Vector2.up;
        }

        private static Quaternion CreateRotation(Vector2 direction, float visualAngleOffsetDegrees)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle + visualAngleOffsetDegrees);
        }
    }
}

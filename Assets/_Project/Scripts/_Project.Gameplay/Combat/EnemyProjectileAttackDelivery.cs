using System;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Projectile;
using UnityEngine;

public class EnemyProjectileAttackDelivery : MonoBehaviour, IEnemyAttackDelivery
{
    [SerializeField] private Transform launchPoint;

    public event Action<ProjectileRuntime> Fired;

    public EnemyAttackRole AttackRole => EnemyAttackRole.Ranged;
    public Transform LaunchPoint { get => launchPoint; set => launchPoint = value; }
    public ProjectileRuntime LastLaunchedProjectile { get; private set; }

    public bool CanDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        return lockedTarget != null &&
               equipmentSnapshot != null &&
               equipmentSnapshot.IsCompatibleWith(AttackRole) &&
               equipmentSnapshot.ProjectilePrefab != null;
    }

    public bool TryDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        if (!CanDeliver(lockedTarget, equipmentSnapshot))
        {
            return false;
        }

        Vector2 origin = launchPoint != null ? launchPoint.position : transform.position;
        var request = new ProjectileLaunchRequest(
            equipmentSnapshot.ProjectilePrefab,
            origin,
            lockedTarget.position,
            transform,
            equipmentSnapshot.ProjectileSpeed,
            equipmentSnapshot.ProjectileDamage,
            equipmentSnapshot.ProjectileLifetime,
            equipmentSnapshot.ProjectileTargetLayerMask,
            equipmentSnapshot.ProjectileVisualAngleOffsetDegrees);

        LastLaunchedProjectile = ProjectileLauncher.Launch(request);
        if (LastLaunchedProjectile == null)
        {
            return false;
        }

        Fired?.Invoke(LastLaunchedProjectile);
        return true;
    }
}

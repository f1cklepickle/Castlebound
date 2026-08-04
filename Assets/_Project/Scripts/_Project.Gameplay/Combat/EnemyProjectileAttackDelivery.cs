using System;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Projectile;
using UnityEngine;

public class EnemyProjectileAttackDelivery : MonoBehaviour, IEnemyAttackDelivery
{
    [SerializeField] private Transform launchPoint;

    public event Action<ProjectileRuntime> Fired;

    public EnemyAttackRole AttackRole => EnemyAttackRole.Ranged;
    public Transform LaunchPoint { get => launchPoint; set => launchPoint = value; }
    public ProjectileRuntime LastLaunchedProjectile { get; private set; }

    public bool CanDeliver(
        Transform lockedTarget,
        EnemyEquipmentDefinition equipmentDefinitionSnapshot,
        CombatEquipmentSnapshot combatEquipmentSnapshot)
    {
        return lockedTarget != null &&
               equipmentDefinitionSnapshot != null &&
               equipmentDefinitionSnapshot.IsCompatibleWith(AttackRole) &&
               combatEquipmentSnapshot.ProjectilePrefab != null;
    }

    public bool TryDeliver(
        Transform lockedTarget,
        EnemyEquipmentDefinition equipmentDefinitionSnapshot,
        CombatEquipmentSnapshot combatEquipmentSnapshot)
    {
        if (!CanDeliver(lockedTarget, equipmentDefinitionSnapshot, combatEquipmentSnapshot))
        {
            return false;
        }

        Vector2 origin = launchPoint != null ? launchPoint.position : transform.position;
        var request = new ProjectileLaunchRequest(
            combatEquipmentSnapshot.ProjectilePrefab,
            origin,
            lockedTarget.position,
            transform,
            combatEquipmentSnapshot.ProjectileSpeed,
            combatEquipmentSnapshot.Damage,
            combatEquipmentSnapshot.ProjectileLifetime,
            equipmentDefinitionSnapshot.ProjectileTargetLayerMask,
            combatEquipmentSnapshot.ProjectileVisualAngleOffsetDegrees);

        LastLaunchedProjectile = ProjectileLauncher.Launch(request);
        if (LastLaunchedProjectile == null)
        {
            return false;
        }

        Fired?.Invoke(LastLaunchedProjectile);
        return true;
    }

    public bool CanDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        return CanDeliver(lockedTarget, equipmentSnapshot, ResolveLegacySnapshot(equipmentSnapshot));
    }

    public bool TryDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot)
    {
        return TryDeliver(lockedTarget, equipmentSnapshot, ResolveLegacySnapshot(equipmentSnapshot));
    }

    private static CombatEquipmentSnapshot ResolveLegacySnapshot(EnemyEquipmentDefinition equipmentSnapshot)
    {
        if (equipmentSnapshot == null || equipmentSnapshot.CombatProfile == null)
            return default;

        var profile = equipmentSnapshot.CombatProfile;
        return new CombatEquipmentSnapshot(
            profile.EquipmentId,
            profile.DamageBonus,
            AttackRatePolicy.Normalize(profile.AttackRateMultiplier),
            profile.RangeBonus,
            profile.KnockbackBonus,
            profile.RequiredCapabilities,
            profile.HandSprite,
            profile.ProjectilePrefab,
            profile.ProjectileSpeed,
            profile.ProjectileLifetime,
            profile.ProjectileVisualAngleOffsetDegrees);
    }
}

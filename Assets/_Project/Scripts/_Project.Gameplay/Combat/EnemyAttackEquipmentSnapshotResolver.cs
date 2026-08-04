using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using UnityEngine;

public static class EnemyAttackEquipmentSnapshotResolver
{
    public static CombatEquipmentSnapshot Resolve(
        EnemyEquipmentDefinition definition,
        int baseDamage,
        float baseAttackRate,
        EnemyAttackRole attackRole)
    {
        CombatEquipmentProfile profile = definition != null ? definition.CombatProfile : null;
        var baseStats = new CombatBaseStats(baseDamage, baseAttackRate, 0f, 0f);

        if (CombatEquipmentResolver.TryResolve(
            baseStats,
            ResolveCapabilities(attackRole),
            profile,
            out var snapshot))
        {
            return snapshot;
        }

        return new CombatEquipmentSnapshot(
            null,
            Mathf.Max(0, baseDamage),
            AttackRatePolicy.Normalize(baseAttackRate),
            0f,
            0f,
            CombatEquipmentCapability.None,
            null,
            null,
            0f,
            0f,
            0f);
    }

    private static CombatEquipmentCapability ResolveCapabilities(EnemyAttackRole attackRole)
    {
        CombatEquipmentCapability capabilities = CombatEquipmentCapability.HandSocket;
        if (attackRole == EnemyAttackRole.Melee)
            capabilities |= CombatEquipmentCapability.MeleeDelivery;
        else if (attackRole == EnemyAttackRole.Ranged)
            capabilities |= CombatEquipmentCapability.ProjectileDelivery;
        return capabilities;
    }
}

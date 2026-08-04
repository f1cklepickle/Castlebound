using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public static class CombatEquipmentResolver
    {
        public static bool TryResolve(
            CombatBaseStats wearerStats,
            CombatEquipmentCapability holderCapabilities,
            CombatEquipmentProfile profile,
            out CombatEquipmentSnapshot snapshot)
        {
            if (profile == null || !profile.CanEquip(holderCapabilities))
            {
                snapshot = default;
                return false;
            }

            snapshot = new CombatEquipmentSnapshot(
                profile.EquipmentId,
                Mathf.Max(0, wearerStats.Damage + profile.DamageBonus),
                AttackRatePolicy.Normalize(wearerStats.AttackRate * profile.AttackRateMultiplier),
                Mathf.Max(0f, wearerStats.Range + profile.RangeBonus),
                Mathf.Max(0f, wearerStats.Knockback + profile.KnockbackBonus),
                profile.RequiredCapabilities,
                profile.HandSprite,
                profile.ProjectilePrefab,
                profile.ProjectileSpeed,
                profile.ProjectileLifetime,
                profile.ProjectileVisualAngleOffsetDegrees);
            return true;
        }
    }
}

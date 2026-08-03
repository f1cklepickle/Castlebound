using Castlebound.Gameplay.Projectile;
using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public readonly struct CombatEquipmentSnapshot
    {
        public string EquipmentId { get; }
        public int Damage { get; }
        public float AttackRate { get; }
        public float Range { get; }
        public float Knockback { get; }
        public CombatEquipmentCapability RequiredCapabilities { get; }
        public Sprite HandSprite { get; }
        public ProjectileRuntime ProjectilePrefab { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetime { get; }
        public float ProjectileVisualAngleOffsetDegrees { get; }

        public CombatEquipmentSnapshot(
            string equipmentId,
            int damage,
            float attackRate,
            float range,
            float knockback,
            CombatEquipmentCapability requiredCapabilities,
            Sprite handSprite,
            ProjectileRuntime projectilePrefab,
            float projectileSpeed,
            float projectileLifetime,
            float projectileVisualAngleOffsetDegrees)
        {
            EquipmentId = equipmentId;
            Damage = damage;
            AttackRate = attackRate;
            Range = range;
            Knockback = knockback;
            RequiredCapabilities = requiredCapabilities;
            HandSprite = handSprite;
            ProjectilePrefab = projectilePrefab;
            ProjectileSpeed = projectileSpeed;
            ProjectileLifetime = projectileLifetime;
            ProjectileVisualAngleOffsetDegrees = projectileVisualAngleOffsetDegrees;
        }
    }
}

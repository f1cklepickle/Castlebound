using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public struct WeaponStats
    {
        public int Damage { get; }
        public float AttackSpeed { get; }
        public Vector2 HitboxSize { get; }
        public float Knockback { get; }

        public WeaponStats(int damage, float attackSpeed, Vector2 hitboxSize, float knockback)
        {
            Damage = damage;
            AttackSpeed = attackSpeed;
            HitboxSize = hitboxSize;
            Knockback = knockback;
        }
    }
}

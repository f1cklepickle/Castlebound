using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public readonly struct PlayerHitRequest
    {
        public PlayerHitRequest(
            int damage,
            GameObject attacker,
            Vector2 attackOrigin,
            CombatDamageType damageType)
        {
            Damage = Mathf.Max(0, damage);
            Attacker = attacker;
            AttackOrigin = attackOrigin;
            DamageType = damageType;
        }

        public int Damage { get; }
        public GameObject Attacker { get; }
        public Vector2 AttackOrigin { get; }
        public CombatDamageType DamageType { get; }
    }
}

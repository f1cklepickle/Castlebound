using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public readonly struct PlayerHitResult
    {
        public PlayerHitResult(PlayerHitRequest request, PlayerHitOutcome outcome, int appliedDamage)
        {
            Request = request;
            Outcome = outcome;
            AppliedDamage = Mathf.Clamp(appliedDamage, 0, request.Damage);
        }

        public PlayerHitRequest Request { get; }
        public PlayerHitOutcome Outcome { get; }
        public int AppliedDamage { get; }
        public int RequestedDamage => Request.Damage;
        public GameObject Attacker => Request.Attacker;
        public Vector2 AttackOrigin => Request.AttackOrigin;
        public CombatDamageType DamageType => Request.DamageType;
    }
}

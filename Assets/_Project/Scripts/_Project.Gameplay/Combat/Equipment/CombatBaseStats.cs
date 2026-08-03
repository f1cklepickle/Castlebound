namespace Castlebound.Gameplay.Combat
{
    public readonly struct CombatBaseStats
    {
        public int Damage { get; }
        public float AttackRate { get; }
        public float Range { get; }
        public float Knockback { get; }

        public CombatBaseStats(int damage, float attackRate, float range, float knockback)
        {
            Damage = damage;
            AttackRate = attackRate;
            Range = range;
            Knockback = knockback;
        }
    }
}

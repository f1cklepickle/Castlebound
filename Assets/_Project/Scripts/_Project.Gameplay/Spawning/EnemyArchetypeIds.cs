namespace Castlebound.Gameplay.Spawning
{
    public static class EnemyArchetypeIds
    {
        public const string GoblinMelee = "goblin_melee";
        public const string GoblinRanged = "goblin_ranged";
        public const string Lurker = "lurker";

        public const string LegacyGrunt = "grunt";

        public static string Canonicalize(string enemyTypeId)
        {
            return enemyTypeId == LegacyGrunt ? GoblinMelee : enemyTypeId;
        }
    }
}

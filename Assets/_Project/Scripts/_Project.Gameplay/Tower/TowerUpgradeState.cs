namespace Castlebound.Gameplay.Tower
{
    public class TowerUpgradeState
    {
        public int DamageLevel { get; private set; }
        public int FireRateLevel { get; private set; }
        public int HealthLevel { get; private set; }
        public int RangeLevel { get; private set; }

        public int GetLevel(TowerUpgradeTrack track)
        {
            return track switch
            {
                TowerUpgradeTrack.Damage => DamageLevel,
                TowerUpgradeTrack.FireRate => FireRateLevel,
                TowerUpgradeTrack.Health => HealthLevel,
                TowerUpgradeTrack.Range => RangeLevel,
                _ => 0
            };
        }

        public void Increment(TowerUpgradeTrack track)
        {
            switch (track)
            {
                case TowerUpgradeTrack.Damage:
                    DamageLevel++;
                    break;
                case TowerUpgradeTrack.FireRate:
                    FireRateLevel++;
                    break;
                case TowerUpgradeTrack.Health:
                    HealthLevel++;
                    break;
                case TowerUpgradeTrack.Range:
                    RangeLevel++;
                    break;
            }
        }
    }
}

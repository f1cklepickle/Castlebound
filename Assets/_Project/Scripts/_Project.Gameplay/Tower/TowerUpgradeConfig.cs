using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [CreateAssetMenu(menuName = "Castlebound/Tower/Tower Upgrade Config")]
    public class TowerUpgradeConfig : ScriptableObject
    {
        [SerializeField] private TowerUpgradeTrackConfig damage = new TowerUpgradeTrackConfig();
        [SerializeField] private TowerUpgradeTrackConfig fireRate = new TowerUpgradeTrackConfig();
        [SerializeField] private TowerUpgradeTrackConfig health = new TowerUpgradeTrackConfig();
        [SerializeField] private TowerUpgradeTrackConfig range = new TowerUpgradeTrackConfig();

        public TowerUpgradeTrackConfig Damage => damage;
        public TowerUpgradeTrackConfig FireRate => fireRate;
        public TowerUpgradeTrackConfig Health => health;
        public TowerUpgradeTrackConfig Range => range;

        public TowerUpgradeTrackConfig GetTrack(TowerUpgradeTrack track)
        {
            return track switch
            {
                TowerUpgradeTrack.Damage => damage,
                TowerUpgradeTrack.FireRate => fireRate,
                TowerUpgradeTrack.Health => health,
                TowerUpgradeTrack.Range => range,
                _ => null
            };
        }

        public bool CanUpgrade(TowerUpgradeTrack track, TowerUpgradeState state)
        {
            var trackConfig = GetTrack(track);
            return trackConfig != null && state != null && trackConfig.CanUpgrade(state.GetLevel(track));
        }

        public bool IsTrackEnabled(TowerUpgradeTrack track)
        {
            var trackConfig = GetTrack(track);
            return trackConfig != null && trackConfig.Enabled;
        }

        public int GetCost(TowerUpgradeTrack track, TowerUpgradeState state)
        {
            var trackConfig = GetTrack(track);
            return trackConfig != null && state != null ? trackConfig.GetCostForLevel(state.GetLevel(track)) : 0;
        }

        public int GetDamage(TowerUpgradeState state) => Mathf.RoundToInt(damage.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.Damage)));
        public float GetCooldownSeconds(TowerUpgradeState state) => fireRate.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.FireRate));
        public int GetMaxHealth(TowerUpgradeState state) => Mathf.RoundToInt(health.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.Health)));
        public float GetMaxRange(TowerUpgradeState state) => range.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.Range));

        private void OnValidate()
        {
            damage?.Normalize();
            fireRate?.Normalize();
            health?.Normalize();
            range?.Normalize();
        }

        private static int GetLevel(TowerUpgradeState state, TowerUpgradeTrack track)
        {
            return state != null ? state.GetLevel(track) : 0;
        }
    }
}

using Castlebound.Gameplay.Balance;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [CreateAssetMenu(menuName = "Castlebound/Tower/Tower Upgrade Config")]
    public class TowerUpgradeConfig : ScriptableObject
    {
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private TowerUpgradeTrackConfig damage = new TowerUpgradeTrackConfig();
        [SerializeField] private TowerUpgradeTrackConfig fireRate = new TowerUpgradeTrackConfig();
        [SerializeField] private TowerUpgradeTrackConfig health = new TowerUpgradeTrackConfig();
        [SerializeField] private TowerUpgradeTrackConfig range = new TowerUpgradeTrackConfig();

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public TowerUpgradeTrackConfig Damage => GetTrack(TowerUpgradeTrack.Damage);
        public TowerUpgradeTrackConfig FireRate => GetTrack(TowerUpgradeTrack.FireRate);
        public TowerUpgradeTrackConfig Health => GetTrack(TowerUpgradeTrack.Health);
        public TowerUpgradeTrackConfig Range => GetTrack(TowerUpgradeTrack.Range);

        public TowerUpgradeTrackConfig GetTrack(TowerUpgradeTrack track)
        {
            var tableTrack = ActiveTowerTable != null ? ActiveTowerTable.GetTrack(track) : null;
            if (tableTrack != null)
            {
                return tableTrack;
            }

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

        public int GetDamage(TowerUpgradeState state) => Mathf.RoundToInt(Damage.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.Damage)));
        public float GetCooldownSeconds(TowerUpgradeState state) => FireRate.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.FireRate));
        public int GetMaxHealth(TowerUpgradeState state) => Mathf.RoundToInt(Health.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.Health)));
        public float GetMaxRange(TowerUpgradeState state) => Range.GetValueForLevel(GetLevel(state, TowerUpgradeTrack.Range));

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

        private TowerBalanceTable ActiveTowerTable => balanceStation != null ? balanceStation.Tower : null;
    }
}

using Castlebound.Gameplay.Tower;
using UnityEngine;

namespace Castlebound.Gameplay.Balance
{
    [CreateAssetMenu(menuName = "Castlebound/Balance/Tower Balance Table")]
    public class TowerBalanceTable : ScriptableObject
    {
        [Header("Build")]
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private int buildCost = 50;
        [SerializeField] private int baseMaxHealth = 10;
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private int baseUpgradeCost = 75;

        [Header("Combat")]
        [SerializeField] private float baseCooldownSeconds = 1f;
        [SerializeField] private float baseMaxRange = 5f;

        [Header("Upgrade Tracks")]
        [SerializeField] private TowerUpgradeTrackConfig damage = CreateDamageTrack();
        [SerializeField] private TowerUpgradeTrackConfig fireRate = CreateFireRateTrack();
        [SerializeField] private TowerUpgradeTrackConfig health = CreateHealthTrack();
        [SerializeField] private TowerUpgradeTrackConfig range = CreateRangeTrack();

        public GameObject TowerPrefab
        {
            get => towerPrefab;
            set => towerPrefab = value;
        }

        public int BuildCost
        {
            get => buildCost;
            set => buildCost = Mathf.Max(0, value);
        }

        public int BaseMaxHealth
        {
            get => baseMaxHealth;
            set => baseMaxHealth = Mathf.Max(0, value);
        }

        public int BaseDamage
        {
            get => baseDamage;
            set => baseDamage = Mathf.Max(0, value);
        }

        public int BaseUpgradeCost
        {
            get => baseUpgradeCost;
            set => baseUpgradeCost = Mathf.Max(0, value);
        }

        public float BaseCooldownSeconds
        {
            get => baseCooldownSeconds;
            set => baseCooldownSeconds = Mathf.Max(0f, value);
        }

        public float BaseMaxRange
        {
            get => baseMaxRange;
            set => baseMaxRange = Mathf.Max(0f, value);
        }

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
            BuildCost = buildCost;
            BaseMaxHealth = baseMaxHealth;
            BaseDamage = baseDamage;
            BaseUpgradeCost = baseUpgradeCost;
            BaseCooldownSeconds = baseCooldownSeconds;
            BaseMaxRange = baseMaxRange;
            damage?.Normalize();
            fireRate?.Normalize();
            health?.Normalize();
            range?.Normalize();
        }

        private static int GetLevel(TowerUpgradeState state, TowerUpgradeTrack track)
        {
            return state != null ? state.GetLevel(track) : 0;
        }

        private static TowerUpgradeTrackConfig CreateDamageTrack()
        {
            return CreateTrack(maxLevel: 5, baseValue: 1f, valuePerLevel: 1f, minValue: 0f, baseCost: 75, costPerLevel: 25);
        }

        private static TowerUpgradeTrackConfig CreateFireRateTrack()
        {
            return CreateTrack(maxLevel: 4, baseValue: 1f, valuePerLevel: -0.1f, minValue: 0.45f, baseCost: 85, costPerLevel: 30);
        }

        private static TowerUpgradeTrackConfig CreateHealthTrack()
        {
            return CreateTrack(maxLevel: 3, baseValue: 10f, valuePerLevel: 3f, minValue: 1f, baseCost: 60, costPerLevel: 20);
        }

        private static TowerUpgradeTrackConfig CreateRangeTrack()
        {
            return CreateTrack(maxLevel: 3, baseValue: 5f, valuePerLevel: 0.5f, minValue: 0f, baseCost: 80, costPerLevel: 25);
        }

        private static TowerUpgradeTrackConfig CreateTrack(
            int maxLevel,
            float baseValue,
            float valuePerLevel,
            float minValue,
            int baseCost,
            int costPerLevel)
        {
            var track = new TowerUpgradeTrackConfig
            {
                Enabled = true,
                MaxLevel = maxLevel,
                BaseValue = baseValue,
                ValuePerLevel = valuePerLevel,
                MinValue = minValue,
                BaseCost = baseCost,
                CostPerLevel = costPerLevel
            };
            return track;
        }
    }
}

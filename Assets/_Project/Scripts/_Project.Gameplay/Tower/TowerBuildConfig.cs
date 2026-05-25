using Castlebound.Gameplay.Balance;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [CreateAssetMenu(menuName = "Castlebound/Tower/Tower Build Config")]
    public class TowerBuildConfig : ScriptableObject
    {
        [SerializeField] private GameBalanceStation balanceStation;
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private int buildCost = 50;
        [SerializeField] private int baseMaxHealth = 10;
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private int baseUpgradeCost = 75;

        public GameBalanceStation BalanceStation
        {
            get => balanceStation;
            set => balanceStation = value;
        }

        public GameObject TowerPrefab
        {
            get => ActiveTowerTable != null && ActiveTowerTable.TowerPrefab != null ? ActiveTowerTable.TowerPrefab : towerPrefab;
            set => towerPrefab = value;
        }

        public int BuildCost
        {
            get => ActiveTowerTable != null ? ActiveTowerTable.BuildCost : buildCost;
            set => buildCost = Mathf.Max(0, value);
        }

        public int BaseMaxHealth
        {
            get => ActiveTowerTable != null ? ActiveTowerTable.BaseMaxHealth : baseMaxHealth;
            set => baseMaxHealth = Mathf.Max(0, value);
        }

        public int BaseDamage
        {
            get => ActiveTowerTable != null ? ActiveTowerTable.BaseDamage : baseDamage;
            set => baseDamage = Mathf.Max(0, value);
        }

        public int BaseUpgradeCost
        {
            get => ActiveTowerTable != null ? ActiveTowerTable.BaseUpgradeCost : baseUpgradeCost;
            set => baseUpgradeCost = Mathf.Max(0, value);
        }

        public float BaseCooldownSeconds => ActiveTowerTable != null ? ActiveTowerTable.BaseCooldownSeconds : 1f;
        public float BaseMaxRange => ActiveTowerTable != null ? ActiveTowerTable.BaseMaxRange : 5f;

        private TowerBalanceTable ActiveTowerTable => balanceStation != null ? balanceStation.Tower : null;
    }
}

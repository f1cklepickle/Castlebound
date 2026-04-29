using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    [CreateAssetMenu(menuName = "Castlebound/Tower/Tower Build Config")]
    public class TowerBuildConfig : ScriptableObject
    {
        [SerializeField] private GameObject towerPrefab;
        [SerializeField] private int buildCost = 50;
        [SerializeField] private int baseMaxHealth = 10;
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private int baseUpgradeCost = 75;

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
    }
}

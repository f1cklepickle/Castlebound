using UnityEngine;

namespace Castlebound.Gameplay.Loot
{
    [CreateAssetMenu(menuName = "Castlebound/Loot/Enemy Loot Profile")]
    public class EnemyLootProfile : ScriptableObject
    {
        [SerializeField] private LootDropper.LootTableMapping[] lootTables;
        [SerializeField] private int globalMaxTables = 10;

        public LootDropper.LootTableMapping[] LootTables
        {
            get => lootTables;
            set => lootTables = value;
        }

        public int GlobalMaxTables
        {
            get => globalMaxTables;
            set => globalMaxTables = Mathf.Max(0, value);
        }

        private void OnValidate()
        {
            GlobalMaxTables = globalMaxTables;
        }
    }
}

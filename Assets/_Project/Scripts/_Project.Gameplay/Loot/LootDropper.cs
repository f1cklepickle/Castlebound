using System;
using Castlebound.Gameplay.Inventory;
using UnityEngine;
using Random = System.Random;

namespace Castlebound.Gameplay.Loot
{
    public class LootDropper : MonoBehaviour
    {
        [Serializable]
        public struct LootTableMapping
        {
            [SerializeField] private LootTable table;
            [SerializeField] private ItemPickupComponent prefab;
            [SerializeField] private int maxDrops;

            public LootTable Table => table;
            public ItemPickupComponent Prefab => prefab;
            public int MaxDrops => maxDrops;

            public LootTableMapping(LootTable table, ItemPickupComponent prefab, int maxDrops)
            {
                this.table = table;
                this.prefab = prefab;
                this.maxDrops = maxDrops;
            }
        }

        [SerializeField] private LootTableMapping[] lootTables;
        [SerializeField] private int maxDrops = 10;
        [SerializeField] private int xpAmount = 0;

        private LootRollResult[] cachedResults = Array.Empty<LootRollResult>();
        private Health health;

        public void SetLootTable(LootTable table)
        {
            lootTables = table != null ? new[] { new LootTableMapping(table, null, maxDrops) } : null;
        }

        public void SetPickupPrefab(ItemPickupComponent prefab)
        {
            if (lootTables == null || lootTables.Length == 0)
            {
                lootTables = new[] { new LootTableMapping(null, prefab, maxDrops) };
                return;
            }

            lootTables[0] = new LootTableMapping(lootTables[0].Table, prefab, lootTables[0].MaxDrops);
        }

        public void SetXpAmount(int amount)
        {
            xpAmount = amount;
        }

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (health != null)
            {
                health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDied -= HandleDied;
            }
        }

        private void Start()
        {
            if (lootTables == null || lootTables.Length == 0 || GameManager.I == null)
            {
                return;
            }

            PreRoll(GameManager.I.LootRng, 0, maxDrops);
        }

        public void PreRoll(Random rng, int waveIndex, int maxDropsOverride = 0)
        {
            if (lootTables == null || lootTables.Length == 0 || rng == null)
            {
                cachedResults = Array.Empty<LootRollResult>();
                return;
            }

            int cap = maxDropsOverride > 0 ? maxDropsOverride : maxDrops;
            var combined = new System.Collections.Generic.List<LootRollResult>();
            for (int i = 0; i < lootTables.Length; i++)
            {
                var mapping = lootTables[i];
                var table = mapping.Table;
                if (table == null)
                {
                    continue;
                }

                int tableCap = mapping.MaxDrops > 0 ? mapping.MaxDrops : cap;
                var results = table.RollMany(rng, waveIndex, tableCap);
                if (results.Length > 0)
                {
                    combined.AddRange(results);
                }
            }

            cachedResults = combined.Count > 0 ? combined.ToArray() : Array.Empty<LootRollResult>();
        }

        private void HandleDied()
        {
            GrantXp();
            SpawnDrops(transform.position);
        }

        public ItemPickupComponent[] SpawnDrops(Vector3 position)
        {
            if (cachedResults.Length == 0)
            {
                return Array.Empty<ItemPickupComponent>();
            }

            var spawned = new ItemPickupComponent[cachedResults.Length];
            for (int i = 0; i < cachedResults.Length; i++)
            {
                var result = cachedResults[i];
                if (result.IsEmpty)
                {
                    continue;
                }

                var instance = SpawnPickupForResult(result, position);
                if (instance != null)
                {
                    spawned[i] = instance;
                }
            }

            return spawned;
        }

        private ItemPickupComponent SpawnPickupForResult(LootRollResult result, Vector3 position)
        {
            var item = result.Item;
            var prefab = ResolvePrefab(result);
            if (prefab == null)
            {
                return null;
            }

            var instance = Instantiate(prefab, position, Quaternion.identity);
            if (item is WeaponDefinition weapon)
            {
                instance.Kind = ItemPickupKind.Weapon;
                instance.ItemDefinition = weapon;
                instance.Amount = 1;
                return instance;
            }

            if (item is PotionDefinition potion)
            {
                instance.Kind = ItemPickupKind.Potion;
                instance.ItemDefinition = potion;
                instance.Amount = result.Amount;
                return instance;
            }

            instance.Kind = ItemPickupKind.Gold;
            instance.ItemDefinition = item;
            instance.Amount = result.Amount;
            return instance;
        }

        private ItemPickupComponent ResolvePrefab(LootRollResult result)
        {
            if (lootTables == null)
            {
                return null;
            }

            for (int i = 0; i < lootTables.Length; i++)
            {
                var mapping = lootTables[i];
                if (mapping.Table == null || mapping.Prefab == null)
                {
                    continue;
                }

                if (mapping.Table.ContainsItem(result.Item))
                {
                    return mapping.Prefab;
                }
            }

            return null;
        }

        private void GrantXp()
        {
            if (xpAmount <= 0)
            {
                return;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            var inventoryComponent = player.GetComponentInChildren<InventoryStateComponent>();
            if (inventoryComponent == null)
            {
                return;
            }

            inventoryComponent.State.AddXp(xpAmount);
        }

    }
}

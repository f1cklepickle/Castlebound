using System;
using Castlebound.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.Serialization;
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
            [FormerlySerializedAs("maxDrops")]
            [SerializeField] private int maxRolls;
            [SerializeField] private int maxSpawns;

            public LootTable Table => table;
            public ItemPickupComponent Prefab => prefab;
            public int MaxRolls => maxRolls;
            public int MaxSpawns => maxSpawns;

            public LootTableMapping(LootTable table, ItemPickupComponent prefab, int maxRolls, int maxSpawns)
            {
                this.table = table;
                this.prefab = prefab;
                this.maxRolls = maxRolls;
                this.maxSpawns = maxSpawns;
            }
        }

        [SerializeField] private LootTableMapping[] lootTables;
        [FormerlySerializedAs("maxDrops")]
        [SerializeField] private int maxRolls = 10;
        [FormerlySerializedAs("globalMaxDrops")]
        [SerializeField] private int globalMaxTables = 10;
        [SerializeField] private int xpAmount = 0;

        private LootRollResult[] cachedResults = Array.Empty<LootRollResult>();
        private Health health;

        public void SetLootTable(LootTable table)
        {
            lootTables = table != null ? new[] { new LootTableMapping(table, null, maxRolls, 0) } : null;
        }

        public void SetPickupPrefab(ItemPickupComponent prefab)
        {
            if (lootTables == null || lootTables.Length == 0)
            {
                lootTables = new[] { new LootTableMapping(null, prefab, maxRolls, 0) };
                return;
            }

            lootTables[0] = new LootTableMapping(lootTables[0].Table, prefab, lootTables[0].MaxRolls, lootTables[0].MaxSpawns);
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

            PreRoll(GameManager.I.LootRng, 0, globalMaxTables);
        }

        public void PreRoll(Random rng, int waveIndex, int maxDropsOverride = 0)
        {
            if (lootTables == null || lootTables.Length == 0 || rng == null)
            {
                cachedResults = Array.Empty<LootRollResult>();
                return;
            }

            int cap = maxDropsOverride > 0 ? maxDropsOverride : globalMaxTables;
            var combined = new System.Collections.Generic.List<LootRollResult>();
            var tableChances = new System.Collections.Generic.List<float>();
            for (int i = 0; i < lootTables.Length; i++)
            {
                var mapping = lootTables[i];
                var table = mapping.Table;
                if (table == null)
                {
                    continue;
                }

                int tableCap = mapping.MaxRolls > 0 ? mapping.MaxRolls : maxRolls;
                var results = table.RollMany(rng, waveIndex, tableCap);
                if (results.Length > 0)
                {
                    for (int j = 0; j < results.Length; j++)
                    {
                        combined.Add(results[j]);
                        tableChances.Add(Mathf.Clamp01(table.DropChance));
                    }
                }
            }

            if (combined.Count == 0)
            {
                cachedResults = Array.Empty<LootRollResult>();
                return;
            }

            if (cap > 0 && combined.Count > cap)
            {
                var indices = new int[combined.Count];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = i;
                }

                Array.Sort(indices, (a, b) => tableChances[a].CompareTo(tableChances[b]));

                var trimmed = new LootRollResult[cap];
                for (int i = 0; i < cap; i++)
                {
                    trimmed[i] = combined[indices[i]];
                }

                cachedResults = trimmed;
                return;
            }

            cachedResults = combined.ToArray();
        }

        private void HandleDied()
        {
            GrantXp();
            SpawnDrops(transform.position);
        }

        public ItemPickupComponent[] SpawnDrops(Vector3 position)
        {
            var requests = LootSpawnPolicy.BuildRequests(cachedResults, lootTables);
            if (requests.Length == 0)
            {
                return Array.Empty<ItemPickupComponent>();
            }

            var spawned = new ItemPickupComponent[requests.Length];
            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                if (request.Prefab == null)
                {
                    continue;
                }

                var instance = Instantiate(request.Prefab, position, Quaternion.identity);
                instance.Kind = request.Kind;
                instance.ItemDefinition = request.Item;
                instance.Amount = request.Amount;
                spawned[i] = instance;
            }

            return spawned;
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

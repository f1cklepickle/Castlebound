using System.Collections.Generic;
using Castlebound.Gameplay.Inventory;
using UnityEngine;

namespace Castlebound.Gameplay.Loot
{
    public static class LootSpawnPolicy
    {
        public static LootSpawnRequest[] BuildRequests(LootRollResult[] results, LootDropper.LootTableMapping[] mappings)
        {
            if (results == null || results.Length == 0)
            {
                return System.Array.Empty<LootSpawnRequest>();
            }

            var requests = new List<LootSpawnRequest>();
            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                if (result.IsEmpty)
                {
                    continue;
                }

                var prefab = ResolvePrefab(result, mappings);
                if (prefab == null)
                {
                    continue;
                }

                if (result.Item is WeaponDefinition weapon)
                {
                    requests.Add(new LootSpawnRequest(prefab, ItemPickupKind.Weapon, weapon, 1));
                    continue;
                }

                if (result.Item is PotionDefinition potion)
                {
                    requests.Add(new LootSpawnRequest(prefab, ItemPickupKind.Potion, potion, result.Amount));
                    continue;
                }

                int spawnCap = ResolveSpawnCap(result, mappings);
                int splitCount = spawnCap > 0 ? Mathf.Min(result.Amount, spawnCap) : result.Amount;
                if (splitCount <= 0)
                {
                    continue;
                }

                int baseAmount = result.Amount / splitCount;
                int remainder = result.Amount % splitCount;
                for (int j = 0; j < splitCount; j++)
                {
                    int amount = baseAmount + (j < remainder ? 1 : 0);
                    if (amount <= 0)
                    {
                        continue;
                    }

                    requests.Add(new LootSpawnRequest(prefab, ItemPickupKind.Gold, result.Item, amount));
                }
            }

            return requests.Count == 0 ? System.Array.Empty<LootSpawnRequest>() : requests.ToArray();
        }

        private static ItemPickupComponent ResolvePrefab(LootRollResult result, LootDropper.LootTableMapping[] mappings)
        {
            if (mappings == null)
            {
                return null;
            }

            for (int i = 0; i < mappings.Length; i++)
            {
                var mapping = mappings[i];
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

        private static int ResolveSpawnCap(LootRollResult result, LootDropper.LootTableMapping[] mappings)
        {
            if (mappings == null)
            {
                return 0;
            }

            for (int i = 0; i < mappings.Length; i++)
            {
                var mapping = mappings[i];
                if (mapping.Table == null)
                {
                    continue;
                }

                if (mapping.Table.ContainsItem(result.Item))
                {
                    return mapping.MaxSpawns;
                }
            }

            return 0;
        }
    }
}

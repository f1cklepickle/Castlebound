using System;
using Random = System.Random;
using Castlebound.Gameplay.Inventory;
using UnityEngine;

namespace Castlebound.Gameplay.Loot
{
    [CreateAssetMenu(menuName = "Castlebound/Loot/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
        [SerializeField] private LootEntry[] entries;

        public LootEntry[] Entries
        {
            get => entries;
            set => entries = value;
        }

        public float DropChance
        {
            get => dropChance;
            set => dropChance = Mathf.Clamp01(value);
        }

        public LootRollResult Roll(Random rng, int waveIndex)
        {
            if (rng == null || entries == null || entries.Length == 0)
            {
                return new LootRollResult(null, 0);
            }

            if (dropChance < 1f && rng.NextDouble() > dropChance)
            {
                return new LootRollResult(null, 0);
            }

            float totalWeight = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                totalWeight += Mathf.Max(0f, entries[i].Weight);
            }

            if (totalWeight <= 0f)
            {
                return new LootRollResult(null, 0);
            }

            float roll = (float)(rng.NextDouble() * totalWeight);
            for (int i = 0; i < entries.Length; i++)
            {
                float weight = Mathf.Max(0f, entries[i].Weight);
                if (roll <= weight)
                {
                    return BuildResult(entries[i], rng);
                }

                roll -= weight;
            }

            return new LootRollResult(null, 0);
        }

        public bool ContainsItem(ItemDefinition item)
        {
            if (item == null || entries == null)
            {
                return false;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Item == item)
                {
                    return true;
                }
            }

            return false;
        }

        public LootRollResult[] RollMany(Random rng, int waveIndex, int maxDrops)
        {
            if (rng == null || entries == null || entries.Length == 0 || maxDrops <= 0)
            {
                return Array.Empty<LootRollResult>();
            }

            if (dropChance < 1f && rng.NextDouble() > dropChance)
            {
                return Array.Empty<LootRollResult>();
            }

            float totalWeight = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                totalWeight += Mathf.Max(0f, entries[i].Weight);
            }

            if (totalWeight <= 0f)
            {
                return Array.Empty<LootRollResult>();
            }

            var results = new LootRollResult[Mathf.Max(1, maxDrops)];
            int count = 0;
            for (int i = 0; i < maxDrops; i++)
            {
                float roll = (float)(rng.NextDouble() * totalWeight);
                for (int j = 0; j < entries.Length; j++)
                {
                    float weight = Mathf.Max(0f, entries[j].Weight);
                    if (roll <= weight)
                    {
                        var result = BuildResult(entries[j], rng);
                        if (!result.IsEmpty)
                        {
                            results[count++] = result;
                        }
                        break;
                    }

                    roll -= weight;
                }
            }

            if (count == 0)
            {
                return Array.Empty<LootRollResult>();
            }

            if (count == results.Length)
            {
                return results;
            }

            var trimmed = new LootRollResult[count];
            Array.Copy(results, trimmed, count);
            return trimmed;
        }

        private LootRollResult BuildResult(LootEntry entry, Random rng)
        {
            ItemDefinition item = entry.Item;
            if (item == null)
            {
                return new LootRollResult(null, 0);
            }

            int min = Mathf.Max(0, entry.MinAmount);
            int max = Mathf.Max(min, entry.MaxAmount);
            int amount = max > min ? rng.Next(min, max + 1) : min;
            return new LootRollResult(item, amount);
        }
    }
}

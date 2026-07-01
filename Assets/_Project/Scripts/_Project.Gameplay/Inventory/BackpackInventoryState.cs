using System;
using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class BackpackInventoryState
    {
        private readonly Dictionary<string, int> itemCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private int maxItemCount;

        public BackpackInventoryState(int maxItemCount = 8)
        {
            MaxItemCount = maxItemCount;
        }

        public event Action OnInventoryChanged;

        public int MaxItemCount
        {
            get => maxItemCount;
            set => maxItemCount = Mathf.Max(0, value);
        }

        public int ItemCount { get; private set; }
        public int EntryCount => itemCounts.Count;

        public IReadOnlyList<BackpackInventoryEntry> Entries
        {
            get
            {
                var entries = new List<BackpackInventoryEntry>(itemCounts.Count);
                foreach (var pair in itemCounts)
                {
                    entries.Add(new BackpackInventoryEntry(pair.Key, pair.Value));
                }

                entries.Sort((a, b) => string.CompareOrdinal(a.ItemId, b.ItemId));
                return entries;
            }
        }

        public int GetCount(string itemId)
        {
            if (!IsValidItemId(itemId))
            {
                return 0;
            }

            return itemCounts.TryGetValue(itemId, out var count) ? count : 0;
        }

        public bool CanAddItem(string itemId, int amount)
        {
            return IsValidItemId(itemId) && amount > 0 && ItemCount + amount <= MaxItemCount;
        }

        public bool AddItem(string itemId, int amount)
        {
            if (!CanAddItem(itemId, amount))
            {
                return false;
            }

            itemCounts.TryGetValue(itemId, out var currentCount);
            itemCounts[itemId] = currentCount + amount;
            ItemCount += amount;
            RaiseChanged();
            return true;
        }

        public bool TryRemoveItem(string itemId, int amount)
        {
            if (!IsValidItemId(itemId) || amount <= 0)
            {
                return false;
            }

            if (!itemCounts.TryGetValue(itemId, out var currentCount) || currentCount < amount)
            {
                return false;
            }

            var remaining = currentCount - amount;
            if (remaining == 0)
            {
                itemCounts.Remove(itemId);
            }
            else
            {
                itemCounts[itemId] = remaining;
            }

            ItemCount -= amount;
            RaiseChanged();
            return true;
        }

        public bool Clear()
        {
            if (ItemCount == 0)
            {
                return false;
            }

            itemCounts.Clear();
            ItemCount = 0;
            RaiseChanged();
            return true;
        }

        private static bool IsValidItemId(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId);
        }

        private void RaiseChanged()
        {
            OnInventoryChanged?.Invoke();
        }
    }
}

using System;
using System.Collections.Generic;

namespace Castlebound.Gameplay.Inventory
{
    public class CastleInventoryState
    {
        private readonly Dictionary<string, int> itemCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        public event Action OnInventoryChanged;

        public int EntryCount => itemCounts.Count;

        public IReadOnlyList<CastleInventoryEntry> Entries
        {
            get
            {
                var entries = new List<CastleInventoryEntry>(itemCounts.Count);
                foreach (var pair in itemCounts)
                {
                    entries.Add(new CastleInventoryEntry(pair.Key, pair.Value));
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

        public bool AddItem(string itemId, int amount)
        {
            if (!IsValidItemId(itemId) || amount <= 0)
            {
                return false;
            }

            itemCounts.TryGetValue(itemId, out var currentCount);
            itemCounts[itemId] = currentCount + amount;
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

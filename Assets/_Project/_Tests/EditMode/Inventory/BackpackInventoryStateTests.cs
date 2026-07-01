using System.Collections.Generic;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;

namespace Castlebound.Tests.Inventory
{
    public class BackpackInventoryStateTests
    {
        [Test]
        public void AddItem_WithCapacity_AddsCountAndRaisesChanged()
        {
            var backpack = new BackpackInventoryState(maxItemCount: 2);
            var recorder = new EventRecorder();
            backpack.OnInventoryChanged += recorder.Record;

            var added = backpack.AddItem("weapon_sword", 1);

            Assert.IsTrue(added);
            Assert.That(backpack.GetCount("weapon_sword"), Is.EqualTo(1));
            Assert.That(backpack.ItemCount, Is.EqualTo(1));
            Assert.That(backpack.EntryCount, Is.EqualTo(1));
            Assert.That(recorder.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddItem_WhenCapacityExceeded_IsRejectedAndDoesNotRaiseChanged()
        {
            var backpack = new BackpackInventoryState(maxItemCount: 1);
            var recorder = new EventRecorder();
            backpack.AddItem("weapon_a", 1);
            backpack.OnInventoryChanged += recorder.Record;

            var added = backpack.AddItem("weapon_b", 1);

            Assert.IsFalse(added);
            Assert.That(backpack.GetCount("weapon_b"), Is.EqualTo(0));
            Assert.That(backpack.ItemCount, Is.EqualTo(1));
            Assert.That(recorder.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddItem_InvalidInput_IsRejected()
        {
            var backpack = new BackpackInventoryState(maxItemCount: 2);

            Assert.IsFalse(backpack.AddItem(null, 1));
            Assert.IsFalse(backpack.AddItem("", 1));
            Assert.IsFalse(backpack.AddItem("   ", 1));
            Assert.IsFalse(backpack.AddItem("weapon_sword", 0));
            Assert.IsFalse(backpack.AddItem("weapon_sword", -1));
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
        }

        [Test]
        public void TryRemoveItem_WithEnoughCount_DecrementsCount()
        {
            var backpack = new BackpackInventoryState(maxItemCount: 3);
            backpack.AddItem("weapon_sword", 2);

            var removed = backpack.TryRemoveItem("weapon_sword", 1);

            Assert.IsTrue(removed);
            Assert.That(backpack.GetCount("weapon_sword"), Is.EqualTo(1));
            Assert.That(backpack.ItemCount, Is.EqualTo(1));
        }

        [Test]
        public void Clear_WithContents_RemovesAllItemsAndRaisesChanged()
        {
            var backpack = new BackpackInventoryState(maxItemCount: 3);
            var recorder = new EventRecorder();
            backpack.AddItem("weapon_sword", 1);
            backpack.OnInventoryChanged += recorder.Record;

            var cleared = backpack.Clear();

            Assert.IsTrue(cleared);
            Assert.That(backpack.ItemCount, Is.EqualTo(0));
            Assert.That(backpack.EntryCount, Is.EqualTo(0));
            Assert.That(recorder.Count, Is.EqualTo(1));
        }

        [Test]
        public void Entries_ReturnSortedSnapshotThatCannotMutateBackpack()
        {
            var backpack = new BackpackInventoryState(maxItemCount: 3);
            backpack.AddItem("weapon_sword", 1);
            backpack.AddItem("potion_basic", 1);

            var entries = backpack.Entries;

            Assert.That(entries[0].ItemId, Is.EqualTo("potion_basic"));
            Assert.That(entries[1].ItemId, Is.EqualTo("weapon_sword"));

            var mutableSnapshot = entries as IList<BackpackInventoryEntry>;
            Assert.NotNull(mutableSnapshot);
            mutableSnapshot.Clear();

            Assert.That(backpack.EntryCount, Is.EqualTo(2));
            Assert.That(backpack.ItemCount, Is.EqualTo(2));
        }

        private sealed class EventRecorder
        {
            public int Count { get; private set; }

            public void Record()
            {
                Count++;
            }
        }
    }
}

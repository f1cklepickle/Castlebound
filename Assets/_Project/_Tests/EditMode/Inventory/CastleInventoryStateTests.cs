using System.Collections.Generic;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;

namespace Castlebound.Tests.Inventory
{
    public class CastleInventoryStateTests
    {
        [Test]
        public void AddItem_WithValidItem_AddsCountAndRaisesChanged()
        {
            var state = new CastleInventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            var added = state.AddItem("potion_basic", 2);

            Assert.IsTrue(added);
            Assert.That(state.GetCount("potion_basic"), Is.EqualTo(2));
            Assert.That(state.EntryCount, Is.EqualTo(1));
            Assert.That(recorder.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddItem_ExistingItem_IncrementsCount()
        {
            var state = new CastleInventoryState();

            state.AddItem("potion_basic", 2);
            state.AddItem("potion_basic", 3);

            Assert.That(state.GetCount("potion_basic"), Is.EqualTo(5));
            Assert.That(state.EntryCount, Is.EqualTo(1));
        }

        [Test]
        public void AddItem_InvalidInput_IsRejectedAndDoesNotRaiseChanged()
        {
            var state = new CastleInventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            Assert.IsFalse(state.AddItem(null, 1));
            Assert.IsFalse(state.AddItem("", 1));
            Assert.IsFalse(state.AddItem("   ", 1));
            Assert.IsFalse(state.AddItem("potion_basic", 0));
            Assert.IsFalse(state.AddItem("potion_basic", -1));

            Assert.That(state.EntryCount, Is.EqualTo(0));
            Assert.That(recorder.Count, Is.EqualTo(0));
        }

        [Test]
        public void TryRemoveItem_WithEnoughCount_DecrementsCountAndRaisesChanged()
        {
            var state = new CastleInventoryState();
            var recorder = new EventRecorder();
            state.AddItem("potion_basic", 5);
            state.OnInventoryChanged += recorder.Record;

            var removed = state.TryRemoveItem("potion_basic", 2);

            Assert.IsTrue(removed);
            Assert.That(state.GetCount("potion_basic"), Is.EqualTo(3));
            Assert.That(recorder.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryRemoveItem_WhenCountReachesZero_RemovesEntry()
        {
            var state = new CastleInventoryState();
            state.AddItem("potion_basic", 2);

            var removed = state.TryRemoveItem("potion_basic", 2);

            Assert.IsTrue(removed);
            Assert.That(state.GetCount("potion_basic"), Is.EqualTo(0));
            Assert.That(state.EntryCount, Is.EqualTo(0));
        }

        [Test]
        public void TryRemoveItem_WithInvalidOrInsufficientCount_IsRejectedAndDoesNotRaiseChanged()
        {
            var state = new CastleInventoryState();
            var recorder = new EventRecorder();
            state.AddItem("potion_basic", 1);
            state.OnInventoryChanged += recorder.Record;

            Assert.IsFalse(state.TryRemoveItem(null, 1));
            Assert.IsFalse(state.TryRemoveItem("", 1));
            Assert.IsFalse(state.TryRemoveItem("   ", 1));
            Assert.IsFalse(state.TryRemoveItem("potion_basic", 0));
            Assert.IsFalse(state.TryRemoveItem("potion_basic", -1));
            Assert.IsFalse(state.TryRemoveItem("missing_item", 1));
            Assert.IsFalse(state.TryRemoveItem("potion_basic", 2));

            Assert.That(state.GetCount("potion_basic"), Is.EqualTo(1));
            Assert.That(recorder.Count, Is.EqualTo(0));
        }

        [Test]
        public void Entries_ReturnSnapshotThatCannotMutateVault()
        {
            var state = new CastleInventoryState();
            state.AddItem("potion_basic", 2);

            var entries = state.Entries;
            Assert.That(entries.Count, Is.EqualTo(1));
            Assert.That(entries[0].ItemId, Is.EqualTo("potion_basic"));
            Assert.That(entries[0].Count, Is.EqualTo(2));

            var mutableSnapshot = entries as IList<CastleInventoryEntry>;
            Assert.NotNull(mutableSnapshot);
            mutableSnapshot.Clear();

            Assert.That(state.EntryCount, Is.EqualTo(1));
            Assert.That(state.GetCount("potion_basic"), Is.EqualTo(2));
        }

        [Test]
        public void Entries_AreSortedByItemId()
        {
            var state = new CastleInventoryState();
            state.AddItem("weapon_sword", 1);
            state.AddItem("potion_basic", 1);

            var entries = state.Entries;

            Assert.That(entries[0].ItemId, Is.EqualTo("potion_basic"));
            Assert.That(entries[1].ItemId, Is.EqualTo("weapon_sword"));
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

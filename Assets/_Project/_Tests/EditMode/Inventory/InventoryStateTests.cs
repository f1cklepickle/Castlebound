using NUnit.Framework;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Inventory
{
    public class InventoryStateTests
    {
        private class EventRecorder
        {
            public int Count { get; private set; }
            public InventoryChangeFlags LastFlags { get; private set; } = InventoryChangeFlags.None;

            public void Record(InventoryChangeFlags flags)
            {
                Count++;
                LastFlags = flags;
            }
        }

        [Test]
        public void AddWeapon_EmptySlot_FillsFirstSlot_AndEmitsWeaponChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddWeapon("weapon_a");

            Assert.AreEqual("weapon_a", state.GetWeaponId(0));
            Assert.AreEqual(InventoryChangeFlags.Weapons, recorder.LastFlags);
            Assert.AreEqual(1, recorder.Count);
        }

        [Test]
        public void AddWeapon_SecondSlotEmpty_FillsSecondSlot_AndEmitsWeaponChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddWeapon("weapon_a");
            state.AddWeapon("weapon_b");

            Assert.AreEqual("weapon_a", state.GetWeaponId(0));
            Assert.AreEqual("weapon_b", state.GetWeaponId(1));
            Assert.AreEqual(InventoryChangeFlags.Weapons, recorder.LastFlags);
            Assert.AreEqual(2, recorder.Count);
        }

        [Test]
        public void AddWeapon_BothSlotsFilled_SwapsWithActive_AndEmitsWeaponChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddWeapon("weapon_a");
            state.AddWeapon("weapon_b");
            state.SetActiveWeaponSlot(0);

            state.AddWeapon("weapon_c");

            Assert.AreEqual("weapon_c", state.GetWeaponId(0));
            Assert.AreEqual("weapon_b", state.GetWeaponId(1));
            Assert.AreEqual(InventoryChangeFlags.Weapons, recorder.LastFlags);
        }

        [Test]
        public void SetActiveWeapon_ValidIndex_SetsActive_AndEmitsWeaponChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddWeapon("weapon_a");
            state.AddWeapon("weapon_b");

            state.SetActiveWeaponSlot(1);

            Assert.AreEqual(1, state.ActiveWeaponSlotIndex);
            Assert.AreEqual(InventoryChangeFlags.Weapons, recorder.LastFlags);
        }

        [Test]
        public void RemoveWeapon_SlotCleared_AndEmitsWeaponChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddWeapon("weapon_a");

            state.RemoveWeaponAtSlot(0);

            Assert.IsNull(state.GetWeaponId(0));
            Assert.AreEqual(InventoryChangeFlags.Weapons, recorder.LastFlags);
        }

        [Test]
        public void AddPotion_EmptyStack_AddsTypeAndCount_AndEmitsPotionChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.TryAddPotion("potion_a", 2);

            Assert.AreEqual("potion_a", state.PotionId);
            Assert.AreEqual(2, state.PotionCount);
            Assert.AreEqual(InventoryChangeFlags.Potions, recorder.LastFlags);
        }

        [Test]
        public void AddPotion_SameType_IncrementsCount_AndEmitsPotionChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.TryAddPotion("potion_a", 1);
            state.TryAddPotion("potion_a", 2);

            Assert.AreEqual("potion_a", state.PotionId);
            Assert.AreEqual(3, state.PotionCount);
            Assert.AreEqual(InventoryChangeFlags.Potions, recorder.LastFlags);
        }

        [Test]
        public void AddPotion_DifferentType_IsRejected_AndDoesNotChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.TryAddPotion("potion_a", 1);
            var countBefore = recorder.Count;

            var result = state.TryAddPotion("potion_b", 1);

            Assert.IsFalse(result);
            Assert.AreEqual("potion_a", state.PotionId);
            Assert.AreEqual(1, state.PotionCount);
            Assert.AreEqual(countBefore, recorder.Count);
        }

        [Test]
        public void ConsumePotion_WithCount_Decrements_AndEmitsPotionChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.TryAddPotion("potion_a", 2);

            state.TryConsumePotion(1);

            Assert.AreEqual(1, state.PotionCount);
            Assert.AreEqual(InventoryChangeFlags.Potions, recorder.LastFlags);
        }

        [Test]
        public void ConsumePotion_Empty_NoChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            var result = state.TryConsumePotion(1);

            Assert.IsFalse(result);
            Assert.IsNull(state.PotionId);
            Assert.AreEqual(0, state.PotionCount);
            Assert.AreEqual(0, recorder.Count);
        }

        [Test]
        public void AddGold_Increments_AndEmitsCurrencyChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddGold(5);

            Assert.AreEqual(5, state.Gold);
            Assert.AreEqual(InventoryChangeFlags.Currency, recorder.LastFlags);
        }

        [Test]
        public void AddXp_Increments_AndEmitsCurrencyChange()
        {
            var state = new InventoryState();
            var recorder = new EventRecorder();
            state.OnInventoryChanged += recorder.Record;

            state.AddXp(3);

            Assert.AreEqual(3, state.Xp);
            Assert.AreEqual(InventoryChangeFlags.Currency, recorder.LastFlags);
        }
    }
}

using System;
using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using Random = System.Random;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Inventory
{
    public class LootTableRngTests
    {
        [Test]
        public void Roll_SeededRng_IsDeterministic()
        {
            var table = ScriptableObject.CreateInstance<LootTable>();
            var itemA = ScriptableObject.CreateInstance<ItemDefinition>();
            var itemB = ScriptableObject.CreateInstance<ItemDefinition>();
            itemA.ItemId = "item_a";
            itemB.ItemId = "item_b";

            table.Entries = new LootEntry[]
            {
                new LootEntry(itemA, 1, 2, 1f),
                new LootEntry(itemB, 1, 1, 3f)
            };

            var rngA = new Random(12345);
            var rngB = new Random(12345);

            var rollA = table.Roll(rngA, 1);
            var rollB = table.Roll(rngB, 1);

            Assert.AreEqual(rollA.Item.ItemId, rollB.Item.ItemId);
            Assert.AreEqual(rollA.Amount, rollB.Amount);

            Object.DestroyImmediate(table);
            Object.DestroyImmediate(itemA);
            Object.DestroyImmediate(itemB);
        }

        [Test]
        public void RollMany_CanReturnMultipleEntries_WithCap()
        {
            var table = ScriptableObject.CreateInstance<LootTable>();
            var itemA = ScriptableObject.CreateInstance<ItemDefinition>();
            var itemB = ScriptableObject.CreateInstance<ItemDefinition>();
            var itemC = ScriptableObject.CreateInstance<ItemDefinition>();
            itemA.ItemId = "item_a";
            itemB.ItemId = "item_b";
            itemC.ItemId = "item_c";

            table.Entries = new LootEntry[]
            {
                new LootEntry(itemA, 1, 1, 1f),
                new LootEntry(itemB, 1, 1, 1f),
                new LootEntry(itemC, 1, 1, 1f)
            };

            var rng = new Random(42);

            var results = table.RollMany(rng, 1, 2);

            Assert.LessOrEqual(results.Length, 2);

            Object.DestroyImmediate(table);
            Object.DestroyImmediate(itemA);
            Object.DestroyImmediate(itemB);
            Object.DestroyImmediate(itemC);
        }
    }
}

using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Castlebound.Tests.Loot
{
    public class LootSpawnPolicyTests
    {
        [Test]
        public void BuildRequests_WithWeaponEntry_CreatesWeaponPickup()
        {
            var context = CreateContext<WeaponDefinition>("weapon_test", amount: 3);

            try
            {
                var requests = LootSpawnPolicy.BuildRequests(context.Results, context.Mappings);

                Assert.That(requests, Has.Length.EqualTo(1));
                Assert.That(requests[0].Kind, Is.EqualTo(ItemPickupKind.Weapon));
                Assert.That(requests[0].Item, Is.SameAs(context.Item));
                Assert.That(requests[0].Amount, Is.EqualTo(1));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void BuildRequests_WithPotionEntry_CreatesPotionPickup()
        {
            var context = CreateContext<PotionDefinition>("potion_test", amount: 3);

            try
            {
                var requests = LootSpawnPolicy.BuildRequests(context.Results, context.Mappings);

                Assert.That(requests, Has.Length.EqualTo(1));
                Assert.That(requests[0].Kind, Is.EqualTo(ItemPickupKind.Potion));
                Assert.That(requests[0].Item, Is.SameAs(context.Item));
                Assert.That(requests[0].Amount, Is.EqualTo(3));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void BuildRequests_WithGoldEntry_CreatesGoldPickup()
        {
            var context = CreateContext<GoldDefinition>("gold_test", amount: 7, maxSpawns: 0);

            try
            {
                var requests = LootSpawnPolicy.BuildRequests(context.Results, context.Mappings);

                Assert.That(requests, Has.Length.EqualTo(7));
                Assert.That(requests[0].Kind, Is.EqualTo(ItemPickupKind.Gold));
                Assert.That(requests[0].Item, Is.SameAs(context.Item));
                Assert.That(requests[0].Amount, Is.EqualTo(1));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void BuildRequests_WithLegacyGoldId_CreatesGoldPickup()
        {
            var context = CreateContext<ItemDefinition>("gold_legacy", amount: 5, maxSpawns: 2);

            try
            {
                var requests = LootSpawnPolicy.BuildRequests(context.Results, context.Mappings);

                Assert.That(requests, Has.Length.EqualTo(2));
                Assert.That(requests[0].Kind, Is.EqualTo(ItemPickupKind.Gold));
                Assert.That(requests[0].Amount, Is.EqualTo(3));
                Assert.That(requests[1].Amount, Is.EqualTo(2));
            }
            finally
            {
                context.Destroy();
            }
        }

        [Test]
        public void BuildRequests_WithUnsupportedGenericItem_DoesNotCreatePickup()
        {
            var context = CreateContext<ItemDefinition>("ore_test", amount: 4);

            try
            {
                var requests = LootSpawnPolicy.BuildRequests(context.Results, context.Mappings);

                Assert.That(requests, Is.Empty);
            }
            finally
            {
                context.Destroy();
            }
        }

        private static TestContext<T> CreateContext<T>(string itemId, int amount, int maxSpawns = 0)
            where T : ItemDefinition
        {
            var item = ScriptableObject.CreateInstance<T>();
            item.ItemId = itemId;

            var table = ScriptableObject.CreateInstance<LootTable>();
            table.Entries = new[]
            {
                new LootEntry(item, amount, amount, 1f)
            };

            var pickupGo = new GameObject("PickupPrefab");
            var pickup = pickupGo.AddComponent<ItemPickupComponent>();

            var mappings = new[]
            {
                new LootDropper.LootTableMapping(table, pickup, 1, maxSpawns)
            };
            var results = new[]
            {
                new LootRollResult(item, amount)
            };

            return new TestContext<T>(item, table, pickupGo, mappings, results);
        }

        private readonly struct TestContext<T>
            where T : ItemDefinition
        {
            private readonly LootTable table;
            private readonly GameObject pickupGo;

            public TestContext(
                T item,
                LootTable table,
                GameObject pickupGo,
                LootDropper.LootTableMapping[] mappings,
                LootRollResult[] results)
            {
                Item = item;
                this.table = table;
                this.pickupGo = pickupGo;
                Mappings = mappings;
                Results = results;
            }

            public T Item { get; }
            public LootDropper.LootTableMapping[] Mappings { get; }
            public LootRollResult[] Results { get; }

            public void Destroy()
            {
                Object.DestroyImmediate(pickupGo);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(Item);
            }
        }
    }
}

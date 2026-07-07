using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Inventory
{
    public class BackpackItemDropControllerTests
    {
        private GameObject root;
        private BackpackInventoryStateComponent backpack;
        private BackpackItemDropController dropController;
        private TestWeaponResolver resolver;
        private WeaponDefinition weapon;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("DropRoot");
            backpack = root.AddComponent<BackpackInventoryStateComponent>();
            resolver = root.AddComponent<TestWeaponResolver>();
            dropController = root.AddComponent<BackpackItemDropController>();
            dropController.SetBackpackSource(backpack);
            dropController.SetDropOrigin(root.transform);
            dropController.SetWeaponDefinitionResolver(resolver);

            weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_dagger";
            resolver.Weapon = weapon;
        }

        [TearDown]
        public void TearDown()
        {
            if (dropController != null && dropController.LastSpawnedPickup != null)
            {
                Object.DestroyImmediate(dropController.LastSpawnedPickup.gameObject);
            }

            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void TryDrop_WeaponInBackpack_SpawnsPickupAndRemovesOneItem()
        {
            backpack.State.AddItem("weapon_dagger", 1);

            var result = dropController.TryDrop("weapon_dagger");

            Assert.IsTrue(result);
            Assert.That(backpack.State.GetCount("weapon_dagger"), Is.EqualTo(0));
            Assert.NotNull(dropController.LastSpawnedPickup);
            Assert.That(dropController.LastSpawnedPickup.Kind, Is.EqualTo(ItemPickupKind.Weapon));
            Assert.That(dropController.LastSpawnedPickup.ItemDefinition, Is.SameAs(weapon));
            var motion = dropController.LastSpawnedPickup.GetComponent<LootSpillMotion>();
            Assert.NotNull(motion);
            var start = dropController.LastSpawnedPickup.transform.position;
            motion.Step(1f);
            Assert.That(dropController.LastSpawnedPickup.transform.position.y, Is.LessThan(start.y));
            Assert.That(Vector3.Distance(start, dropController.LastSpawnedPickup.transform.position), Is.GreaterThan(2f));
            Assert.IsFalse(dropController.LastSpawnedPickup.CanAutoPickup(new InventoryState()));
        }

        [Test]
        public void TryDrop_UnknownItem_DoesNotRemoveFromBackpack()
        {
            backpack.State.AddItem("not_weapon", 1);

            var result = dropController.TryDrop("not_weapon");

            Assert.IsFalse(result);
            Assert.That(backpack.State.GetCount("not_weapon"), Is.EqualTo(1));
            Assert.IsNull(dropController.LastSpawnedPickup);
        }

        private sealed class TestWeaponResolver : MonoBehaviour, IWeaponDefinitionResolver
        {
            public WeaponDefinition Weapon { get; set; }

            public WeaponDefinition Resolve(string weaponId)
            {
                return Weapon != null && Weapon.ItemId == weaponId ? Weapon : null;
            }
        }
    }
}

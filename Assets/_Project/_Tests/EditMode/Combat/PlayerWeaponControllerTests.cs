using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Combat;

namespace Castlebound.Tests.Combat
{
    public class PlayerWeaponControllerTests
    {
        [Test]
        public void EquipWeapon_SetsActiveDefinition_FromInventory()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_basic";
            weapon.Damage = 5;
            weapon.AttackSpeed = 1.2f;
            weapon.HitboxSize = new Vector2(1.2f, 0.6f);
            weapon.Knockback = 3f;

            var controller = playerGo.AddComponent<PlayerWeaponController>();
            controller.SetWeaponDefinitionResolver(new TestWeaponResolver(weapon));

            inventoryComponent.State.AddWeapon("weapon_basic");

            controller.RefreshEquippedWeapon(inventoryComponent.State);

            Assert.AreEqual("weapon_basic", controller.CurrentWeaponId);

            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void Attack_UsesWeaponStats()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_basic";
            weapon.Damage = 7;
            weapon.AttackSpeed = 2.5f;
            weapon.HitboxSize = new Vector2(2f, 1f);
            weapon.Knockback = 4f;

            var controller = playerGo.AddComponent<PlayerWeaponController>();
            controller.SetWeaponDefinitionResolver(new TestWeaponResolver(weapon));

            inventoryComponent.State.AddWeapon("weapon_basic");
            controller.RefreshEquippedWeapon(inventoryComponent.State);

            var stats = controller.CurrentWeaponStats;

            Assert.AreEqual(7, stats.Damage);
            Assert.AreEqual(2.5f, stats.AttackSpeed);
            Assert.AreEqual(2f, stats.HitboxSize.x);
            Assert.AreEqual(1f, stats.HitboxSize.y);
            Assert.AreEqual(4f, stats.Knockback);

            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void RefreshEquippedWeapon_EmptySlot_UsesUnarmedStats()
        {
            var playerGo = new GameObject("Player");
            var inventoryComponent = playerGo.AddComponent<InventoryStateComponent>();

            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.ItemId = "weapon_basic";
            weapon.Damage = 7;
            weapon.AttackSpeed = 2.5f;
            weapon.HitboxSize = new Vector2(2f, 1f);
            weapon.Knockback = 4f;

            var controller = playerGo.AddComponent<PlayerWeaponController>();
            controller.SetWeaponDefinitionResolver(new TestWeaponResolver(weapon));

            inventoryComponent.State.AddWeapon("weapon_basic");
            inventoryComponent.State.SetActiveWeaponSlot(1);

            controller.RefreshEquippedWeapon(inventoryComponent.State);

            var stats = controller.CurrentWeaponStats;

            Assert.IsNull(controller.CurrentWeaponId);
            Assert.AreEqual(0, stats.Damage);
            Assert.AreEqual(0f, stats.AttackSpeed);
            Assert.AreEqual(0f, stats.HitboxSize.x);
            Assert.AreEqual(0f, stats.HitboxSize.y);
            Assert.AreEqual(0f, stats.Knockback);

            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(weapon);
        }

        private sealed class TestWeaponResolver : IWeaponDefinitionResolver
        {
            private readonly WeaponDefinition definition;

            public TestWeaponResolver(WeaponDefinition definition)
            {
                this.definition = definition;
            }

            public WeaponDefinition Resolve(string weaponId)
            {
                return definition != null && definition.ItemId == weaponId ? definition : null;
            }
        }
    }
}

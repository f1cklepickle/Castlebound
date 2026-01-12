using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Combat
{
    public class HitboxSizeTests
    {
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

        [Test]
        public void Activate_UpdatesBoxColliderSize_FromWeaponDefinition()
        {
            var player = new GameObject("Player");
            var inventoryComponent = player.AddComponent<InventoryStateComponent>();
            var controller = player.AddComponent<PlayerWeaponController>();

            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.ItemId = "weapon_a";
            definition.HitboxSize = new Vector2(1.5f, 0.75f);

            controller.SetWeaponDefinitionResolver(new TestWeaponResolver(definition));
            inventoryComponent.State.AddWeapon("weapon_a");
            controller.RefreshEquippedWeapon(inventoryComponent.State);

            var hitboxGo = new GameObject("Hitbox");
            hitboxGo.transform.SetParent(player.transform, false);
            var box = hitboxGo.AddComponent<BoxCollider2D>();
            var hitbox = hitboxGo.AddComponent<Hitbox>();

            hitbox.Activate();

            Assert.AreEqual(definition.HitboxSize, box.size);

            Object.DestroyImmediate(hitboxGo);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void Activate_UpdatesBoxColliderOffset_FromWeaponDefinition()
        {
            var player = new GameObject("Player");
            var inventoryComponent = player.AddComponent<InventoryStateComponent>();
            var controller = player.AddComponent<PlayerWeaponController>();

            var definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.ItemId = "weapon_a";
            definition.HitboxSize = new Vector2(1.2f, 0.6f);
            definition.HitboxOffset = new Vector2(0.4f, 0.1f);

            controller.SetWeaponDefinitionResolver(new TestWeaponResolver(definition));
            inventoryComponent.State.AddWeapon("weapon_a");
            controller.RefreshEquippedWeapon(inventoryComponent.State);

            var hitboxGo = new GameObject("Hitbox");
            hitboxGo.transform.SetParent(player.transform, false);
            var box = hitboxGo.AddComponent<BoxCollider2D>();
            var hitbox = hitboxGo.AddComponent<Hitbox>();

            hitbox.Activate();

            Assert.AreEqual(definition.HitboxOffset, box.offset);

            Object.DestroyImmediate(hitboxGo);
            Object.DestroyImmediate(player);
            Object.DestroyImmediate(definition);
        }
    }
}

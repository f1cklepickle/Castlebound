using NUnit.Framework;
using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Tests.Inventory
{
    public class ItemDefinitionTests
    {
        [Test]
        public void WeaponDefinition_ValidFields_AreAccepted()
        {
            var asset = ScriptableObject.CreateInstance<WeaponDefinition>();

            asset.ItemId = "weapon_basic";
            asset.DisplayName = "Basic Sword";
            asset.Damage = 5;
            asset.AttackSpeed = 1.2f;
            asset.HitboxSize = new Vector2(1.2f, 0.6f);
            asset.Knockback = 3f;

            Assert.IsTrue(asset.IsValidId);
            Assert.Greater(asset.Damage, 0);
            Assert.Greater(asset.AttackSpeed, 0f);
            Assert.Greater(asset.HitboxSize.x, 0f);
            Assert.Greater(asset.HitboxSize.y, 0f);
        }

        [Test]
        public void PotionDefinition_ValidFields_AreAccepted()
        {
            var asset = ScriptableObject.CreateInstance<PotionDefinition>();

            asset.ItemId = "potion_basic";
            asset.DisplayName = "Small Heal";
            asset.HealAmount = 10;
            asset.CooldownSeconds = 3f;

            Assert.IsTrue(asset.IsValidId);
            Assert.Greater(asset.HealAmount, 0);
            Assert.Greater(asset.CooldownSeconds, 0f);
        }

        [Test]
        public void ItemDefinition_EmptyId_IsInvalid()
        {
            var asset = ScriptableObject.CreateInstance<ItemDefinition>();

            asset.ItemId = "";

            Assert.IsFalse(asset.IsValidId);
        }
    }
}

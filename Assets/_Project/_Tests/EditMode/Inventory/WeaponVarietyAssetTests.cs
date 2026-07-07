using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Loot;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Inventory
{
    public class WeaponVarietyAssetTests
    {
        [Test]
        public void PrototypeWeaponDefinitions_UseRequestedStatProfile()
        {
            var sword = LoadWeapon("Weapon_Sword");
            var ironClub = LoadWeapon("Weapon_IronClub");
            var club = LoadWeapon("Weapon_Club");
            var rustyDagger = LoadWeapon("Weapon_RustyDagger");

            Assert.That(ironClub.Damage, Is.EqualTo(5));
            Assert.That(ironClub.AttackSpeed, Is.GreaterThan(0.8f));
            Assert.That(ironClub.AttackSpeed, Is.LessThan(sword.AttackSpeed));
            Assert.That(ironClub.HitboxSize.x, Is.GreaterThan(sword.HitboxSize.x));
            Assert.That(ironClub.Icon, Is.Not.Null);
            Assert.That(ironClub.HandSprite, Is.SameAs(ironClub.Icon));

            Assert.That(club.Damage, Is.EqualTo(3));
            Assert.That(club.AttackSpeed, Is.GreaterThan(sword.AttackSpeed));
            Assert.That(club.HitboxSize.x, Is.LessThan(sword.HitboxSize.x));
            Assert.That(club.Icon, Is.Not.Null);
            Assert.That(club.HandSprite, Is.SameAs(club.Icon));

            Assert.That(rustyDagger.Damage, Is.EqualTo(3));
            Assert.That(rustyDagger.AttackSpeed, Is.GreaterThan(club.AttackSpeed));
            Assert.That(rustyDagger.HitboxSize.x, Is.LessThan(club.HitboxSize.x));
            Assert.That(rustyDagger.Icon, Is.Not.Null);
            Assert.That(rustyDagger.HandSprite, Is.SameAs(rustyDagger.Icon));
        }

        [Test]
        public void WeaponPickupPrefabs_RenderTheirAssignedDefinition()
        {
            AssertPickupMatchesDefinition("IronClub");
            AssertPickupMatchesDefinition("Club");
            AssertPickupMatchesDefinition("RustyDagger");
        }

        [Test]
        public void WeaponLootTable_IncludesPrototypeWeapons()
        {
            var table = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/_Project/Items/LootTables/LootTable_WeaponBasic.asset");

            Assert.That(table, Is.Not.Null);
            Assert.That(table.ContainsItem(LoadWeapon("Weapon_IronClub")), Is.True);
            Assert.That(table.ContainsItem(LoadWeapon("Weapon_Club")), Is.True);
            Assert.That(table.ContainsItem(LoadWeapon("Weapon_RustyDagger")), Is.True);
        }

        private static void AssertPickupMatchesDefinition(string weaponName)
        {
            var definition = LoadWeapon("Weapon_" + weaponName);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Pickups/Pickup_Weapon_" + weaponName + ".prefab");

            Assert.That(prefab, Is.Not.Null);
            var pickup = prefab.GetComponent<ItemPickupComponent>();
            var renderer = prefab.GetComponent<SpriteRenderer>();
            var visual = prefab.GetComponent<ItemPickupVisual>();

            Assert.That(pickup, Is.Not.Null);
            Assert.That(renderer, Is.Not.Null);
            Assert.That(visual, Is.Not.Null);
            Assert.That(pickup.Kind, Is.EqualTo(ItemPickupKind.Weapon));
            Assert.That(pickup.ItemDefinition, Is.SameAs(definition));
            AssertSpritesReferenceSameAsset(renderer.sprite, definition.Icon);
        }

        private static void AssertSpritesReferenceSameAsset(Sprite actual, Sprite expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(expected, Is.Not.Null);

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(actual, out string actualGuid, out long actualLocalId);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(expected, out string expectedGuid, out long expectedLocalId);

            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
            Assert.That(actualLocalId, Is.EqualTo(expectedLocalId));
        }

        private static WeaponDefinition LoadWeapon(string assetName)
        {
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/_Project/Items/Definitions/" + assetName + ".asset");
            Assert.That(weapon, Is.Not.Null, assetName);
            return weapon;
        }
    }
}

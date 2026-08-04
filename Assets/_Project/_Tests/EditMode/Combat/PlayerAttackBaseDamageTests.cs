using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class PlayerAttackBaseDamageTests
    {
        [Test]
        public void PlayerController_DefaultBaseAttackDamage_IsOne()
        {
            var player = new GameObject("Player");

            try
            {
                var controller = player.AddComponent<PlayerController>();
                Assert.That(controller.BaseAttackDamage, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [Test]
        public void PlayerPrefab_AuthorsBaseAttackDamageOfOne()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Player.prefab");

            Assert.NotNull(prefab);
            Assert.That(prefab.GetComponent<PlayerController>().BaseAttackDamage, Is.EqualTo(1));
        }

        [Test]
        public void PlayerBaseDamage_AddsToUnarmedAndSwordProfiles()
        {
            var unarmed = AssetDatabase.LoadAssetAtPath<CombatEquipmentProfile>(
                "Assets/_Project/Items/CombatProfiles/CombatEquipment_Unarmed.asset");
            var sword = AssetDatabase.LoadAssetAtPath<CombatEquipmentProfile>(
                "Assets/_Project/Items/CombatProfiles/CombatEquipment_Sword.asset");
            var baseStats = new CombatBaseStats(1, 1.5f, 0f, 0f);
            var capabilities = CombatEquipmentCapability.MeleeDelivery
                | CombatEquipmentCapability.HandSocket;

            Assert.That(CombatEquipmentResolver.TryResolve(
                baseStats,
                capabilities,
                unarmed,
                out var unarmedSnapshot), Is.True);
            Assert.That(CombatEquipmentResolver.TryResolve(
                baseStats,
                capabilities,
                sword,
                out var swordSnapshot), Is.True);
            Assert.That(unarmedSnapshot.Damage, Is.EqualTo(1));
            Assert.That(swordSnapshot.Damage, Is.EqualTo(6));
        }
    }
}

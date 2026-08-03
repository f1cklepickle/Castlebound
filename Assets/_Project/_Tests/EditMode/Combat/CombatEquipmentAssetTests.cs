using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEditor;

namespace Castlebound.Tests.Combat
{
    public class CombatEquipmentAssetTests
    {
        [Test]
        public void Club_PlayerItemAndEnemyLoadoutReferenceSameCombatProfile()
        {
            var playerClub = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                "Assets/_Project/Items/Definitions/Weapon_Club.asset");
            var enemyClub = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Club.asset");

            Assert.That(playerClub, Is.Not.Null);
            Assert.That(enemyClub, Is.Not.Null);
            Assert.That(playerClub.CombatProfile, Is.Not.Null);
            Assert.That(enemyClub.CombatProfile, Is.SameAs(playerClub.CombatProfile));
            Assert.That(playerClub.ItemId, Is.EqualTo("weapon_club"));
            Assert.That(enemyClub.CompatibleRole, Is.EqualTo(EnemyAttackRole.Melee));
        }

        [Test]
        public void AuthoredDagger_ResolvesFasterThanAuthoredClubForEnemyCapabilities()
        {
            var dagger = AssetDatabase.LoadAssetAtPath<CombatEquipmentProfile>(
                "Assets/_Project/Items/CombatProfiles/CombatEquipment_RustyDagger.asset");
            var club = AssetDatabase.LoadAssetAtPath<CombatEquipmentProfile>(
                "Assets/_Project/Items/CombatProfiles/CombatEquipment_Club.asset");
            var enemyStats = new CombatBaseStats(1, 1f, 0.5f, 0f);
            var enemyCapabilities =
                CombatEquipmentCapability.MeleeDelivery |
                CombatEquipmentCapability.HandSocket;

            Assert.That(CombatEquipmentResolver.TryResolve(
                enemyStats, enemyCapabilities, dagger, out var daggerSnapshot), Is.True);
            Assert.That(CombatEquipmentResolver.TryResolve(
                enemyStats, enemyCapabilities, club, out var clubSnapshot), Is.True);
            Assert.That(daggerSnapshot.AttackRate, Is.GreaterThan(clubSnapshot.AttackRate));
        }

        [Test]
        public void Rock_ProfileOwnsProjectilePayloadWhileEnemyAdapterOwnsTargetLayer()
        {
            var rock = AssetDatabase.LoadAssetAtPath<EnemyEquipmentDefinition>(
                "Assets/_Project/Items/Definitions/EnemyEquipment_Rock.asset");

            Assert.That(rock.CombatProfile, Is.Not.Null);
            Assert.That(rock.CombatProfile.ProjectilePrefab, Is.Not.Null);
            Assert.That(rock.CombatProfile.DamageBonus, Is.EqualTo(2));
            Assert.That(rock.CombatProfile.ProjectileSpeed, Is.EqualTo(8f));
            Assert.That(rock.ProjectileTargetLayerMask.value, Is.Not.Zero);
        }
    }
}

using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class CombatEquipmentProfileTests
    {
        [Test]
        public void CanEquip_RequiresEveryCapabilityWithoutEntityTypeChecks()
        {
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            try
            {
                profile.RequiredCapabilities =
                    CombatEquipmentCapability.MeleeDelivery |
                    CombatEquipmentCapability.HandSocket;

                Assert.IsTrue(profile.CanEquip(
                    CombatEquipmentCapability.MeleeDelivery |
                    CombatEquipmentCapability.HandSocket |
                    CombatEquipmentCapability.ProjectileDelivery));
                Assert.IsFalse(profile.CanEquip(CombatEquipmentCapability.MeleeDelivery));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void TryResolve_CombinesWearerStatsWithEquipmentEffects()
        {
            var profile = CreateProfile("club", 3, 1.45f, 0.4f, 0.85f);
            try
            {
                var wearerStats = new CombatBaseStats(2, 1.2f, 0.6f, 0.15f);

                bool resolved = CombatEquipmentResolver.TryResolve(
                    wearerStats,
                    CombatEquipmentCapability.MeleeDelivery | CombatEquipmentCapability.HandSocket,
                    profile,
                    out CombatEquipmentSnapshot snapshot);

                Assert.IsTrue(resolved);
                Assert.That(snapshot.EquipmentId, Is.EqualTo("club"));
                Assert.That(snapshot.Damage, Is.EqualTo(5));
                Assert.That(snapshot.AttackRate, Is.EqualTo(1.74f).Within(0.0001f));
                Assert.That(snapshot.Range, Is.EqualTo(1f).Within(0.0001f));
                Assert.That(snapshot.Knockback, Is.EqualTo(1f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ResolvedSnapshot_DoesNotChangeWhenProfileIsRetuned()
        {
            var profile = CreateProfile("club", 3, 1.45f, 0.4f, 0.85f);
            try
            {
                CombatEquipmentResolver.TryResolve(
                    new CombatBaseStats(1, 1f, 0.5f, 0f),
                    CombatEquipmentCapability.MeleeDelivery | CombatEquipmentCapability.HandSocket,
                    profile,
                    out CombatEquipmentSnapshot snapshot);

                profile.DamageBonus = 20;
                profile.AttackRateMultiplier = 4f;

                Assert.That(snapshot.Damage, Is.EqualTo(4));
                Assert.That(snapshot.AttackRate, Is.EqualTo(1.45f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void Dagger_ResolvesFasterAttackRateThanClubForSameWearer()
        {
            var dagger = CreateProfile("rusty_dagger", 3, 1.8f, 0.2f, 0.55f);
            var club = CreateProfile("club", 3, 1.45f, 0.4f, 0.85f);
            try
            {
                var wearerStats = new CombatBaseStats(1, 1f, 0.5f, 0f);
                var capabilities = CombatEquipmentCapability.MeleeDelivery | CombatEquipmentCapability.HandSocket;

                CombatEquipmentResolver.TryResolve(wearerStats, capabilities, dagger, out var daggerSnapshot);
                CombatEquipmentResolver.TryResolve(wearerStats, capabilities, club, out var clubSnapshot);

                Assert.That(daggerSnapshot.AttackRate, Is.GreaterThan(clubSnapshot.AttackRate));
            }
            finally
            {
                Object.DestroyImmediate(dagger);
                Object.DestroyImmediate(club);
            }
        }

        [Test]
        public void SharedProfile_CanBackPlayerAndEnemyAdaptersWithoutSharingHolderData()
        {
            var profile = CreateProfile("club", 3, 1.45f, 0.4f, 0.85f);
            var playerItem = ScriptableObject.CreateInstance<WeaponDefinition>();
            var enemyLoadoutItem = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            try
            {
                playerItem.CombatProfile = profile;
                playerItem.HitboxSize = new Vector2(2f, 2f);
                enemyLoadoutItem.CombatProfile = profile;
                enemyLoadoutItem.CompatibleRole = EnemyAttackRole.Melee;
                enemyLoadoutItem.HandleScale = new Vector2(0.75f, 0.75f);

                Assert.That(playerItem.CombatProfile, Is.SameAs(profile));
                Assert.That(enemyLoadoutItem.CombatProfile, Is.SameAs(profile));
                Assert.That(playerItem.Damage, Is.EqualTo(3));
                Assert.That(enemyLoadoutItem.ProjectileDamage, Is.EqualTo(3));
                Assert.That(playerItem.HitboxSize, Is.EqualTo(new Vector2(2f, 2f)));
                Assert.That(enemyLoadoutItem.HandleScale, Is.EqualTo(new Vector2(0.75f, 0.75f)));
            }
            finally
            {
                Object.DestroyImmediate(enemyLoadoutItem);
                Object.DestroyImmediate(playerItem);
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void RuntimeEquipmentComponents_ExposeOneHolderNeutralSourceContract()
        {
            Assert.That(typeof(ICombatEquipmentSource).IsAssignableFrom(typeof(PlayerWeaponController)), Is.True);
            Assert.That(typeof(ICombatEquipmentSource).IsAssignableFrom(typeof(EnemyEquipment)), Is.True);
        }

        private static CombatEquipmentProfile CreateProfile(
            string equipmentId,
            int damageBonus,
            float attackRateMultiplier,
            float rangeBonus,
            float knockbackBonus)
        {
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            profile.EquipmentId = equipmentId;
            profile.DamageBonus = damageBonus;
            profile.AttackRateMultiplier = attackRateMultiplier;
            profile.RangeBonus = rangeBonus;
            profile.KnockbackBonus = knockbackBonus;
            profile.RequiredCapabilities =
                CombatEquipmentCapability.MeleeDelivery |
                CombatEquipmentCapability.HandSocket;
            return profile;
        }
    }
}

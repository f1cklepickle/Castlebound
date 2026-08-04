using System.IO;
using System.Reflection;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class EnemyAttackTimingTests
    {
        [Test]
        public void EnemyAttack_UsesSharedClockWithoutCoroutineWaits()
        {
            const string path = "Assets/_Project/Scripts/_Project.Gameplay/Combat/EnemyAttack.cs";
            const string snapshotResolverPath =
                "Assets/_Project/Scripts/_Project.Gameplay/Combat/EnemyAttackEquipmentSnapshotResolver.cs";
            string source = File.ReadAllText(path);
            string snapshotResolverSource = File.ReadAllText(snapshotResolverPath);

            StringAssert.Contains("AttackClock", source);
            StringAssert.Contains("EnemyAttackEquipmentSnapshotResolver", source);
            StringAssert.Contains("CombatEquipmentResolver", snapshotResolverSource);
            StringAssert.DoesNotContain("WaitForSeconds", source);
            StringAssert.DoesNotContain("StartCoroutine", source);
        }

        [Test]
        public void BaseRate_PreservesConfiguredWindupAndCooldownCycle()
        {
            Assert.That(EnemyAttack.CalculateBaseAttackRate(0.3f, 0.8f),
                Is.EqualTo(1f / 1.1f).Within(0.0001f));
        }

        [Test]
        public void CombatSnapshot_CapturesEquipmentRateForCurrentSwing()
        {
            var enemy = new GameObject("Enemy");
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            var definition = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();

            try
            {
                profile.RequiredCapabilities = CombatEquipmentCapability.MeleeDelivery;
                profile.AttackRateMultiplier = 2f;
                definition.CombatProfile = profile;
                definition.CompatibleRole = EnemyAttackRole.Melee;

                var equipment = enemy.AddComponent<EnemyEquipment>();
                equipment.Equip(definition);
                var attack = enemy.AddComponent<EnemyAttack>();
                SetField(attack, "windupSeconds", 0.2f);
                attack.CooldownSeconds = 0.8f;

                CombatEquipmentSnapshot captured = attack.CaptureCombatEquipmentSnapshot();
                profile.AttackRateMultiplier = 4f;

                Assert.That(captured.AttackRate, Is.EqualTo(2f).Within(0.0001f));
                Assert.That(attack.CaptureCombatEquipmentSnapshot().AttackRate,
                    Is.EqualTo(4f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void MeleeDelivery_UsesCapturedCombatDamage()
        {
            var enemy = new GameObject("Enemy");
            var target = new GameObject("Target");

            try
            {
                var delivery = enemy.AddComponent<EnemyMeleeAttackDelivery>();
                delivery.Damage = 1;
                var health = target.AddComponent<Health>();
                health.ConfigureMaxHealth(10, refill: true);
                var snapshot = new CombatEquipmentSnapshot(
                    "club",
                    4,
                    1f,
                    0f,
                    0f,
                    CombatEquipmentCapability.MeleeDelivery,
                    null,
                    null,
                    0f,
                    0f,
                    0f);

                Assert.IsTrue(delivery.TryDeliver(target.transform, null, snapshot));
                Assert.That(health.Current, Is.EqualTo(6));
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(enemy);
            }
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            instance.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(instance, value);
        }
    }
}

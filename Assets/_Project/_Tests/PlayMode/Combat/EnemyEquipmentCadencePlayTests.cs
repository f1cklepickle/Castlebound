using System.Collections;
using System.Reflection;
using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Castlebound.Tests.PlayMode.Combat
{
    public class EnemyEquipmentCadencePlayTests
    {
        [UnityTest]
        public IEnumerator FasterEquipment_CompletesMoreAttacksForSameEnemyBaseRate()
        {
            int fastDamage = 0;
            int slowDamage = 0;

            yield return MeasureDamageOverWindow(2f, value => fastDamage = value);
            yield return MeasureDamageOverWindow(1f, value => slowDamage = value);

            Assert.That(fastDamage, Is.GreaterThan(slowDamage + 1),
                "A faster equipment snapshot should produce a reliably faster enemy cadence.");
        }

        private static IEnumerator MeasureDamageOverWindow(
            float attackRateMultiplier,
            System.Action<int> reportDamage)
        {
            var target = new GameObject("Player");
            var enemy = new GameObject("Enemy");
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            var definition = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();

            try
            {
                target.tag = "Player";
                target.layer = LayerMask.NameToLayer("Player");
                target.transform.position = new Vector2(0.5f, 0f);
                target.AddComponent<BoxCollider2D>();
                var health = target.AddComponent<Health>();
                health.ConfigureMaxHealth(50, refill: true);

                var body = enemy.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                enemy.AddComponent<BoxCollider2D>();
                var controller = enemy.AddComponent<EnemyController2D>();
                controller.Debug_SetupRefs(target.transform);
                SetField(enemy.GetComponent<EnemyFacing>(), "attackAlignmentThreshold", 180f);

                profile.RequiredCapabilities = CombatEquipmentCapability.MeleeDelivery;
                profile.AttackRateMultiplier = attackRateMultiplier;
                definition.CombatProfile = profile;
                definition.CompatibleRole = EnemyAttackRole.Melee;
                var equipment = enemy.AddComponent<EnemyEquipment>();
                equipment.Equip(definition);

                var attack = enemy.AddComponent<EnemyAttack>();
                SetField(attack, "windupSeconds", 0.05f);
                attack.CooldownSeconds = 0.15f;

                yield return new WaitForSeconds(0.72f);
                reportDamage(50 - health.Current);
            }
            finally
            {
                Object.Destroy(enemy);
                Object.Destroy(target);
                Object.Destroy(definition);
                Object.Destroy(profile);
            }

            yield return null;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            instance.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(instance, value);
        }
    }
}

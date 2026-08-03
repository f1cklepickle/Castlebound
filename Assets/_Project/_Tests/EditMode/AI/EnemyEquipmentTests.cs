using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyEquipmentTests
    {
        [Test]
        public void EquipUnarmedDefinition_ExplicitlyClearsWeaponPresentation()
        {
            var enemy = new GameObject("EnemyEquipmentTests");
            var definition = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            try
            {
                var equipment = enemy.AddComponent<EnemyEquipment>();
                profile.EquipmentId = "unarmed";
                definition.CombatProfile = profile;
                CombatEquipmentProfile publishedProfile = null;
                equipment.EquipmentChanged += changedProfile => publishedProfile = changedProfile;

                Assert.IsTrue(equipment.Equip(definition));
                Assert.That(equipment.ActiveEquipment, Is.SameAs(definition));
                Assert.That(equipment.ActiveCombatProfile, Is.SameAs(profile));
                Assert.That(publishedProfile, Is.SameAs(profile));
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void Definition_ReportsCompatibleAttackRoleWithoutEnumBranches()
        {
            var definition = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            var profile = ScriptableObject.CreateInstance<CombatEquipmentProfile>();
            try
            {
                profile.RequiredCapabilities = CombatEquipmentCapability.ProjectileDelivery;
                definition.CombatProfile = profile;
                definition.CompatibleRole = EnemyAttackRole.Ranged;

                Assert.IsTrue(definition.IsCompatibleWith(EnemyAttackRole.Ranged));
                Assert.IsFalse(definition.IsCompatibleWith(EnemyAttackRole.Melee));
            }
            finally
            {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(definition);
            }
        }
    }
}

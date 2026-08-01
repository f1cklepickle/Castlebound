using Castlebound.Gameplay.AI;
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
            try
            {
                var equipment = enemy.AddComponent<EnemyEquipment>();
                definition.EquipmentId = "unarmed";

                Assert.IsTrue(equipment.Equip(definition));
                Assert.That(equipment.ActiveEquipment, Is.SameAs(definition));
            }
            finally
            {
                Object.DestroyImmediate(definition);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void Definition_ReportsCompatibleAttackRoleWithoutEnumBranches()
        {
            var definition = ScriptableObject.CreateInstance<EnemyEquipmentDefinition>();
            try
            {
                definition.CompatibleRole = EnemyAttackRole.Ranged;

                Assert.IsTrue(definition.IsCompatibleWith(EnemyAttackRole.Ranged));
                Assert.IsFalse(definition.IsCompatibleWith(EnemyAttackRole.Melee));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }
    }
}

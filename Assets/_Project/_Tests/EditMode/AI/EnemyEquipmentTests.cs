using Castlebound.Gameplay.AI;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.AI
{
    public class EnemyEquipmentTests
    {
        [Test]
        public void EquipUnarmed_ExplicitlyClearsWeaponPresentation()
        {
            var enemy = new GameObject("EnemyEquipmentTests");
            try
            {
                var equipment = enemy.AddComponent<EnemyEquipment>();
                equipment.Equip(EnemyEquipment.Loadout.Unarmed);
                Assert.That(equipment.EquippedLoadout, Is.EqualTo(EnemyEquipment.Loadout.Unarmed));
            }
            finally
            {
                Object.DestroyImmediate(enemy);
            }
        }
    }
}

using Castlebound.Gameplay.AI;
using Castlebound.Gameplay.Combat;
using UnityEngine;

public interface IEnemyAttackDelivery
{
    EnemyAttackRole AttackRole { get; }
    bool CanDeliver(
        Transform lockedTarget,
        EnemyEquipmentDefinition equipmentDefinitionSnapshot,
        CombatEquipmentSnapshot combatEquipmentSnapshot);
    bool TryDeliver(
        Transform lockedTarget,
        EnemyEquipmentDefinition equipmentDefinitionSnapshot,
        CombatEquipmentSnapshot combatEquipmentSnapshot);
}

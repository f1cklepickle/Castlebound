using Castlebound.Gameplay.AI;
using UnityEngine;

public interface IEnemyAttackDelivery
{
    EnemyAttackRole AttackRole { get; }
    bool CanDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot);
    bool TryDeliver(Transform lockedTarget, EnemyEquipmentDefinition equipmentSnapshot);
}

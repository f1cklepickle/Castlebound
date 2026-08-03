using System;

namespace Castlebound.Gameplay.Combat
{
    public interface ICombatEquipmentSource
    {
        CombatEquipmentProfile ActiveCombatProfile { get; }
        event Action<CombatEquipmentProfile> EquipmentChanged;
    }
}

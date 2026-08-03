using System;

namespace Castlebound.Gameplay.Combat
{
    [Flags]
    public enum CombatEquipmentCapability
    {
        None = 0,
        MeleeDelivery = 1 << 0,
        ProjectileDelivery = 1 << 1,
        HandSocket = 1 << 2
    }
}

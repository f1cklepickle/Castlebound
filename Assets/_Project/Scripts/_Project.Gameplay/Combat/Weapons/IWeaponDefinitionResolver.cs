using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Combat
{
    public interface IWeaponDefinitionResolver
    {
        WeaponDefinition Resolve(string weaponId);
    }
}

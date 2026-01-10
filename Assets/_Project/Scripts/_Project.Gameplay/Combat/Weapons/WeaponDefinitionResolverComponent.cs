using Castlebound.Gameplay.Inventory;
using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public class WeaponDefinitionResolverComponent : MonoBehaviour, IWeaponDefinitionResolver
    {
        [SerializeField] private WeaponDefinition[] definitions;

        public WeaponDefinition Resolve(string weaponId)
        {
            if (definitions == null || string.IsNullOrWhiteSpace(weaponId))
            {
                return null;
            }

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (definition != null && definition.ItemId == weaponId)
                {
                    return definition;
                }
            }

            return null;
        }
    }
}

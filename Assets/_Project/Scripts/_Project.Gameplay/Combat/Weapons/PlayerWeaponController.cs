using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Combat
{
    public class PlayerWeaponController : MonoBehaviour
    {
        private IWeaponDefinitionResolver resolver;
        private WeaponDefinition equippedDefinition;

        public string CurrentWeaponId => equippedDefinition != null ? equippedDefinition.ItemId : null;

        public WeaponStats CurrentWeaponStats
        {
            get
            {
                if (equippedDefinition == null)
                {
                    return new WeaponStats(0, 0f, Vector2.zero, 0f);
                }

                return new WeaponStats(
                    equippedDefinition.Damage,
                    equippedDefinition.AttackSpeed,
                    equippedDefinition.HitboxSize,
                    equippedDefinition.Knockback);
            }
        }

        public void SetWeaponDefinitionResolver(IWeaponDefinitionResolver definitionResolver)
        {
            resolver = definitionResolver;
        }

        public void RefreshEquippedWeapon(InventoryState inventory)
        {
            if (inventory == null || resolver == null)
            {
                equippedDefinition = null;
                return;
            }

            string weaponId = inventory.GetWeaponId(inventory.ActiveWeaponSlotIndex);
            equippedDefinition = resolver.Resolve(weaponId);
        }
    }
}

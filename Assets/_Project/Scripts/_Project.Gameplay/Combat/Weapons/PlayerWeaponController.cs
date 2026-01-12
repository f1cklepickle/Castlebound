using UnityEngine;
using Castlebound.Gameplay.Inventory;

namespace Castlebound.Gameplay.Combat
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] private InventoryStateComponent inventorySource;
        [SerializeField] private MonoBehaviour resolverSource;

        private IWeaponDefinitionResolver resolver;
        private WeaponDefinition equippedDefinition;
        private InventoryState inventory;

        public string CurrentWeaponId => equippedDefinition != null ? equippedDefinition.ItemId : null;

        public WeaponStats CurrentWeaponStats
        {
            get
            {
                if (equippedDefinition == null)
                {
                    return new WeaponStats(0, 0f, Vector2.zero, Vector2.zero, 0f);
                }

                return new WeaponStats(
                    equippedDefinition.Damage,
                    equippedDefinition.AttackSpeed,
                    equippedDefinition.HitboxSize,
                    equippedDefinition.HitboxOffset,
                    equippedDefinition.Knockback);
            }
        }

        public void SetWeaponDefinitionResolver(IWeaponDefinitionResolver definitionResolver)
        {
            resolver = definitionResolver;
            resolverSource = definitionResolver as MonoBehaviour;
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

        private void OnEnable()
        {
            EnsureReferences();
            if (inventory != null)
            {
                inventory.OnInventoryChanged += OnInventoryChanged;
            }

            RefreshEquippedWeapon(inventory);
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= OnInventoryChanged;
            }
        }

        private void OnInventoryChanged(InventoryChangeFlags flags)
        {
            if ((flags & InventoryChangeFlags.Weapons) == 0)
            {
                return;
            }

            RefreshEquippedWeapon(inventory);
        }

        private void EnsureReferences()
        {
            if (inventorySource == null)
            {
                inventorySource = GetComponentInParent<InventoryStateComponent>();
            }

            if (resolver == null)
            {
                if (resolverSource == null)
                {
                    resolverSource = FindObjectOfType<WeaponDefinitionResolverComponent>();
                }

                resolver = resolverSource as IWeaponDefinitionResolver;
            }

            inventory = inventorySource != null ? inventorySource.State : null;
        }
    }
}

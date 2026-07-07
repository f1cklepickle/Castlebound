using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class ItemPickupVisual : MonoBehaviour
    {
        [SerializeField] private ItemPickupComponent pickup;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private ItemDefinition lastDefinition;

        private void OnEnable()
        {
            Refresh();
        }

        private void LateUpdate()
        {
            if (pickup == null)
            {
                EnsureReferences();
            }

            ItemDefinition currentDefinition = pickup != null ? pickup.ItemDefinition : null;
            if (currentDefinition != lastDefinition)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            EnsureReferences();

            ItemDefinition definition = pickup != null ? pickup.ItemDefinition : null;
            Sprite sprite = definition != null ? definition.Icon : null;
            lastDefinition = definition;

            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = sprite != null;
        }

        private void EnsureReferences()
        {
            if (pickup == null)
            {
                pickup = GetComponent<ItemPickupComponent>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void OnValidate()
        {
            EnsureReferences();
        }
    }
}

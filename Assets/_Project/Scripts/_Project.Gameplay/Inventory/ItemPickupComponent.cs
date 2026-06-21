using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class ItemPickupComponent : MonoBehaviour
    {
        [SerializeField] private ItemPickupKind kind;
        [SerializeField] private ItemDefinition itemDefinition;
        [SerializeField] private int amount = 1;
        [SerializeField] private float pickupDelaySeconds = 0f;

        private float pickupDelayRemaining = 0f;
        private ItemPickup cachedPickup;
        private bool isPickupCacheDirty = true;

        public ItemPickupKind Kind
        {
            get => kind;
            set
            {
                kind = value;
                InvalidatePickupCache();
            }
        }

        public ItemDefinition ItemDefinition
        {
            get => itemDefinition;
            set
            {
                itemDefinition = value;
                InvalidatePickupCache();
            }
        }

        public int Amount
        {
            get => amount;
            set
            {
                amount = value;
                InvalidatePickupCache();
            }
        }

        public bool IsConsumed { get; private set; }

        public void SetPickupDelay(float seconds)
        {
            pickupDelayRemaining = Mathf.Max(0f, seconds);
        }

        private void Awake()
        {
            pickupDelayRemaining = Mathf.Max(0f, pickupDelaySeconds);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsConsumed)
            {
                return;
            }

            if (pickupDelayRemaining > 0f)
            {
                return;
            }

            if (!TryGetInventory(other, out InventoryState inventory))
            {
                return;
            }

            TryAutoPickup(inventory);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (IsConsumed)
            {
                return;
            }

            if (pickupDelayRemaining > 0f)
            {
                return;
            }

            if (!TryGetInventory(other, out InventoryState inventory))
            {
                return;
            }

            TryAutoPickup(inventory);
        }

        public bool TryAutoPickup(InventoryState inventory)
        {
            if (!CanAutoPickup(inventory))
            {
                return false;
            }

            ItemPickup pickup = GetPickup();
            bool result = pickup.TryAutoPickup(inventory);
            if (result)
            {
                Consume();
            }

            return result;
        }

        public bool CanAutoPickup(InventoryState inventory)
        {
            if (IsConsumed || pickupDelayRemaining > 0f)
            {
                return false;
            }

            ItemPickup pickup = GetPickup();
            return pickup != null && pickup.CanAutoPickup(inventory);
        }

        public bool TryManualPickup(InventoryState inventory)
        {
            if (IsConsumed)
            {
                return false;
            }

            if (pickupDelayRemaining > 0f)
            {
                return false;
            }

            ItemPickup pickup = GetPickup();
            if (pickup == null)
            {
                return false;
            }

            bool result = pickup.TryManualPickup(inventory);
            if (result)
            {
                Consume();
            }

            return result;
        }

        private ItemPickup GetPickup()
        {
            if (!isPickupCacheDirty)
            {
                return cachedPickup;
            }

            cachedPickup = BuildPickup();
            isPickupCacheDirty = false;
            return cachedPickup;
        }

        private ItemPickup BuildPickup()
        {
            switch (kind)
            {
                case ItemPickupKind.Weapon:
                    if (itemDefinition is WeaponDefinition weapon)
                    {
                        return ItemPickup.Weapon(weapon.ItemId);
                    }

                    return null;
                case ItemPickupKind.Potion:
                    if (itemDefinition is PotionDefinition potion)
                    {
                        return ItemPickup.Potion(potion.ItemId, amount);
                    }

                    return null;
                case ItemPickupKind.Gold:
                    return ItemPickup.Gold(amount);
                case ItemPickupKind.Xp:
                    return ItemPickup.Xp(amount);
                default:
                    return null;
            }
        }

        private void InvalidatePickupCache()
        {
            cachedPickup = null;
            isPickupCacheDirty = true;
        }

        private void OnValidate()
        {
            InvalidatePickupCache();
        }

        private static bool TryGetInventory(Component other, out InventoryState inventory)
        {
            if (other.GetComponent<Castlebound.Gameplay.Player.PlayerPickupCollider>() == null)
            {
                inventory = null;
                return false;
            }

            InventoryStateComponent component = other.GetComponentInParent<InventoryStateComponent>();
            if (component != null)
            {
                inventory = component.State;
                return true;
            }

            inventory = null;
            return false;
        }

        private void Update()
        {
            if (pickupDelayRemaining <= 0f)
            {
                return;
            }

            pickupDelayRemaining = Mathf.Max(0f, pickupDelayRemaining - Time.deltaTime);
        }

        private void Consume()
        {
            IsConsumed = true;
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}

using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Loot;
using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class BackpackItemDropController : MonoBehaviour
    {
        [SerializeField] private BackpackInventoryStateComponent backpackSource;
        [SerializeField] private Transform dropOrigin;
        [SerializeField] private MonoBehaviour weaponDefinitionResolverSource;
        [SerializeField] private float dropStartDistance = 0.45f;
        [SerializeField] private float dropSlideDistance = 3f;
        [SerializeField] private float dropSlideDuration = 0.35f;
        [SerializeField] private float dropScatterDegrees = 14f;
        [SerializeField] private float pickupDelaySeconds = 5f;

        private IWeaponDefinitionResolver weaponDefinitionResolver;

        public ItemPickupComponent LastSpawnedPickup { get; private set; }

        public void SetBackpackSource(BackpackInventoryStateComponent source)
        {
            backpackSource = source;
        }

        public void SetDropOrigin(Transform origin)
        {
            dropOrigin = origin;
        }

        public void SetWeaponDefinitionResolver(IWeaponDefinitionResolver resolver)
        {
            weaponDefinitionResolver = resolver;
            weaponDefinitionResolverSource = resolver as MonoBehaviour;
        }

        public bool TryDrop(string itemId)
        {
            BackpackInventoryState backpack = backpackSource != null ? backpackSource.State : null;
            if (backpack == null || string.IsNullOrWhiteSpace(itemId) || backpack.GetCount(itemId) <= 0)
            {
                return false;
            }

            WeaponDefinition weapon = ResolveWeapon(itemId);
            if (weapon == null)
            {
                return false;
            }

            ItemPickupComponent pickup = CreateWeaponPickup(weapon);
            if (pickup == null)
            {
                return false;
            }

            if (!backpack.TryRemoveItem(itemId, 1))
            {
                DestroyPickup(pickup.gameObject);
                return false;
            }

            LastSpawnedPickup = pickup;
            return true;
        }

        private WeaponDefinition ResolveWeapon(string itemId)
        {
            if (weaponDefinitionResolver == null)
            {
                weaponDefinitionResolver = weaponDefinitionResolverSource as IWeaponDefinitionResolver ?? FindWeaponDefinitionResolver();
            }

            return weaponDefinitionResolver != null ? weaponDefinitionResolver.Resolve(itemId) : null;
        }

        private IWeaponDefinitionResolver FindWeaponDefinitionResolver()
        {
            var behaviours = FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IWeaponDefinitionResolver resolver)
                {
                    return resolver;
                }
            }

            return null;
        }

        private ItemPickupComponent CreateWeaponPickup(WeaponDefinition weapon)
        {
            var pickupObject = new GameObject("Dropped_" + weapon.ItemId, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
            ResolveDropPositions(out Vector3 startPosition, out Vector3 targetPosition);
            pickupObject.transform.position = startPosition;

            var renderer = pickupObject.GetComponent<SpriteRenderer>();
            renderer.sprite = weapon.Icon;
            renderer.sortingOrder = 12;

            var body = pickupObject.GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = pickupObject.GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.25f;

            var pickup = pickupObject.AddComponent<ItemPickupComponent>();
            pickup.Kind = ItemPickupKind.Weapon;
            pickup.ItemDefinition = weapon;
            pickup.Amount = 1;
            pickup.SetPickupDelay(pickupDelaySeconds);

            pickupObject.AddComponent<ItemPickupVisual>();
            var spillMotion = pickupObject.AddComponent<LootSpillMotion>();
            spillMotion.Initialize(targetPosition, dropSlideDuration);
            return pickup;
        }

        private void ResolveDropPositions(out Vector3 startPosition, out Vector3 targetPosition)
        {
            Transform origin = dropOrigin != null ? dropOrigin : transform;
            Vector3 facingOffset = -origin.up;
            if (facingOffset.sqrMagnitude <= 0.0001f)
            {
                facingOffset = Vector3.down;
            }

            Vector3 direction = Quaternion.Euler(0f, 0f, Random.Range(-dropScatterDegrees, dropScatterDegrees)) * facingOffset.normalized;
            startPosition = origin.position + direction * Mathf.Max(0f, dropStartDistance);
            targetPosition = origin.position + direction * Mathf.Max(dropStartDistance, dropSlideDistance);
        }

        private static void DestroyPickup(GameObject pickupObject)
        {
            if (Application.isPlaying)
            {
                Destroy(pickupObject);
            }
            else
            {
                DestroyImmediate(pickupObject);
            }
        }
    }
}

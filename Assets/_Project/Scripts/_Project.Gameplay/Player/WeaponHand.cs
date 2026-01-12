using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using UnityEngine;

public class WeaponHand : MonoBehaviour
{
    [SerializeField] private InventoryStateComponent inventorySource;
    [SerializeField] private MonoBehaviour resolverSource;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform handSocket;

    private InventoryState inventory;
    private IWeaponDefinitionResolver resolver;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    public void Initialize()
    {
        EnsureReferences();
        if (inventory != null)
        {
            inventory.OnInventoryChanged += OnInventoryChanged;
        }

        Refresh();
    }

    private void OnInventoryChanged(InventoryChangeFlags flags)
    {
        if ((flags & InventoryChangeFlags.Weapons) == 0)
        {
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        var definition = ResolveDefinition();
        var sprite = definition != null ? definition.HandSprite : null;
        spriteRenderer.sprite = sprite;
        spriteRenderer.enabled = sprite != null;

        if (sprite == null)
        {
            return;
        }

        if (handSocket != null && spriteRenderer.transform.parent != handSocket)
        {
            spriteRenderer.transform.SetParent(handSocket, false);
        }

        spriteRenderer.transform.localPosition = definition.HandleOffset;
    }

    private WeaponDefinition ResolveDefinition()
    {
        if (inventory == null || resolver == null)
        {
            return null;
        }

        var weaponId = inventory.GetWeaponId(inventory.ActiveWeaponSlotIndex);
        return resolver.Resolve(weaponId);
    }

    private void EnsureReferences()
    {
        if (inventorySource == null)
        {
            inventorySource = GetComponentInParent<InventoryStateComponent>();
        }

        inventory = inventorySource != null ? inventorySource.State : null;

        if (resolver == null)
        {
            resolver = resolverSource as IWeaponDefinitionResolver ?? FindResolver();
            if (resolverSource == null && resolver is MonoBehaviour behaviour)
            {
                resolverSource = behaviour;
            }
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (handSocket == null)
        {
            handSocket = transform;
        }
    }

    private IWeaponDefinitionResolver FindResolver()
    {
        var behaviours = GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IWeaponDefinitionResolver found)
            {
                return found;
            }
        }

        return null;
    }
}

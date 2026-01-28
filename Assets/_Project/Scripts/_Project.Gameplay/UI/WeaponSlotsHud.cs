using Castlebound.Gameplay.Combat;
using Castlebound.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotsHud : MonoBehaviour
{
    [SerializeField] private InventoryStateComponent inventorySource;
    [SerializeField] private MonoBehaviour resolverSource;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private Image[] activeSlotHighlights;
    [SerializeField] private Sprite placeholderSprite;

    private InventoryState inventory;
    private IWeaponDefinitionResolver resolver;

    void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (inventorySource == null)
        {
            inventorySource = GetComponentInParent<InventoryStateComponent>();
            if (inventorySource == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                inventorySource = player != null ? player.GetComponent<InventoryStateComponent>() : null;
            }
        }

        resolver = resolverSource as IWeaponDefinitionResolver ?? FindResolver();
        if (resolverSource == null && resolver is MonoBehaviour behaviour)
        {
            resolverSource = behaviour;
        }

        inventory = inventorySource != null ? inventorySource.State : null;
        if (inventory != null)
        {
            inventory.OnInventoryChanged += OnInventoryChanged;
        }

        Refresh();
    }

    void OnDisable()
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

        Refresh();
    }

    private void Refresh()
    {
        UpdateSlotIcons();
        UpdateActiveHighlights();
    }

    private void UpdateSlotIcons()
    {
        if (slotIcons == null)
        {
            return;
        }

        for (int i = 0; i < slotIcons.Length; i++)
        {
            var icon = slotIcons[i];
            if (icon == null)
            {
                continue;
            }

            Sprite sprite = null;
            if (inventory != null)
            {
                var weaponId = inventory.GetWeaponId(i);
                var definition = resolver != null ? resolver.Resolve(weaponId) : null;
                sprite = definition != null ? definition.Icon : null;
            }

            if (sprite == null)
            {
                sprite = placeholderSprite;
            }

            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }
    }

    private void UpdateActiveHighlights()
    {
        if (activeSlotHighlights == null)
        {
            return;
        }

        for (int i = 0; i < activeSlotHighlights.Length; i++)
        {
            var highlight = activeSlotHighlights[i];
            if (highlight == null)
            {
                continue;
            }

            highlight.enabled = inventory != null && inventory.ActiveWeaponSlotIndex == i;
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

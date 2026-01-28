using Castlebound.Gameplay.Inventory;
using TMPro;
using UnityEngine;

public class GoldXpHud : MonoBehaviour
{
    [SerializeField] private InventoryStateComponent inventorySource;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI xpText;

    private InventoryState inventory;

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
        if ((flags & InventoryChangeFlags.Currency) == 0)
        {
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (goldText != null)
        {
            goldText.text = inventory != null ? inventory.Gold.ToString() : string.Empty;
        }

        if (xpText != null)
        {
            xpText.text = inventory != null ? inventory.Xp.ToString() : string.Empty;
        }
    }
}

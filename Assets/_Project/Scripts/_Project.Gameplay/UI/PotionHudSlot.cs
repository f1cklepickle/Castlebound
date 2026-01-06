using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Castlebound.Gameplay.Inventory;

public class PotionHudSlot : MonoBehaviour
{
    [SerializeField] private InventoryStateComponent inventorySource;
    [SerializeField] private PotionUseController potionUseController;
    [SerializeField] private PotionDefinitionResolverComponent resolverSource;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Sprite placeholderSprite;

    private InventoryState inventory;

    void Awake()
    {
        if (iconImage == null) iconImage = FindChildImage("Icon");
        if (countText == null) countText = FindChildText("CountText");
        if (cooldownOverlay == null) cooldownOverlay = FindChildImage("CooldownOverlay");
    }

    void OnEnable()
    {
        if (inventorySource == null) inventorySource = GetComponentInParent<InventoryStateComponent>();
        if (potionUseController == null) potionUseController = GetComponentInParent<PotionUseController>();
        if (resolverSource == null) resolverSource = GetComponentInParent<PotionDefinitionResolverComponent>();

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

    void Update()
    {
        UpdateCooldownOverlay();
    }

    void OnInventoryChanged(InventoryChangeFlags flags)
    {
        if ((flags & InventoryChangeFlags.Potions) == 0)
        {
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        int count = inventory != null ? inventory.PotionCount : 0;
        if (countText != null)
        {
            countText.text = count > 0 ? count.ToString() : string.Empty;
        }

        if (iconImage != null)
        {
            iconImage.enabled = true;
            Sprite icon = inventory != null ? ResolveIcon() : null;
            iconImage.sprite = icon != null ? icon : placeholderSprite;
        }

        UpdateCooldownOverlay();
    }

    private Sprite ResolveIcon()
    {
        if (inventory == null || resolverSource == null || string.IsNullOrWhiteSpace(inventory.PotionId))
        {
            return null;
        }

        PotionDefinition definition = resolverSource.Resolve(inventory.PotionId);
        return definition != null ? definition.Icon : null;
    }

    private void UpdateCooldownOverlay()
    {
        if (cooldownOverlay == null || potionUseController == null)
        {
            return;
        }

        float duration = potionUseController.CurrentCooldownSeconds;
        float remaining = potionUseController.CooldownRemaining;
        if (duration <= 0f)
        {
            cooldownOverlay.fillAmount = 0f;
            return;
        }

        cooldownOverlay.fillAmount = Mathf.Clamp01(remaining / duration);
    }

    private Image FindChildImage(string name)
    {
        Transform child = transform.Find(name);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private TextMeshProUGUI FindChildText(string name)
    {
        Transform child = transform.Find(name);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }
}

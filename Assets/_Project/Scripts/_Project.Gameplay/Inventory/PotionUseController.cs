using UnityEngine;
using UnityEngine.InputSystem;

namespace Castlebound.Gameplay.Inventory
{
    public class PotionUseController : MonoBehaviour
    {
        [SerializeField] private InventoryStateComponent inventorySource;
        [SerializeField] private PotionDefinitionResolverComponent resolverSource;
        [SerializeField] private MonoBehaviour healTargetSource;

        private PotionConsumeController consumeController;
        private IHealable healTarget;

        private void Awake()
        {
            EnsureController();
        }

        public void OnUsePotion(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            TryConsume();
        }

        public bool TryConsume()
        {
            EnsureController();
            if (inventorySource == null || consumeController == null)
            {
                return false;
            }

            return consumeController.TryConsume(inventorySource.State);
        }

        public float CooldownRemaining
        {
            get
            {
                EnsureController();
                return consumeController != null ? consumeController.CooldownRemaining : 0f;
            }
        }

        public float CurrentCooldownSeconds
        {
            get
            {
                EnsureController();
                return consumeController != null ? consumeController.CurrentCooldownSeconds : 0f;
            }
        }

        public void SetInventorySource(InventoryStateComponent inventory)
        {
            inventorySource = inventory;
        }

        public void SetResolverSource(PotionDefinitionResolverComponent resolver)
        {
            resolverSource = resolver;
        }

        public void SetHealTargetSource(MonoBehaviour source)
        {
            healTargetSource = source;
        }

        private void EnsureController()
        {
            if (inventorySource == null)
            {
                inventorySource = GetComponentInParent<InventoryStateComponent>();
            }

            if (resolverSource == null)
            {
                resolverSource = GetComponentInParent<PotionDefinitionResolverComponent>();
            }

            healTarget = healTargetSource as IHealable;
            if (healTarget == null)
            {
                healTarget = GetComponentInParent<IHealable>();
            }

            if (consumeController == null)
            {
                consumeController = new PotionConsumeController(resolverSource, new UnityTimeProvider(), healTarget);
            }
        }
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public class PotionConsumeController
    {
        private readonly IPotionDefinitionResolver resolver;
        private readonly ITimeProvider timeProvider;
        private readonly IHealable healTarget;
        private float nextUseTime;

        public float CurrentCooldownSeconds { get; private set; }

        public PotionConsumeController(IPotionDefinitionResolver resolver, ITimeProvider timeProvider, IHealable healTarget)
        {
            this.resolver = resolver;
            this.timeProvider = timeProvider;
            this.healTarget = healTarget;
        }

        public float CooldownRemaining
        {
            get
            {
                if (timeProvider == null)
                {
                    return 0f;
                }

                return Mathf.Max(0f, nextUseTime - timeProvider.Time);
            }
        }

        public bool TryConsume(InventoryState inventory)
        {
            if (inventory == null || resolver == null || timeProvider == null || healTarget == null)
            {
                return false;
            }

            if (inventory.PotionCount <= 0 || string.IsNullOrWhiteSpace(inventory.PotionId))
            {
                return false;
            }

            if (timeProvider.Time < nextUseTime)
            {
                return false;
            }

            PotionDefinition definition = resolver.Resolve(inventory.PotionId);
            if (definition == null)
            {
                return false;
            }

            if (!inventory.TryConsumePotion(1))
            {
                return false;
            }

            healTarget.Heal(definition.HealAmount);
            CurrentCooldownSeconds = Mathf.Max(0f, definition.CooldownSeconds);
            nextUseTime = timeProvider.Time + CurrentCooldownSeconds;
            return true;
        }
    }
}

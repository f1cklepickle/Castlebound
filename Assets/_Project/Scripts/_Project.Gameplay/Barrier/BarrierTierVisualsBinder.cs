using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    public class BarrierTierVisualsBinder : MonoBehaviour
    {
        [SerializeField] private BarrierUpgradeController upgradeController;
        [SerializeField] private BarrierHealth health;
        [SerializeField] private BarrierTierVisuals visuals;

        private int lastTier = -1;
        private int lastHealth = int.MinValue;
        private int lastMaxHealth = int.MinValue;

        public void SetReferences(BarrierUpgradeController controller, BarrierHealth barrierHealth, BarrierTierVisuals tierVisuals)
        {
            upgradeController = controller;
            health = barrierHealth;
            visuals = tierVisuals;
        }

        private void OnEnable()
        {
            Tick();
        }

        private void Update()
        {
            Tick();
        }

        public void Tick()
        {
            if (visuals == null)
            {
                return;
            }

            int tier = upgradeController != null ? upgradeController.GetCurrentTier() : 0;
            int current = health != null ? health.CurrentHealth : 0;
            int max = health != null ? health.MaxHealth : 0;

            if (tier == lastTier && current == lastHealth && max == lastMaxHealth)
            {
                return;
            }

            lastTier = tier;
            lastHealth = current;
            lastMaxHealth = max;

            visuals.UpdateVisuals(tier, current, max);
        }
    }
}

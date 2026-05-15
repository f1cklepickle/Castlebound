using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Tower
{
    public class TowerUpgradeController : MonoBehaviour
    {
        [SerializeField] private TowerUpgradeConfig config;
        [SerializeField] private TowerRuntime runtime;
        [SerializeField] private TowerAttackController attackController;
        [SerializeField] private TowerTargetingController targetingController;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private FeedbackEventChannel feedbackChannel;
        [SerializeField] private InventoryStateComponent inventorySource;

        private readonly TowerUpgradeState state = new TowerUpgradeState();
        private InventoryState inventory;
        private WavePhaseTracker phaseTracker;

        public TowerUpgradeConfig Config
        {
            get => config;
            set => config = value;
        }

        public TowerUpgradeState State => state;

        private void Reset()
        {
            EnsureReferences();
        }

        private void Awake()
        {
            EnsureReferences();
            ApplyCurrentUpgrades();
        }

        public bool TryUpgrade(TowerUpgradeTrack track)
        {
            if (!CanUpgrade(track))
            {
                RaiseUpgradeFeedback(FeedbackCueType.UpgradeDenied);
                return false;
            }

            var source = GetInventory();
            int cost = config.GetCost(track, state);
            if (cost > 0 && (source == null || !source.TrySpendGold(cost)))
            {
                RaiseUpgradeFeedback(FeedbackCueType.UpgradeDenied);
                return false;
            }

            state.Increment(track);
            ApplyCurrentUpgrades();
            RaiseUpgradeFeedback(FeedbackCueType.UpgradeSuccess);
            return true;
        }

        public bool CanUpgrade(TowerUpgradeTrack track)
        {
            return IsInPreWave() && config != null && config.CanUpgrade(track, state) && HasRequiredTarget(track);
        }

        public int GetUpgradeCost(TowerUpgradeTrack track)
        {
            return config != null ? config.GetCost(track, state) : 0;
        }

        public int GetLevel(TowerUpgradeTrack track)
        {
            return state.GetLevel(track);
        }

        public void ApplyCurrentUpgrades()
        {
            if (config == null)
            {
                return;
            }

            EnsureReferences();
            ApplyHealth();
            ApplyAttack();
            ApplyRange();
        }

        public void SetInventory(InventoryState state)
        {
            inventory = state;
        }

        public void SetPhaseTracker(WavePhaseTracker tracker)
        {
            phaseTracker = tracker;
        }

        public void SetFeedbackChannel(FeedbackEventChannel channel)
        {
            feedbackChannel = channel;
        }

        private void ApplyHealth()
        {
            if (runtime == null || !config.Health.Enabled)
            {
                return;
            }

            int previousMax = runtime.MaxHealth;
            int nextMax = Mathf.Max(1, config.GetMaxHealth(state));
            runtime.MaxHealth = nextMax;
            int delta = Mathf.Max(0, nextMax - previousMax);
            if (delta > 0)
            {
                runtime.CurrentHealth = Mathf.Min(runtime.CurrentHealth + delta, runtime.MaxHealth);
            }
        }

        private void ApplyAttack()
        {
            if (attackController == null)
            {
                return;
            }

            if (config.Damage.Enabled)
            {
                attackController.Damage = config.GetDamage(state);
            }

            if (config.FireRate.Enabled)
            {
                attackController.CooldownSeconds = config.GetCooldownSeconds(state);
            }
        }

        private void ApplyRange()
        {
            if (targetingController == null || !config.Range.Enabled)
            {
                return;
            }

            targetingController.MaxRange = config.GetMaxRange(state);
        }

        private bool HasRequiredTarget(TowerUpgradeTrack track)
        {
            return track switch
            {
                TowerUpgradeTrack.Damage => attackController != null,
                TowerUpgradeTrack.FireRate => attackController != null,
                TowerUpgradeTrack.Health => runtime != null,
                TowerUpgradeTrack.Range => targetingController != null,
                _ => false
            };
        }

        private void EnsureReferences()
        {
            if (runtime == null)
            {
                runtime = GetComponent<TowerRuntime>();
            }

            if (attackController == null)
            {
                attackController = GetComponent<TowerAttackController>();
            }

            if (targetingController == null)
            {
                targetingController = GetComponent<TowerTargetingController>();
            }
        }

        private bool IsInPreWave()
        {
            if (phaseTracker != null)
            {
                return phaseTracker.CurrentPhase == WavePhase.PreWave;
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            return waveRunner != null
                && waveRunner.PhaseTracker != null
                && waveRunner.PhaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private InventoryState GetInventory()
        {
            if (inventory != null)
            {
                return inventory;
            }

            if (inventorySource == null)
            {
                inventorySource = GetComponent<InventoryStateComponent>();
            }

            inventory = inventorySource != null ? inventorySource.State : null;
            return inventory;
        }

        private void RaiseUpgradeFeedback(FeedbackCueType type)
        {
            if (feedbackChannel == null)
            {
                return;
            }

            feedbackChannel.Raise(new FeedbackCue(type, transform.position, GetInstanceID()));
        }
    }
}

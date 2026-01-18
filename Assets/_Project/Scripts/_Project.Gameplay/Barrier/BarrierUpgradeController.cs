using Castlebound.Gameplay.Inventory;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Barrier
{
    public class BarrierUpgradeController : MonoBehaviour
    {
        [SerializeField] private BarrierUpgradeConfig config;
        [SerializeField] private BarrierUpgradeStateAsset sharedState;
        [SerializeField] private BarrierHealth target;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private FeedbackEventChannel feedbackChannel;
        [SerializeField] private InventoryStateComponent inventorySource;

        private BarrierUpgradeState localState;
        private InventoryState inventory;
        private WavePhaseTracker phaseTracker;

        public BarrierUpgradeConfig Config
        {
            get => config;
            set => config = value;
        }

        public BarrierUpgradeStateAsset SharedState
        {
            get => sharedState;
            set => sharedState = value;
        }

        public BarrierUpgradeState State => localState ??= new BarrierUpgradeState();

        private BarrierHealth Target => target != null ? target : target = GetComponent<BarrierHealth>();

        private void OnEnable()
        {
            ApplyCurrentTier();
            HookWaveRunner();
        }

        private void OnDisable()
        {
            UnhookWaveRunner();
        }

        public void ApplyCurrentTier()
        {
            if (config == null)
            {
                return;
            }

            var health = Target;
            if (health == null)
            {
                return;
            }

            int tier = sharedState != null ? sharedState.Tier : State.Tier;
            int maxHealth = config.GetMaxHealthForTier(tier);
            health.MaxHealth = maxHealth;
        }

        public void HandleWaveStarted()
        {
            ApplyCurrentTier();
        }

        public bool TryUpgrade()
        {
            if (!IsInPreWave())
            {
                RaiseUpgradeFeedback(FeedbackCueType.UpgradeDenied);
                return false;
            }

            var source = GetInventory();
            if (source == null || config == null)
            {
                RaiseUpgradeFeedback(FeedbackCueType.UpgradeDenied);
                return false;
            }

            var state = sharedState != null ? null : State;
            int tier = sharedState != null ? sharedState.Tier : state.Tier;
            int cost = config.GetUpgradeCostForTier(tier);
            if (!source.TrySpendGold(cost))
            {
                RaiseUpgradeFeedback(FeedbackCueType.UpgradeDenied);
                return false;
            }

            if (sharedState != null)
            {
                sharedState.IncrementTier();
            }
            else
            {
                state.IncrementTier();
            }

            ApplyCurrentTier();
            RaiseUpgradeFeedback(FeedbackCueType.UpgradeSuccess);
            return true;
        }

        public void SetFeedbackChannel(FeedbackEventChannel channel)
        {
            feedbackChannel = channel;
        }

        public void SetInventory(InventoryState state)
        {
            inventory = state;
        }

        public void SetPhaseTracker(WavePhaseTracker tracker)
        {
            phaseTracker = tracker;
        }

        public int GetCurrentTier()
        {
            return sharedState != null ? sharedState.Tier : State.Tier;
        }

        public int GetUpgradeCost()
        {
            if (config == null)
            {
                return 0;
            }

            return config.GetUpgradeCostForTier(GetCurrentTier());
        }

        public int GetCurrentMaxHealth()
        {
            if (config == null)
            {
                return 0;
            }

            return config.GetMaxHealthForTier(GetCurrentTier());
        }

        public int GetCurrentHealth()
        {
            var health = Target;
            return health != null ? health.CurrentHealth : 0;
        }

        private void HookWaveRunner()
        {
            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            if (waveRunner != null)
            {
                waveRunner.OnWaveStarted += OnWaveStarted;
            }
        }

        private void UnhookWaveRunner()
        {
            if (waveRunner != null)
            {
                waveRunner.OnWaveStarted -= OnWaveStarted;
            }
        }

        private void OnWaveStarted(int waveIndex)
        {
            HandleWaveStarted();
        }

        private bool IsInPreWave()
        {
            if (phaseTracker != null)
            {
                return phaseTracker.CurrentPhase == WavePhase.PreWave;
            }

            if (waveRunner != null && waveRunner.PhaseTracker != null)
            {
                return waveRunner.PhaseTracker.CurrentPhase == WavePhase.PreWave;
            }

            return false;
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

            var targetTransform = Target != null ? Target.transform : null;
            var position = targetTransform != null ? targetTransform.position : Vector3.zero;
            var targetId = Target != null ? Target.GetInstanceID() : 0;
            var cue = new FeedbackCue(type, position, targetId);
            feedbackChannel.Raise(cue);
        }
    }
}

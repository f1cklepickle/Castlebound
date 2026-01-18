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

        private BarrierUpgradeState localState;

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
    }
}

using System;
using Castlebound.Gameplay.Spawning;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    [RequireComponent(typeof(BarrierHealth))]
    public class BarrierPressureTracker : MonoBehaviour
    {
        [SerializeField] private int breaksPerWave = 3;
        [SerializeField] private MonoBehaviour waveIndexProvider;

        public event Action<string> OnPressureTriggered
        {
            add
            {
                EnsureInitialized();
                onPressureTriggered += value;
            }
            remove => onPressureTriggered -= value;
        }

        private BarrierHealth barrierHealth;
        private bool subscribed;
        private bool hasWaveIndex;
        private Action<string> onPressureTriggered;
        private string barrierId;
        private int currentWaveIndex;
        private int breakCount;
        private bool triggeredThisWave;

        private IWaveIndexProvider WaveProvider => waveIndexProvider as IWaveIndexProvider;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
        }

        private void OnValidate()
        {
            if (breaksPerWave < 1)
            {
                breaksPerWave = 1;
            }
        }

        private void OnDisable()
        {
            if (barrierHealth != null && subscribed)
            {
                barrierHealth.OnBroken -= HandleBarrierBroken;
                subscribed = false;
            }
        }

        private void HandleBarrierBroken()
        {
            EnsureInitialized();

            var waveIndex = GetWaveIndex();
            if (waveIndex != currentWaveIndex)
            {
                currentWaveIndex = waveIndex;
                breakCount = 0;
                triggeredThisWave = false;
            }

            breakCount++;

            if (triggeredThisWave || breakCount < breaksPerWave)
            {
                return;
            }

            triggeredThisWave = true;
            onPressureTriggered?.Invoke(barrierId);
        }

        private int GetWaveIndex()
        {
            var provider = WaveProvider;
            return provider != null ? provider.CurrentWaveIndex : 1;
        }

        private string ResolveBarrierId()
        {
            var gate = GetComponent<GateIdProvider>();
            if (gate != null && !string.IsNullOrWhiteSpace(gate.GateId))
            {
                return gate.GateId;
            }

            return gameObject.name;
        }

    private void EnsureInitialized()
    {
        if (barrierHealth == null)
        {
            barrierHealth = GetComponent<BarrierHealth>();
        }

            if (waveIndexProvider == null)
            {
                waveIndexProvider = FindObjectOfType<WaveIndexProviderComponent>();
            }

            if (barrierId == null)
            {
                barrierId = ResolveBarrierId();
            }

            if (!hasWaveIndex)
            {
                currentWaveIndex = GetWaveIndex();
                hasWaveIndex = true;
            }

            if (barrierHealth != null && !subscribed)
            {
            barrierHealth.OnBroken += HandleBarrierBroken;
            subscribed = true;
        }
    }

#if UNITY_EDITOR
    public void Debug_ForceInitialize()
    {
        EnsureInitialized();
    }
#endif
}
}

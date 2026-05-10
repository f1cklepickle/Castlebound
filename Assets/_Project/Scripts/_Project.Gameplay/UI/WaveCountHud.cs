using Castlebound.Gameplay.Spawning;
using TMPro;
using UnityEngine;

namespace Castlebound.Gameplay.UI
{
    public class WaveCountHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private string format = "Wave {0}";
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private MonoBehaviour waveIndexProviderSource;

        private IWaveIndexProvider waveIndexProvider;
        private bool isHooked;

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            UnhookWaveRunner();
        }

        public void Initialize()
        {
            ResolveReferences();
            HookWaveRunner();
            Refresh();
        }

        public void SetWaveRunner(EnemySpawnerRunner runner)
        {
            if (waveRunner == runner)
            {
                return;
            }

            UnhookWaveRunner();
            waveRunner = runner;
            ResolveProvider();
            HookWaveRunner();
            Refresh();
        }

        public void SetWaveIndexProvider(IWaveIndexProvider provider)
        {
            waveIndexProvider = provider;
            waveIndexProviderSource = provider as MonoBehaviour;
            Refresh();
        }

        public void Refresh()
        {
            if (waveText == null)
            {
                return;
            }

            waveText.text = waveIndexProvider != null
                ? FormatWaveIndex(waveIndexProvider.CurrentWaveIndex)
                : string.Empty;
        }

        private void ResolveReferences()
        {
            if (waveText == null)
            {
                waveText = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (waveRunner == null)
            {
                waveRunner = GetComponentInParent<EnemySpawnerRunner>();
                if (waveRunner == null)
                {
                    waveRunner = FindObjectOfType<EnemySpawnerRunner>();
                }
            }

            ResolveProvider();
        }

        private void ResolveProvider()
        {
            waveIndexProvider = waveIndexProviderSource as IWaveIndexProvider;

            if (waveIndexProvider == null && waveRunner != null)
            {
                waveIndexProvider = FindProviderOn(waveRunner.gameObject);
                waveIndexProviderSource = waveIndexProvider as MonoBehaviour;
            }

            if (waveIndexProvider == null)
            {
                waveIndexProvider = FindProviderInParents();
                waveIndexProviderSource = waveIndexProvider as MonoBehaviour;
            }

            if (waveIndexProvider == null)
            {
                var providerComponent = FindObjectOfType<WaveIndexProviderComponent>();
                waveIndexProvider = providerComponent;
                waveIndexProviderSource = providerComponent;
            }
        }

        private IWaveIndexProvider FindProviderInParents()
        {
            var behaviours = GetComponentsInParent<MonoBehaviour>(true);
            return FindProviderInBehaviours(behaviours);
        }

        private static IWaveIndexProvider FindProviderOn(GameObject source)
        {
            if (source == null)
            {
                return null;
            }

            var behaviours = source.GetComponents<MonoBehaviour>();
            return FindProviderInBehaviours(behaviours);
        }

        private static IWaveIndexProvider FindProviderInBehaviours(MonoBehaviour[] behaviours)
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IWaveIndexProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private void HookWaveRunner()
        {
            if (isHooked || waveRunner == null)
            {
                return;
            }

            waveRunner.OnWaveStarted += OnWaveStarted;
            isHooked = true;
        }

        private void UnhookWaveRunner()
        {
            if (!isHooked || waveRunner == null)
            {
                isHooked = false;
                return;
            }

            waveRunner.OnWaveStarted -= OnWaveStarted;
            isHooked = false;
        }

        private void OnWaveStarted(int waveIndex)
        {
            if (waveText != null)
            {
                waveText.text = FormatWaveIndex(waveIndex);
            }
        }

        private string FormatWaveIndex(int waveIndex)
        {
            var displayFormat = string.IsNullOrEmpty(format) ? "{0}" : format;
            return string.Format(displayFormat, Mathf.Max(1, waveIndex));
        }
    }
}

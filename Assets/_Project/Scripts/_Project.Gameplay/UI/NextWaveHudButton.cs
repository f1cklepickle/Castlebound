using Castlebound.Gameplay.Spawning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class NextWaveHudButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private RectTransform hudParent;
        [SerializeField] private string label = "Start Wave";

        private WavePhaseTracker phaseTracker;
        private bool isHooked;

        public Button Button => button;

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            UnhookPhaseTracker();
            if (button != null)
            {
                button.onClick.RemoveListener(StartNextWave);
            }
        }

        public void Initialize()
        {
            ResolveReferences();
            EnsureButton();
            HookPhaseTracker();
            RefreshVisibility();
        }

        public void SetWaveRunner(EnemySpawnerRunner runner)
        {
            if (waveRunner == runner)
            {
                return;
            }

            UnhookPhaseTracker();
            waveRunner = runner;
            phaseTracker = waveRunner != null ? waveRunner.PhaseTracker : phaseTracker;
            HookPhaseTracker();
            RefreshVisibility();
        }

        public void SetPhaseTracker(WavePhaseTracker tracker)
        {
            if (phaseTracker == tracker)
            {
                return;
            }

            UnhookPhaseTracker();
            phaseTracker = tracker;
            HookPhaseTracker();
            RefreshVisibility();
        }

        public void SetButton(Button nextWaveButton)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(StartNextWave);
            }

            button = nextWaveButton;
            if (button != null)
            {
                button.onClick.AddListener(StartNextWave);
            }

            RefreshVisibility();
        }

        public void StartNextWave()
        {
            if (!IsInPreWave())
            {
                return;
            }

            if (waveRunner != null)
            {
                waveRunner.StartNextWaveFromMenu();
            }
            else
            {
                phaseTracker?.SetPhase(WavePhase.InWave);
            }

            RefreshVisibility();
        }

        private void ResolveReferences()
        {
            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            if (phaseTracker == null && waveRunner != null)
            {
                phaseTracker = waveRunner.PhaseTracker;
            }

            if (hudParent == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                hudParent = canvas != null ? canvas.transform as RectTransform : transform as RectTransform;
            }
        }

        private void EnsureButton()
        {
            if (button == null)
            {
                button = CreateButton(hudParent != null ? hudParent : transform, label);
            }

            button.onClick.RemoveListener(StartNextWave);
            button.onClick.AddListener(StartNextWave);
        }

        private void HookPhaseTracker()
        {
            if (isHooked || phaseTracker == null)
            {
                return;
            }

            phaseTracker.PhaseChanged += OnPhaseChanged;
            isHooked = true;
        }

        private void UnhookPhaseTracker()
        {
            if (!isHooked || phaseTracker == null)
            {
                isHooked = false;
                return;
            }

            phaseTracker.PhaseChanged -= OnPhaseChanged;
            isHooked = false;
        }

        private void OnPhaseChanged(WavePhase phase)
        {
            RefreshVisibility();
        }

        private bool IsInPreWave()
        {
            return phaseTracker == null || phaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private void RefreshVisibility()
        {
            if (button == null)
            {
                return;
            }

            var visible = IsInPreWave();
            button.gameObject.SetActive(visible);
            button.interactable = visible;
        }

        private static Button CreateButton(Transform parent, string labelText)
        {
            var buttonObject = new GameObject("NextWaveButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -18f);
            rect.sizeDelta = new Vector2(156f, 42f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.17f, 0.2f, 0.95f);

            var button = buttonObject.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.15f, 0.17f, 0.2f, 0.95f);
            colors.highlightedColor = new Color(0.24f, 0.27f, 0.31f, 1f);
            colors.pressedColor = new Color(0.1f, 0.12f, 0.15f, 1f);
            colors.disabledColor = new Color(0.12f, 0.13f, 0.15f, 0.45f);
            button.colors = colors;
            button.targetGraphic = image;

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = labelText;
            text.fontSize = 16;
            text.fontSizeMin = 11;
            text.fontSizeMax = 16;
            text.enableAutoSizing = true;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            return button;
        }
    }
}

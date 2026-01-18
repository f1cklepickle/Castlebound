using System;
using Castlebound.Gameplay.Spawning;
using UnityEngine;
using UnityEngine.UI;

namespace Castlebound.Gameplay.UI
{
    public class UpgradeMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform menuRoot;
        [SerializeField] private bool autoOpenEveryPreWave = true;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private bool pausePlayerWhileOpen = true;
        [SerializeField] private PlayerController playerController;

        private WavePhaseTracker phaseTracker;

        public bool IsMenuOpen { get; private set; }
        public event Action<bool> MenuStateChanged;

        public RectTransform MenuRoot => menuRoot;

        private void OnEnable()
        {
            ResolvePhaseTracker();
            HookPhaseTracker();
            ApplyMenuState(false);
        }

        private void OnDisable()
        {
            UnhookPhaseTracker();
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
        }

        public void SetAutoOpenOnFirstPreWave(bool enabled)
        {
            autoOpenEveryPreWave = enabled;
        }

        public void SetPlayerController(PlayerController controller)
        {
            playerController = controller;
        }

        public void ToggleMenu()
        {
            if (!IsInPreWave())
            {
                return;
            }

            if (IsMenuOpen)
            {
                CloseMenu();
                return;
            }

            ApplyMenuState(true);
        }

        public void CloseMenu()
        {
            ApplyMenuState(false);

            if (waveRunner != null)
            {
                waveRunner.StartNextWaveFromMenu();
                return;
            }

            phaseTracker?.SetPhase(WavePhase.InWave);
        }

        private void OnPhaseChanged(WavePhase phase)
        {
            if (phase != WavePhase.PreWave)
            {
                return;
            }

            if (!autoOpenEveryPreWave)
            {
                return;
            }

            ApplyMenuState(true);
        }

        private void ResolvePhaseTracker()
        {
            if (phaseTracker != null)
            {
                return;
            }

            if (waveRunner == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            phaseTracker = waveRunner != null ? waveRunner.PhaseTracker : null;
        }

        private void HookPhaseTracker()
        {
            if (phaseTracker != null)
            {
                phaseTracker.PhaseChanged += OnPhaseChanged;
            }
        }

        private void UnhookPhaseTracker()
        {
            if (phaseTracker != null)
            {
                phaseTracker.PhaseChanged -= OnPhaseChanged;
            }
        }

        private bool IsInPreWave()
        {
            return phaseTracker != null && phaseTracker.CurrentPhase == WavePhase.PreWave;
        }

        private void ApplyMenuState(bool open)
        {
            EnsureMenuRoot();
            IsMenuOpen = open;
            if (menuRoot != null)
            {
                menuRoot.gameObject.SetActive(open);
            }

            SetPlayerPaused(open);
            MenuStateChanged?.Invoke(open);
        }

        public RectTransform EnsureMenuRoot()
        {
            if (menuRoot != null)
            {
                return menuRoot;
            }

            var panel = new GameObject("UpgradeMenuPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(transform, false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520f, 260f);
            rect.anchoredPosition = Vector2.zero;

            var image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.7f);
            image.raycastTarget = true;

            menuRoot = rect;
            return menuRoot;
        }

        private void SetPlayerPaused(bool paused)
        {
            if (!pausePlayerWhileOpen)
            {
                return;
            }

            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }

            if (playerController != null)
            {
                playerController.SetInputLocked(paused);
                if (paused)
                {
                    playerController.StopMovement();
                }
            }
        }
    }
}

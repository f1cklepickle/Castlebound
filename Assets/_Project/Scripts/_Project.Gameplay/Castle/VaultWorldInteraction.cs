using Castlebound.Gameplay.Spawning;
using Castlebound.Gameplay.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Castlebound.Gameplay.Castle
{
    public class VaultWorldInteraction : MonoBehaviour
    {
        [SerializeField] private VaultPanelController vaultPanel;
        [SerializeField] private InventoryPanelController inventoryPanel;
        [SerializeField] private EnemySpawnerRunner waveRunner;
        [SerializeField] private VaultOutlinePresenter outlinePresenter;
        [SerializeField] private Collider2D touchTargetCollider;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float touchHoldSeconds = 0.5f;

        private WavePhaseTracker phaseTracker;
        private bool playerInRange;
        private float touchHoldTimer;
        private bool touchToggledThisPress;
        private bool phaseHooked;
        private readonly Collider2D[] rangeHits = new Collider2D[8];

        public bool PlayerInRange => playerInRange;

        private WavePhase CurrentPhase => phaseTracker != null ? phaseTracker.CurrentPhase : WavePhase.PreWave;

        private void OnEnable()
        {
            ResolveReferences();
            HookPhaseTracker();
            ApplyVisualState();
        }

        private void OnDisable()
        {
            UnhookPhaseTracker();
        }

        private void Update()
        {
            RefreshPlayerRange();

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryToggleVault();
            }

            UpdateTouchHold();
        }

        public void SetInventoryPanel(InventoryPanelController panel)
        {
            inventoryPanel = panel;
        }

        public void SetVaultPanel(VaultPanelController panel)
        {
            vaultPanel = panel;
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

            ApplyVisualState();
        }

        public void SetTouchTargetCollider(Collider2D collider)
        {
            touchTargetCollider = collider;
        }

        public void SetWorldCamera(Camera camera)
        {
            worldCamera = camera;
        }

        public void SetTouchHoldSeconds(float seconds)
        {
            touchHoldSeconds = Mathf.Max(0f, seconds);
        }

        public void SetPlayerInRange(bool inRange)
        {
            if (playerInRange == inRange)
            {
                return;
            }

            playerInRange = inRange;
            if (!playerInRange)
            {
                ResetTouchHold();
            }

            ApplyVisualState();
        }

        public bool TryOpenVault()
        {
            ResolveReferences();
            ApplyVisualState();

            if (!VaultInteractionPolicy.CanOpen(playerInRange, CurrentPhase) || vaultPanel == null)
            {
                return false;
            }

            if (phaseTracker != null)
            {
                vaultPanel.SetPhaseTracker(phaseTracker);
            }

            return vaultPanel.OpenFromWorld();
        }

        public bool TryToggleVault()
        {
            ResolveReferences();
            ApplyVisualState();

            if (vaultPanel != null && vaultPanel.IsOpen && playerInRange)
            {
                vaultPanel.ClosePanel();
                return true;
            }

            return TryOpenVault();
        }

        private bool TryToggleVaultFromHeldScreenPosition(Vector2 screenPosition, float deltaSeconds)
        {
            if (!playerInRange || !IsScreenPositionOverTouchTarget(screenPosition))
            {
                ResetTouchHold();
                return false;
            }

            if (touchToggledThisPress)
            {
                return false;
            }

            touchHoldTimer += Mathf.Max(0f, deltaSeconds);
            if (touchHoldTimer < touchHoldSeconds)
            {
                return false;
            }

            if (!TryToggleVault())
            {
                return false;
            }

            touchToggledThisPress = true;
            return true;
        }

        public void RefreshPlayerRange()
        {
            var rangeCollider = touchTargetCollider != null ? touchTargetCollider : GetComponent<Collider2D>();
            if (rangeCollider == null)
            {
                return;
            }

            Physics2D.SyncTransforms();
            var filter = new ContactFilter2D();
            filter.NoFilter();
            int hitCount = rangeCollider.OverlapCollider(filter, rangeHits);
            bool foundPlayer = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = rangeHits[i];
                if (hit != null && hit.CompareTag(playerTag))
                {
                    foundPlayer = true;
                    break;
                }
            }

            for (int i = 0; i < hitCount; i++)
            {
                rangeHits[i] = null;
            }

            SetPlayerInRange(foundPlayer);
        }

        private void ResolveReferences()
        {
            if (inventoryPanel == null)
            {
                inventoryPanel = FindObjectOfType<InventoryPanelController>();
            }

            if (vaultPanel == null)
            {
                vaultPanel = FindObjectOfType<VaultPanelController>();
            }

            if (vaultPanel == null && inventoryPanel != null)
            {
                vaultPanel = inventoryPanel.GetComponent<VaultPanelController>();
                if (vaultPanel == null)
                {
                    vaultPanel = inventoryPanel.gameObject.AddComponent<VaultPanelController>();
                }
            }

            if (waveRunner == null && phaseTracker == null)
            {
                waveRunner = FindObjectOfType<EnemySpawnerRunner>();
            }

            if (outlinePresenter == null)
            {
                outlinePresenter = GetComponentInChildren<VaultOutlinePresenter>(true);
                if (outlinePresenter == null)
                {
                    outlinePresenter = GetComponentInParent<VaultOutlinePresenter>(true);
                }

                if (outlinePresenter == null && transform.parent != null)
                {
                    outlinePresenter = transform.parent.GetComponentInChildren<VaultOutlinePresenter>(true);
                }
            }

            if (touchTargetCollider == null)
            {
                touchTargetCollider = GetComponent<Collider2D>();
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
                if (worldCamera == null)
                {
                    worldCamera = FindObjectOfType<Camera>();
                }
            }

            if (waveRunner != null && phaseTracker == null)
            {
                SetPhaseTracker(waveRunner.PhaseTracker);
            }

            if (inventoryPanel != null && phaseTracker != null)
            {
                inventoryPanel.SetPhaseTracker(phaseTracker);
            }

            if (vaultPanel != null && phaseTracker != null)
            {
                vaultPanel.SetPhaseTracker(phaseTracker);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other != null && other.CompareTag(playerTag))
            {
                SetPlayerInRange(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other != null && other.CompareTag(playerTag))
            {
                SetPlayerInRange(false);
            }
        }

        private void HandlePhaseChanged(WavePhase phase)
        {
            RefreshPlayerRange();
            ApplyVisualState();
        }

        private void HookPhaseTracker()
        {
            if (phaseTracker == null || phaseHooked)
            {
                return;
            }

            phaseTracker.PhaseChanged += HandlePhaseChanged;
            phaseHooked = true;
        }

        private void UnhookPhaseTracker()
        {
            if (phaseTracker == null || !phaseHooked)
            {
                return;
            }

            phaseTracker.PhaseChanged -= HandlePhaseChanged;
            phaseHooked = false;
        }

        private void ApplyVisualState()
        {
            if (outlinePresenter != null)
            {
                outlinePresenter.Apply(VaultInteractionPolicy.GetVisualState(playerInRange, CurrentPhase));
            }
        }

        private void UpdateTouchHold()
        {
            if (!playerInRange)
            {
                ResetTouchHold();
                return;
            }

            if (!TryReadPressedPointerPosition(out Vector2 screenPosition))
            {
                ResetTouchHold();
                return;
            }

            if (!IsScreenPositionOverTouchTarget(screenPosition))
            {
                ResetTouchHold();
                return;
            }

            if (touchToggledThisPress)
            {
                return;
            }

            TryToggleVaultFromHeldScreenPosition(screenPosition, Time.deltaTime);
        }

        private bool TryReadPressedPointerPosition(out Vector2 screenPosition)
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.isPressed)
                {
                    screenPosition = touch.position.ReadValue();
                    return true;
                }
            }

            if (Pointer.current != null && Pointer.current.press.isPressed)
            {
                screenPosition = Pointer.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        public bool IsScreenPositionOverTouchTarget(Vector2 screenPosition)
        {
            if (touchTargetCollider == null)
            {
                return false;
            }

            var cameraToUse = worldCamera != null ? worldCamera : Camera.main;
            if (cameraToUse == null)
            {
                cameraToUse = FindObjectOfType<Camera>();
                worldCamera = cameraToUse;
            }

            if (cameraToUse == null)
            {
                return false;
            }

            float distance = Mathf.Abs(cameraToUse.transform.position.z - transform.position.z);
            Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distance));
            return touchTargetCollider.OverlapPoint(worldPosition);
        }

        private void ResetTouchHold()
        {
            touchHoldTimer = 0f;
            touchToggledThisPress = false;
        }
    }
}

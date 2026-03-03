using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Castlebound.Gameplay.Barrier;

namespace Castlebound.Gameplay.Input
{
    public class TouchRepairButton : MonoBehaviour, IPointerClickHandler
    {
        private PlayerController _playerController;
        private IReadOnlyList<BarrierHealth> _barrierSource;
        private Image _image;

        public bool IsVisible { get; private set; }
        public event Action OnRepairRequested;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null)
                _image.enabled = false;
        }

        private void Start()
        {
            // Always resolve at runtime — cross-prefab serialized references are unreliable.
            _playerController = FindObjectOfType<PlayerController>();
        }

        // ── Injection (tests) ─────────────────────────────────────────────────

        public void SetBarrierSource(IReadOnlyList<BarrierHealth> source)
        {
            _barrierSource = source;
        }

        /// <summary>Used by tests to drive proximity checks without a live PlayerController.</summary>
        public void SetPlayerTransform(Transform t) { }

        // ── Runtime pointer event ─────────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
        {
            FireRepair();
        }

        // ── Runtime Update ────────────────────────────────────────────────────

        private void Update()
        {
            // Delegate to PlayerController so the button's visibility range is
            // always identical to the R-key repair check (same physics overlap + mask).
            if (_playerController != null)
                IsVisible = _playerController.HasRepairableBarrierInRange();

            if (_image != null)
                _image.enabled = IsVisible;
        }

        // ── Core logic (called directly from tests) ───────────────────────────

        public void UpdateProximity(Vector2 playerPosition, float checkRadius)
        {
            IsVisible = false;

            var all = _barrierSource ?? BarrierHealth.All;
            for (int i = 0; i < all.Count; i++)
            {
                var barrier = all[i];
                if (barrier == null || !barrier.IsBroken)
                    continue;

                var barrierPos = (Vector2)barrier.transform.position;
                if (Vector2.Distance(playerPosition, barrierPos) <= checkRadius)
                {
                    IsVisible = true;
                    return;
                }
            }
        }

        public void FireRepair()
        {
            OnRepairRequested?.Invoke();
        }
    }
}

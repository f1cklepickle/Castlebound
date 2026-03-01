using UnityEngine;
using UnityEngine.EventSystems;

namespace Castlebound.Gameplay.Input
{
    public class TouchAimAttackZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [field: SerializeField] public float AttackDeadzone { get; set; } = 50f;
        public Vector2 FacingDirection { get; private set; }
        public bool IsFiring { get; private set; }

        private Vector2 _touchAnchor;

        // ── Runtime pointer events ────────────────────────────────────────────

        public void OnPointerDown(PointerEventData eventData)
        {
            SimulatePointerDown(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Compute delta from where the thumb landed, not from last frame.
            SimulateAimInput(eventData.position - _touchAnchor);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SimulatePointerUp();
        }

        // ── Testable simulation API ───────────────────────────────────────────

        public void SimulatePointerDown(Vector2 screenPosition)
        {
            _touchAnchor = screenPosition;
        }

        public void SimulateAimInput(Vector2 delta)
        {
            if (delta == Vector2.zero)
                return;

            // Always update facing — deadzone only gates whether we fire.
            FacingDirection = delta.normalized;
            IsFiring = delta.magnitude > AttackDeadzone;
        }

        public void SimulatePointerUp()
        {
            FacingDirection = Vector2.zero;
            IsFiring = false;
        }
    }
}

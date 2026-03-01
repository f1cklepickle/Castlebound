using UnityEngine;
using UnityEngine.EventSystems;

namespace Castlebound.Gameplay.Input
{
    public class TouchMovementZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Vector2 MoveVector { get; private set; }
        public Vector2 AnchorPosition { get; private set; }

        // ── Runtime pointer events ────────────────────────────────────────────

        public void OnPointerDown(PointerEventData eventData)
        {
            SimulatePointerDown(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SimulateDrag(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SimulatePointerUp();
        }

        // ── Testable simulation API ───────────────────────────────────────────

        public void SimulatePointerDown(Vector2 screenPosition)
        {
            AnchorPosition = screenPosition;
        }

        public void SimulateDrag(Vector2 screenPosition)
        {
            var delta = screenPosition - AnchorPosition;
            MoveVector = Vector2.ClampMagnitude(delta.normalized, 1f);
        }

        public void SimulatePointerUp()
        {
            MoveVector = Vector2.zero;
            AnchorPosition = Vector2.zero;
        }
    }
}

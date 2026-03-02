using UnityEngine;
using UnityEngine.EventSystems;

namespace Castlebound.Gameplay.Input
{
    public class TouchMovementZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Tooltip("How far (in screen pixels) a finger must drag from the anchor to reach full speed. " +
                 "Drag distance is mapped proportionally, so half this radius = half speed.")]
        [SerializeField] private float maxRadius = 75f;

        public Vector2 MoveVector { get; private set; }
        public Vector2 AnchorPosition { get; private set; }

        /// <summary>Exposes <see cref="maxRadius"/> so tests can override it without the inspector.</summary>
        public float MaxRadius
        {
            get => maxRadius;
            set => maxRadius = value;
        }

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
            // Divide by maxRadius so the vector grows from 0 → 1 proportionally
            // as the finger moves from the anchor out to maxRadius pixels away.
            // ClampMagnitude caps at 1 so dragging further than maxRadius still
            // yields full speed rather than a value > 1.
            // (The old delta.normalized was always magnitude 1 — fully digital.)
            MoveVector = Vector2.ClampMagnitude(delta / maxRadius, 1f);
        }

        public void SimulatePointerUp()
        {
            MoveVector = Vector2.zero;
            AnchorPosition = Vector2.zero;
        }
    }
}

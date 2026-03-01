using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Castlebound.Gameplay.Input
{
    /// <summary>
    /// A lightweight click handler that only fires for touch input.
    /// With InputSystemUIInputModule (new Input System), pointer events carry
    /// an ExtendedPointerEventData whose pointerType distinguishes touch from
    /// mouse/pen — pointerId alone is unreliable in this module.
    /// Falls back to pointerId &lt; 0 for legacy StandaloneInputModule scenes.
    /// </summary>
    public class TouchOnlyButton : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick = new UnityEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsTouch(eventData)) return;
            onClick.Invoke();
        }

        private static bool IsTouch(PointerEventData eventData)
        {
            // InputSystemUIInputModule wraps data in ExtendedPointerEventData.
            if (eventData is ExtendedPointerEventData extended)
                return extended.pointerType == UIPointerType.Touch;

            // Legacy StandaloneInputModule: mouse buttons use negative IDs.
            return eventData.pointerId >= 0;
        }
    }
}

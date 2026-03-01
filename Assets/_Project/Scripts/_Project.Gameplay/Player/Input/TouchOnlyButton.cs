using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Castlebound.Gameplay.Input
{
    /// <summary>
    /// A lightweight click handler that only fires for touch input.
    /// Pointer IDs >= 0 are touch fingers; negative IDs are mouse buttons.
    /// Use this instead of UnityEngine.UI.Button on HUD elements that must
    /// remain inert to mouse clicks on PC.
    /// </summary>
    public class TouchOnlyButton : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClick = new UnityEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            // Mouse buttons report pointerId < 0; touch fingers report >= 0.
            if (eventData.pointerId < 0) return;
            onClick.Invoke();
        }
    }
}

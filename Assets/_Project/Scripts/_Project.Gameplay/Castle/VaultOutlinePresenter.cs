using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class VaultOutlinePresenter : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer outlineRenderer;
        [SerializeField] private Color accessibleColor = Color.white;
        [SerializeField] private Color blockedColor = Color.red;

        public void SetOutlineRenderer(SpriteRenderer renderer)
        {
            outlineRenderer = renderer;
        }

        public void Apply(VaultInteractionVisualState state)
        {
            if (outlineRenderer == null)
            {
                return;
            }

            bool visible = state != VaultInteractionVisualState.Hidden;
            Color color = state == VaultInteractionVisualState.Blocked ? blockedColor : accessibleColor;
            outlineRenderer.enabled = visible;
            outlineRenderer.color = color;
        }

        public SpriteRenderer[] GetEdgeRenderersForTests()
        {
            return outlineRenderer != null
                ? new[] { outlineRenderer }
                : new SpriteRenderer[0];
        }
    }
}

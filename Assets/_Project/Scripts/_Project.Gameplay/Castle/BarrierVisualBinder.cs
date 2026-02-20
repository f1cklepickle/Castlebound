using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierVisualBinder : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Sprite northSprite;
        [SerializeField] private Sprite eastSprite;
        [SerializeField] private Sprite southSprite;
        [SerializeField] private Sprite westSprite;

        public void ApplySide(BarrierSide side)
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            if (targetRenderer == null)
            {
                return;
            }

            targetRenderer.sprite = side switch
            {
                BarrierSide.North => northSprite,
                BarrierSide.East => eastSprite,
                BarrierSide.South => southSprite,
                BarrierSide.West => westSprite,
                _ => targetRenderer.sprite
            };
        }
    }
}

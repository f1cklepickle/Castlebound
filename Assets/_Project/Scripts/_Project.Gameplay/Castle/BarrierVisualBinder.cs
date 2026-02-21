using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierVisualBinder : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Transform systemsRoot;
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

            if (systemsRoot != null)
            {
                systemsRoot.localRotation = Quaternion.Euler(0f, 0f, GetSideRotation(side));
            }
        }

        private static float GetSideRotation(BarrierSide side)
        {
            switch (side)
            {
                case BarrierSide.East:
                    return -90f;
                case BarrierSide.South:
                    return 180f;
                case BarrierSide.West:
                    return 90f;
                default:
                    return 0f;
            }
        }
    }
}

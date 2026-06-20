using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierVisualBinder : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer groundRenderer;
        [SerializeField] private SpriteRenderer gateRenderer;
        [SerializeField] private SpriteRenderer wallRenderer;
        [SerializeField] private SpriteRenderer archRenderer;
        [SerializeField] private Transform systemsRoot;
        [SerializeField] private BarrierDirectionalSpriteSet groundSprites = new BarrierDirectionalSpriteSet();
        [SerializeField] private BarrierDirectionalSpriteSet gateSprites = new BarrierDirectionalSpriteSet();
        [SerializeField] private BarrierDirectionalSpriteSet wallSprites = new BarrierDirectionalSpriteSet();
        [SerializeField] private BarrierDirectionalSpriteSet archSprites = new BarrierDirectionalSpriteSet();

        public SpriteRenderer GateRenderer => gateRenderer;

        public void ApplySide(BarrierSide side)
        {
            ApplySprite(groundRenderer, groundSprites, side);
            ApplySprite(gateRenderer, gateSprites, side);
            ApplySprite(wallRenderer, wallSprites, side);
            ApplySprite(archRenderer, archSprites, side);

            if (systemsRoot != null)
            {
                systemsRoot.localRotation = Quaternion.Euler(0f, 0f, GetSideRotation(side));
            }
        }

        private static void ApplySprite(SpriteRenderer target, BarrierDirectionalSpriteSet sprites, BarrierSide side)
        {
            if (target == null || sprites == null)
            {
                return;
            }

            var sprite = sprites.GetSprite(side);
            if (sprite != null)
            {
                target.sprite = sprite;
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

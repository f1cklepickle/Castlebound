using UnityEngine;

namespace Castlebound.Gameplay.World.Placement
{
    public class BearTrapVisualState : MonoBehaviour
    {
        private static readonly int CloseTrigger = Animator.StringToHash("Close");

        [SerializeField] private SpriteRenderer visualRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Animator animator;
        [SerializeField] private string closeTriggerName = "Close";

        public Sprite OpenSprite
        {
            get => openSprite;
            set => openSprite = value;
        }

        public Sprite ClosedSprite
        {
            get => closedSprite;
            set => closedSprite = value;
        }

        private void Awake()
        {
            if (visualRenderer == null)
            {
                visualRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            ApplyOpenVisual();
        }

        public void ApplyOpenVisual()
        {
            if (visualRenderer != null && openSprite != null)
            {
                visualRenderer.sprite = openSprite;
            }
        }

        public void ApplyClosedVisual()
        {
            if (animator != null)
            {
                animator.SetTrigger(string.IsNullOrWhiteSpace(closeTriggerName) ? CloseTrigger : Animator.StringToHash(closeTriggerName));
                return;
            }

            if (visualRenderer != null && closedSprite != null)
            {
                visualRenderer.sprite = closedSprite;
            }
        }
    }
}

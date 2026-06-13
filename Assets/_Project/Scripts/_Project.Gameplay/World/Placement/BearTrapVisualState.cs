using System.Collections;
using UnityEngine;

namespace Castlebound.Gameplay.World.Placement
{
    public class BearTrapVisualState : MonoBehaviour
    {
        private static readonly int CloseTrigger = Animator.StringToHash("Close");

        [SerializeField] private SpriteRenderer visualRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite[] closeAnimationFrames;
        [SerializeField] private float closeFrameSeconds = 0.08f;
        [SerializeField] private Animator animator;
        [SerializeField] private string closeTriggerName = "Close";

        private Coroutine closeAnimationRoutine;

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

        public int CloseAnimationFrameCount => closeAnimationFrames != null ? closeAnimationFrames.Length : 0;

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
            StopCloseAnimation();

            if (visualRenderer != null && openSprite != null)
            {
                visualRenderer.sprite = openSprite;
            }
        }

        public void ApplyClosedVisual()
        {
            StopCloseAnimation();

            if (closeAnimationFrames != null && closeAnimationFrames.Length > 0)
            {
                if (isActiveAndEnabled)
                {
                    closeAnimationRoutine = StartCoroutine(PlayCloseAnimation());
                }
                else
                {
                    ApplyFinalClosedFrame();
                }

                return;
            }

            if (animator != null)
            {
                animator.SetTrigger(string.IsNullOrWhiteSpace(closeTriggerName) ? CloseTrigger : Animator.StringToHash(closeTriggerName));
                return;
            }

            ApplyFinalClosedFrame();
        }

        private IEnumerator PlayCloseAnimation()
        {
            for (int i = 0; i < closeAnimationFrames.Length; i++)
            {
                if (visualRenderer != null && closeAnimationFrames[i] != null)
                {
                    visualRenderer.sprite = closeAnimationFrames[i];
                }

                yield return new WaitForSeconds(Mathf.Max(0.01f, closeFrameSeconds));
            }

            ApplyFinalClosedFrame();
            closeAnimationRoutine = null;
        }

        private void ApplyFinalClosedFrame()
        {
            if (visualRenderer == null)
            {
                return;
            }

            if (closedSprite != null)
            {
                visualRenderer.sprite = closedSprite;
                return;
            }

            if (closeAnimationFrames != null && closeAnimationFrames.Length > 0)
            {
                var finalFrame = closeAnimationFrames[closeAnimationFrames.Length - 1];
                if (finalFrame != null)
                {
                    visualRenderer.sprite = finalFrame;
                }
            }
        }

        private void StopCloseAnimation()
        {
            if (closeAnimationRoutine == null)
            {
                return;
            }

            StopCoroutine(closeAnimationRoutine);
            closeAnimationRoutine = null;
        }
    }
}

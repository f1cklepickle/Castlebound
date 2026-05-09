using UnityEngine;

namespace Castlebound.Gameplay.Combat
{
    public class WeaponFireAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] fireFrames;
        [SerializeField] private float baseDurationSeconds = 0.25f;
        [SerializeField] private float referenceCooldownSeconds = 1f;
        [SerializeField] private float minPlaybackSpeed = 0.5f;
        [SerializeField] private float maxPlaybackSpeed = 3f;

        private int frameIndex;
        private float frameTimer;
        private float frameDuration;
        private bool playing;

        public Sprite[] FireFrames
        {
            get => fireFrames;
            set => fireFrames = value;
        }

        public float BaseDurationSeconds
        {
            get => baseDurationSeconds;
            set => baseDurationSeconds = Mathf.Max(0.01f, value);
        }

        public float ReferenceCooldownSeconds
        {
            get => referenceCooldownSeconds;
            set => referenceCooldownSeconds = Mathf.Max(0.01f, value);
        }

        public float MinPlaybackSpeed
        {
            get => minPlaybackSpeed;
            set => minPlaybackSpeed = Mathf.Max(0.01f, value);
        }

        public float MaxPlaybackSpeed
        {
            get => maxPlaybackSpeed;
            set => maxPlaybackSpeed = Mathf.Max(MinPlaybackSpeed, value);
        }

        private void Reset()
        {
            EnsureReferences();
            CaptureIdleSprite();
            NormalizeTuning();
        }

        private void Awake()
        {
            EnsureReferences();
            CaptureIdleSprite();
            NormalizeTuning();
        }

        private void OnValidate()
        {
            NormalizeTuning();
        }

        private void Update()
        {
            if (!playing || fireFrames == null || fireFrames.Length == 0)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            while (frameTimer >= frameDuration && playing)
            {
                frameTimer -= frameDuration;
                AdvanceFrame();
            }
        }

        public void Play(float cooldownSeconds)
        {
            EnsureReferences();
            CaptureIdleSprite();

            if (targetRenderer == null || fireFrames == null || fireFrames.Length == 0)
            {
                return;
            }

            frameIndex = 0;
            frameTimer = 0f;
            frameDuration = Mathf.Max(0.001f, baseDurationSeconds / fireFrames.Length / CalculatePlaybackSpeed(cooldownSeconds));
            playing = true;
            targetRenderer.sprite = fireFrames[frameIndex];
        }

        public float CalculatePlaybackSpeed(float cooldownSeconds)
        {
            NormalizeTuning();

            if (cooldownSeconds <= 0f)
            {
                return maxPlaybackSpeed;
            }

            return Mathf.Clamp(referenceCooldownSeconds / cooldownSeconds, minPlaybackSpeed, maxPlaybackSpeed);
        }

        private void AdvanceFrame()
        {
            frameIndex++;
            if (frameIndex >= fireFrames.Length)
            {
                playing = false;
                if (targetRenderer != null && idleSprite != null)
                {
                    targetRenderer.sprite = idleSprite;
                }

                return;
            }

            targetRenderer.sprite = fireFrames[frameIndex];
        }

        private void EnsureReferences()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void CaptureIdleSprite()
        {
            if (idleSprite == null && targetRenderer != null)
            {
                idleSprite = targetRenderer.sprite;
            }
        }

        private void NormalizeTuning()
        {
            baseDurationSeconds = Mathf.Max(0.01f, baseDurationSeconds);
            referenceCooldownSeconds = Mathf.Max(0.01f, referenceCooldownSeconds);
            minPlaybackSpeed = Mathf.Max(0.01f, minPlaybackSpeed);
            maxPlaybackSpeed = Mathf.Max(minPlaybackSpeed, maxPlaybackSpeed);
        }
    }
}

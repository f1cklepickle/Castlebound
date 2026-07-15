using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemySpriteSheetLoop : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Texture2D spriteSheet;
        [SerializeField] private int frameWidth = 64;
        [SerializeField] private int frameHeight = 64;
        [SerializeField] private int frameCount = 1;
        [SerializeField] private float framesPerSecond = 4f;
        [SerializeField] private float pixelsPerUnit = 32f;

        private Sprite[] frames;
        private int currentFrameIndex;
        private float frameTimer;

        public Texture2D SpriteSheet => spriteSheet;
        public int FrameWidth => frameWidth;
        public int FrameHeight => frameHeight;
        public int FrameCount => frameCount;
        public float FramesPerSecond => framesPerSecond;
        public int CurrentFrameIndex => currentFrameIndex;

        private void Awake()
        {
            EnsureRenderer();
            BuildFrames();
            ApplyFrame(0);
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        public void Configure(Texture2D sheet, int width, int height, int count, float fps, float ppu = 32f)
        {
            spriteSheet = sheet;
            frameWidth = Mathf.Max(1, width);
            frameHeight = Mathf.Max(1, height);
            frameCount = Mathf.Max(1, count);
            framesPerSecond = Mathf.Max(0f, fps);
            pixelsPerUnit = Mathf.Max(1f, ppu);

            EnsureRenderer();
            BuildFrames();
            ApplyFrame(0);
        }

        public void Advance(float deltaTime)
        {
            if (frames == null || frames.Length <= 1 || framesPerSecond <= 0f)
            {
                return;
            }

            frameTimer += Mathf.Max(0f, deltaTime);
            float frameDuration = 1f / framesPerSecond;
            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                ApplyFrame((currentFrameIndex + 1) % frames.Length);
            }
        }

        private void EnsureRenderer()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void BuildFrames()
        {
            if (spriteSheet == null)
            {
                frames = null;
                return;
            }

            int safeFrameWidth = Mathf.Max(1, frameWidth);
            int safeFrameHeight = Mathf.Max(1, frameHeight);
            int columns = Mathf.Max(1, spriteSheet.width / safeFrameWidth);
            int rows = Mathf.Max(1, spriteSheet.height / safeFrameHeight);
            int maxFrames = Mathf.Max(1, columns * rows);
            int safeFrameCount = Mathf.Clamp(frameCount, 1, maxFrames);

            frames = new Sprite[safeFrameCount];
            for (int i = 0; i < safeFrameCount; i++)
            {
                int column = i % columns;
                int rowFromTop = i / columns;
                float x = column * safeFrameWidth;
                float y = spriteSheet.height - safeFrameHeight - rowFromTop * safeFrameHeight;

                var sprite = Sprite.Create(
                    spriteSheet,
                    new Rect(x, y, safeFrameWidth, safeFrameHeight),
                    new Vector2(0.5f, 0.5f),
                    pixelsPerUnit);
                sprite.name = $"{spriteSheet.name}_{i}";
                frames[i] = sprite;
            }
        }

        private void ApplyFrame(int index)
        {
            currentFrameIndex = Mathf.Max(0, index);
            if (targetRenderer != null && frames != null && frames.Length > 0)
            {
                targetRenderer.sprite = frames[currentFrameIndex % frames.Length];
            }
        }
    }
}

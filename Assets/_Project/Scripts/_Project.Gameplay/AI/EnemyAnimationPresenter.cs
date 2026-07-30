using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public class EnemyAnimationPresenter : MonoBehaviour
    {
        public enum PresentationState
        {
            Idle,
            Walk,
            Attack,
            Hold
        }

        [Header("References")]
        [SerializeField] private SpriteRenderer targetRenderer;

        [Header("Sprite Sheets")]
        [SerializeField] private Texture2D idleSheet;
        [SerializeField] private Texture2D walkSheet;
        [SerializeField] private Texture2D attackSheet;

        [Header("Frames")]
        [SerializeField, Min(1)] private int idleFrameCount = 9;
        [SerializeField, Min(1)] private int walkFrameCount = 6;
        [SerializeField, Min(1)] private int attackFrameCount = 7;
        [SerializeField, Min(0)] private int attackImpactFrameIndex = 6;
        [SerializeField, Min(1)] private int frameWidth = 64;
        [SerializeField, Min(1)] private int frameHeight = 64;
        [SerializeField, Min(1f)] private float pixelsPerUnit = 32f;

        [Header("Presentation Timing")]
        [SerializeField, Min(0f)] private float idleFramesPerSecond = 6f;
        [SerializeField, Min(0f)] private float walkFramesPerSecond = 10f;
        [SerializeField] private EnemyAttackAnimationTiming attackTiming = new EnemyAttackAnimationTiming();
        [SerializeField, Min(0f)] private float idleDelaySeconds = 2f;

        private static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();

        private Sprite[] idleFrames;
        private Sprite[] walkFrames;
        private Sprite[] attackFrames;
        private PresentationState state;
        private float stateElapsedSeconds;
        private float inactiveSeconds;
        private float attackWindupSeconds;
        private bool movementRequested;

        public Texture2D IdleSheet => idleSheet;
        public Texture2D WalkSheet => walkSheet;
        public Texture2D AttackSheet => attackSheet;
        public int IdleFrameCount => idleFrameCount;
        public int WalkFrameCount => walkFrameCount;
        public int AttackFrameCount => attackFrameCount;
        public EnemyAttackAnimationTiming AttackTiming => attackTiming;
        public float IdleDelaySeconds => idleDelaySeconds;
        public PresentationState CurrentState => state;

        private void Awake()
        {
            InitializePresentation();
        }

        public void InitializePresentation()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            if (attackTiming == null)
                attackTiming = new EnemyAttackAnimationTiming();
            idleFrames = GetOrCreateFrames(idleSheet, idleFrameCount);
            walkFrames = GetOrCreateFrames(walkSheet, walkFrameCount);
            attackFrames = GetOrCreateFrames(attackSheet, attackFrameCount);
            state = PresentationState.Hold;
            ApplyFrame(idleFrames, 0);
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        public void SetMovementRequested(bool isMoving)
        {
            movementRequested = isMoving;
            if (isMoving)
                inactiveSeconds = 0f;
        }

        public void PlayAttack(float authoritativeWindupSeconds)
        {
            attackWindupSeconds = Mathf.Max(0f, authoritativeWindupSeconds);
            state = PresentationState.Attack;
            stateElapsedSeconds = 0f;
            inactiveSeconds = 0f;
            ApplyFrame(attackFrames, 0);
        }

        public void CancelAttack()
        {
            state = PresentationState.Hold;
            stateElapsedSeconds = 0f;
            inactiveSeconds = 0f;
            ApplyFrame(idleFrames, 0);
        }

        public void Advance(float deltaTime)
        {
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            stateElapsedSeconds += safeDeltaTime;

            if (state == PresentationState.Attack)
            {
                int attackFrame = attackTiming.ResolveFrame(
                    stateElapsedSeconds,
                    attackWindupSeconds,
                    attackFrameCount,
                    attackImpactFrameIndex);
                ApplyFrame(attackFrames, attackFrame);

                if (!attackTiming.IsComplete(stateElapsedSeconds, attackWindupSeconds))
                    return;

                state = PresentationState.Hold;
                stateElapsedSeconds = 0f;
                inactiveSeconds = 0f;
                if (!movementRequested)
                {
                    ApplyFrame(idleFrames, 0);
                    return;
                }
            }

            if (movementRequested)
            {
                if (state != PresentationState.Walk)
                    stateElapsedSeconds = 0f;
                state = PresentationState.Walk;
                ApplyLoopingFrame(walkFrames, walkFramesPerSecond);
                return;
            }

            inactiveSeconds += safeDeltaTime;
            if (inactiveSeconds < idleDelaySeconds)
            {
                state = PresentationState.Hold;
                return;
            }

            if (state != PresentationState.Idle)
                stateElapsedSeconds = 0f;
            state = PresentationState.Idle;
            ApplyLoopingFrame(idleFrames, idleFramesPerSecond);
        }

        private void ApplyLoopingFrame(Sprite[] frames, float framesPerSecond)
        {
            int frameIndex = framesPerSecond <= 0f || frames == null || frames.Length == 0
                ? 0
                : Mathf.FloorToInt(stateElapsedSeconds * framesPerSecond) % frames.Length;
            ApplyFrame(frames, frameIndex);
        }

        private Sprite[] GetOrCreateFrames(Texture2D sheet, int requestedFrameCount)
        {
            if (sheet == null)
                return null;

            int safeFrameCount = Mathf.Max(1, requestedFrameCount);
            string cacheKey = $"{sheet.GetInstanceID()}:{frameWidth}:{frameHeight}:{safeFrameCount}:{pixelsPerUnit}";
            if (FrameCache.TryGetValue(cacheKey, out Sprite[] cachedFrames))
                return cachedFrames;

            int columns = Mathf.Max(1, sheet.width / frameWidth);
            int rows = Mathf.Max(1, sheet.height / frameHeight);
            int frameCount = Mathf.Clamp(safeFrameCount, 1, columns * rows);
            var frames = new Sprite[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                int column = i % columns;
                int rowFromTop = i / columns;
                var rect = new Rect(
                    column * frameWidth,
                    sheet.height - frameHeight - rowFromTop * frameHeight,
                    frameWidth,
                    frameHeight);
                frames[i] = Sprite.Create(sheet, rect, new Vector2(0.5f, 0f), pixelsPerUnit);
                frames[i].name = $"{sheet.name}_{i}";
            }

            FrameCache[cacheKey] = frames;
            return frames;
        }

        private void ApplyFrame(Sprite[] frames, int frameIndex)
        {
            if (targetRenderer == null || frames == null || frames.Length == 0)
                return;

            targetRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }
    }
}

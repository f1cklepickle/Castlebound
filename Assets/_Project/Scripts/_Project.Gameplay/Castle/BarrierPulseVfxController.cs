using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public class BarrierPulseVfxController : MonoBehaviour
    {
        [SerializeField] private BarrierPulseEmitter pulseEmitter;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private LineRenderer ringRenderer;
        [SerializeField] private Sprite ringStripSprite;
        [SerializeField] private bool autoCreateVisualComponents = true;
        [SerializeField] private int ringSortingOrder = 10;
        [SerializeField] private float ringAlpha = 0.7f;
        [SerializeField] private bool ringFlipbookEnabled = true;
        [SerializeField] private int ringFlipbookTilesX = 9;
        [SerializeField] private int ringFlipbookTilesY = 1;
        [SerializeField] private float ringFlipbookFps = 12f;
        [SerializeField] private bool ringSeamDriftEnabled = true;
        [SerializeField] private float ringSeamDriftSpeed = 0.75f;
        [SerializeField] private int minRingSegments = 12;
        [SerializeField] private int maxRingSegments = 64;
        [SerializeField] private float radiusPerSegment = 0.35f;
        [SerializeField] private float ringThicknessWorldUnits = 0.08f;
        [SerializeField] private float radiusUpdateEpsilon = 0.02f;

        private static readonly Dictionary<int, Vector2[]> UnitCircleCache = new Dictionary<int, Vector2[]>();

        private bool isConfigured;
        private float lastRenderedRadius = -1f;
        private int lastRenderedSegments = -1;
        private Vector3 lastRenderedCenter = new Vector3(float.NaN, float.NaN, float.NaN);
        private Material ringRuntimeMaterial;

        private void Awake()
        {
            BindIfNeeded();
            ConfigureRenderers();
            SetVisualEnabled(false);
        }

        private void OnEnable()
        {
            BindIfNeeded();
            ConfigureRenderers();
        }

        private void OnDestroy()
        {
            if (ringRuntimeMaterial != null)
            {
                Destroy(ringRuntimeMaterial);
            }

        }

        private void Update()
        {
            BindIfNeeded();
            ConfigureRenderers();

            if (pulseEmitter == null || ringRenderer == null)
            {
                return;
            }

            if (!pulseEmitter.IsPulseActive)
            {
                SetVisualEnabled(false);
                return;
            }

            SetVisualEnabled(true);
            float radius = Mathf.Max(0f, pulseEmitter.CurrentRadius);
            UpdateRing(radius);
            ApplyRingFlipbook(pulseEmitter.PulseProgress01, pulseEmitter.PulseDuration);
        }

        private void BindIfNeeded()
        {
            if (pulseEmitter == null)
            {
                pulseEmitter = GetComponent<BarrierPulseEmitter>();
                if (pulseEmitter == null)
                {
                    pulseEmitter = GetComponentInParent<BarrierPulseEmitter>();
                }
            }

            if (vfxRoot == null)
            {
                var existing = transform.Find("PulseVfx");
                if (existing != null)
                {
                    vfxRoot = existing;
                }
                else if (autoCreateVisualComponents)
                {
                    var root = new GameObject("PulseVfx");
                    vfxRoot = root.transform;
                    vfxRoot.SetParent(transform, false);
                    vfxRoot.localPosition = Vector3.zero;
                    vfxRoot.localRotation = Quaternion.identity;
                }
            }

            if (vfxRoot != null)
            {
                // Compensate parent scale so VFX radius behaves consistently in world space.
                vfxRoot.localScale = GetInverseScale(transform.lossyScale);
            }

            if (ringRenderer == null)
            {
                if (vfxRoot != null)
                {
                    ringRenderer = vfxRoot.GetComponent<LineRenderer>();
                }
                if (ringRenderer == null)
                {
                    ringRenderer = GetComponent<LineRenderer>();
                }
                if (ringRenderer == null)
                {
                    ringRenderer = GetComponentInChildren<LineRenderer>(true);
                }
                if (ringRenderer == null && autoCreateVisualComponents)
                {
                    var target = vfxRoot != null ? vfxRoot.gameObject : gameObject;
                    ringRenderer = target.AddComponent<LineRenderer>();
                }
                isConfigured = false;
            }

        }

        private void ConfigureRenderers()
        {
            if (isConfigured)
            {
                bool ringNeedsLateMaterialBind = ringRenderer != null
                    && ringRenderer.sharedMaterial == null
                    && ringStripSprite != null;
                if (!ringNeedsLateMaterialBind)
                {
                    return;
                }
            }

            if (ringRenderer != null)
            {
                ringRenderer.textureMode = LineTextureMode.Tile;
                ringRenderer.widthMultiplier = ringThicknessWorldUnits;
                ringRenderer.loop = true;
                ringRenderer.useWorldSpace = true;
                ringRenderer.alignment = LineAlignment.View;
                ringRenderer.numCornerVertices = 0;
                ringRenderer.numCapVertices = 0;
                ringRenderer.generateLightingData = false;
                ringRenderer.receiveShadows = false;
                ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ringRenderer.sortingOrder = ringSortingOrder;

                if (ringStripSprite != null)
                {
                    ringRuntimeMaterial = CreateSpriteMaterial(ringStripSprite, ringRuntimeMaterial);
                    if (ringRuntimeMaterial != null)
                    {
                        ringRenderer.sharedMaterial = ringRuntimeMaterial;
                    }
                }

                var configuredRingColor = Color.white;
                configuredRingColor.a = Mathf.Clamp01(ringAlpha);
                ringRenderer.startColor = configuredRingColor;
                ringRenderer.endColor = configuredRingColor;
            }

            isConfigured = true;
        }

        private void SetVisualEnabled(bool enabled)
        {
            if (ringRenderer != null)
            {
                ringRenderer.enabled = enabled;
            }
        }

        private void UpdateRing(float radius)
        {
            int segments = ComputeSegmentCount(radius);
            Vector3 center = pulseEmitter != null ? pulseEmitter.PulseOriginPosition : transform.position;

            bool shouldSkip = Mathf.Abs(radius - lastRenderedRadius) <= radiusUpdateEpsilon
                && segments == lastRenderedSegments
                && center == lastRenderedCenter;
            if (shouldSkip)
            {
                return;
            }

            ringRenderer.positionCount = segments;
            Vector2[] unitCircle = GetUnitCircle(segments);

            for (int i = 0; i < segments; i++)
            {
                float x = unitCircle[i].x * radius;
                float y = unitCircle[i].y * radius;
                ringRenderer.SetPosition(i, center + new Vector3(x, y, 0f));
            }

            lastRenderedRadius = radius;
            lastRenderedSegments = segments;
            lastRenderedCenter = center;
        }

        private void ApplyRingFlipbook(float pulseProgress01, float pulseDuration)
        {
            if (!ringFlipbookEnabled || ringRenderer == null || ringRenderer.sharedMaterial == null)
            {
                return;
            }

            int tilesX = Mathf.Max(1, ringFlipbookTilesX);
            int tilesY = Mathf.Max(1, ringFlipbookTilesY);
            int frameCount = tilesX * tilesY;
            if (frameCount <= 1)
            {
                return;
            }

            float duration = Mathf.Max(0.0001f, pulseDuration);
            float elapsed = Mathf.Clamp01(pulseProgress01) * duration;
            int frameIndex = Mathf.FloorToInt(elapsed * Mathf.Max(0f, ringFlipbookFps));
            frameIndex = Mod(frameIndex, frameCount);

            int x = frameIndex % tilesX;
            int y = frameIndex / tilesX;

            float scaleX = 1f / tilesX;
            float scaleY = 1f / tilesY;
            float offsetX = x * scaleX;
            float offsetY = 1f - ((y + 1) * scaleY);
            if (ringSeamDriftEnabled)
            {
                offsetX += elapsed * Mathf.Max(0f, ringSeamDriftSpeed);
                offsetX = offsetX - Mathf.Floor(offsetX);
            }

            var mat = ringRenderer.sharedMaterial;
            mat.mainTextureScale = new Vector2(scaleX, scaleY);
            mat.mainTextureOffset = new Vector2(offsetX, offsetY);
        }

        private int ComputeSegmentCount(float radius)
        {
            int minSeg = Mathf.Clamp(minRingSegments, 8, 256);
            int maxSeg = Mathf.Clamp(maxRingSegments, minSeg, 256);
            float segmentStep = Mathf.Max(0.05f, radiusPerSegment);
            int dynamicSeg = minSeg + Mathf.CeilToInt(radius / segmentStep);
            return Mathf.Clamp(dynamicSeg, minSeg, maxSeg);
        }

        private static Vector2[] GetUnitCircle(int segments)
        {
            if (UnitCircleCache.TryGetValue(segments, out var points))
            {
                return points;
            }

            points = new Vector2[segments];
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                float angle = t * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            UnitCircleCache[segments] = points;
            return points;
        }

        private static Material CreateSpriteMaterial(Sprite sprite, Material previous)
        {
            if (sprite == null)
            {
                return previous;
            }

            if (previous != null && previous.mainTexture == sprite.texture)
            {
                return previous;
            }

            if (previous != null)
            {
                Destroy(previous);
            }

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            var material = new Material(shader);
            material.mainTexture = sprite.texture;
            return material;
        }

        private static Vector3 GetInverseScale(Vector3 lossy)
        {
            return new Vector3(
                SafeInverse(lossy.x),
                SafeInverse(lossy.y),
                SafeInverse(lossy.z));
        }

        private static float SafeInverse(float value)
        {
            return Mathf.Abs(value) < 0.0001f ? 1f : 1f / value;
        }

        private static int Mod(int value, int divisor)
        {
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }
    }
}

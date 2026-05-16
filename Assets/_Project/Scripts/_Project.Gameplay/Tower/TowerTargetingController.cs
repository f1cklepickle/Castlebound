using UnityEngine;
using Castlebound.Gameplay.AI;

namespace Castlebound.Gameplay.Tower
{
    public class TowerTargetingController : MonoBehaviour
    {
        private const int MinTargetBufferSize = 1;

        [SerializeField] private TowerTargetingProfile targetingProfile;
        [SerializeField] private float minRange;
        [SerializeField] private float maxRange = 5f;
        [SerializeField] private int targetBufferSize = 16;

        private Collider2D[] targetBuffer;
        private float nextScanTime;

        public TowerTargetingProfile TargetingProfile
        {
            get => targetingProfile;
            set => targetingProfile = value;
        }

        public float MinRange
        {
            get => minRange;
            set
            {
                minRange = Mathf.Max(0f, value);
                maxRange = Mathf.Max(minRange, maxRange);
            }
        }

        public float MaxRange
        {
            get => maxRange;
            set => maxRange = Mathf.Max(minRange, value);
        }

        public Transform CurrentTarget { get; private set; }

        public int TargetBufferSize
        {
            get => targetBufferSize;
            set
            {
                targetBufferSize = Mathf.Max(MinTargetBufferSize, value);
                EnsureTargetBuffer();
            }
        }

        private void Awake()
        {
            NormalizeRanges();
            EnsureTargetBuffer();
        }

        private void OnValidate()
        {
            NormalizeRanges();
            targetBufferSize = Mathf.Max(MinTargetBufferSize, targetBufferSize);
        }

        private void Update()
        {
            if (targetingProfile == null || Time.time < nextScanTime)
            {
                return;
            }

            AcquireTargetNow();
            nextScanTime = Time.time + targetingProfile.ScanInterval;
        }

        public Transform AcquireTargetNow()
        {
            EnsureTargetBuffer();
            CurrentTarget = null;

            if (targetingProfile == null || maxRange <= 0f)
            {
                return null;
            }

            var origin = (Vector2)transform.position;
            var hitCount = Physics2D.OverlapCircleNonAlloc(
                origin,
                maxRange,
                targetBuffer,
                targetingProfile.TargetLayers);

            var minRangeSqr = minRange * minRange;
            var maxRangeSqr = maxRange * maxRange;
            var bestScore = targetingProfile.SelectionMode == TowerTargetSelectionMode.Nearest
                ? float.PositiveInfinity
                : float.NegativeInfinity;

            for (var i = 0; i < hitCount; i++)
            {
                var candidateCollider = targetBuffer[i];
                targetBuffer[i] = null;

                if (!IsValidCandidate(candidateCollider, origin, minRangeSqr, maxRangeSqr, out var distanceSqr))
                {
                    continue;
                }

                if (IsBetterCandidate(distanceSqr, bestScore))
                {
                    bestScore = distanceSqr;
                    CurrentTarget = candidateCollider.transform;
                }
            }

            return CurrentTarget;
        }

        private bool IsValidCandidate(
            Collider2D candidateCollider,
            Vector2 origin,
            float minRangeSqr,
            float maxRangeSqr,
            out float distanceSqr)
        {
            distanceSqr = 0f;

            if (candidateCollider == null || !candidateCollider.enabled || !candidateCollider.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (candidateCollider.transform == transform || candidateCollider.transform.IsChildOf(transform))
            {
                return false;
            }

            var regionState = candidateCollider.GetComponentInParent<EnemyRegionState>();
            if (regionState != null && regionState.EnemyInside)
            {
                return false;
            }

            distanceSqr = ((Vector2)candidateCollider.transform.position - origin).sqrMagnitude;
            return distanceSqr >= minRangeSqr && distanceSqr <= maxRangeSqr;
        }

        private bool IsBetterCandidate(float distanceSqr, float bestScore)
        {
            if (targetingProfile.SelectionMode == TowerTargetSelectionMode.Farthest)
            {
                return distanceSqr > bestScore;
            }

            return distanceSqr < bestScore;
        }

        private void EnsureTargetBuffer()
        {
            targetBufferSize = Mathf.Max(MinTargetBufferSize, targetBufferSize);

            if (targetBuffer == null || targetBuffer.Length != targetBufferSize)
            {
                targetBuffer = new Collider2D[targetBufferSize];
            }
        }

        private void NormalizeRanges()
        {
            minRange = Mathf.Max(0f, minRange);
            maxRange = Mathf.Max(minRange, maxRange);
        }
    }
}

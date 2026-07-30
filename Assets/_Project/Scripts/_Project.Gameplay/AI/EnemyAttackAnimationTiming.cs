using System;
using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    [Serializable]
    public sealed class EnemyAttackAnimationTiming
    {
        private static readonly float[] DefaultWindupFrameNormalizedTimes =
            { 0f, 0.07f, 0.2f, 0.37f, 0.57f, 0.8f, 1f };

        [SerializeField] private float[] windupFrameNormalizedTimes =
            { 0f, 0.07f, 0.2f, 0.37f, 0.57f, 0.8f, 1f };
        [SerializeField, Min(0f)] private float impactHoldWindupRatio = 0.2f;
        [SerializeField] private float[] recoveryFrameNormalizedDelays = Array.Empty<float>();
        [SerializeField, Min(0f)] private float finalFrameHoldWindupRatio;

        public float ImpactHoldWindupRatio => impactHoldWindupRatio;
        public int WindupFrameCount => LengthOf(windupFrameNormalizedTimes);

        public int ResolveFrame(
            float elapsedSeconds,
            float authoritativeWindupSeconds,
            int frameCount,
            int impactFrameIndex)
        {
            int safeFrameCount = Mathf.Max(1, frameCount);
            int safeImpactFrame = Mathf.Clamp(impactFrameIndex, 0, safeFrameCount - 1);
            float safeElapsed = Mathf.Max(0f, elapsedSeconds);
            float safeWindup = Mathf.Max(0f, authoritativeWindupSeconds);

            if (safeElapsed < safeWindup && safeWindup > 0f)
            {
                float progress = safeElapsed / safeWindup;
                int windupFrame = 0;
                float[] windupTimes = HasUsableWindupTimeline(windupFrameNormalizedTimes, safeImpactFrame)
                    ? windupFrameNormalizedTimes
                    : DefaultWindupFrameNormalizedTimes;
                int authoredWindupFrames = Mathf.Min(safeImpactFrame + 1, LengthOf(windupTimes));
                for (int i = 1; i < authoredWindupFrames; i++)
                {
                    if (progress < Mathf.Clamp01(windupTimes[i]))
                        break;
                    windupFrame = i;
                }
                return Mathf.Clamp(windupFrame, 0, safeImpactFrame);
            }

            float afterImpactSeconds = safeElapsed - safeWindup;
            float normalizedAfterImpact = safeWindup > 0f ? afterImpactSeconds / safeWindup : float.PositiveInfinity;
            if (normalizedAfterImpact < Mathf.Max(0f, impactHoldWindupRatio))
                return safeImpactFrame;

            int recoveryFrameCount = safeFrameCount - safeImpactFrame - 1;
            int authoredRecoveryFrames = Mathf.Min(recoveryFrameCount, LengthOf(recoveryFrameNormalizedDelays));
            int recoveryFrame = safeImpactFrame;
            for (int i = 0; i < authoredRecoveryFrames; i++)
            {
                if (normalizedAfterImpact < Mathf.Max(impactHoldWindupRatio, recoveryFrameNormalizedDelays[i]))
                    break;
                recoveryFrame = safeImpactFrame + i + 1;
            }

            return Mathf.Clamp(recoveryFrame, safeImpactFrame, safeFrameCount - 1);
        }

        public bool IsComplete(float elapsedSeconds, float authoritativeWindupSeconds)
        {
            float safeWindup = Mathf.Max(0f, authoritativeWindupSeconds);
            float finalRecoveryRatio = LengthOf(recoveryFrameNormalizedDelays) > 0
                ? Mathf.Max(impactHoldWindupRatio, recoveryFrameNormalizedDelays[recoveryFrameNormalizedDelays.Length - 1])
                : Mathf.Max(0f, impactHoldWindupRatio);
            return elapsedSeconds >= safeWindup
                * (1f + finalRecoveryRatio + Mathf.Max(0f, finalFrameHoldWindupRatio));
        }

        private static int LengthOf(float[] values)
        {
            return values != null ? values.Length : 0;
        }

        private static bool HasUsableWindupTimeline(float[] values, int impactFrameIndex)
        {
            if (LengthOf(values) < impactFrameIndex + 1 || values[0] > 0f || values[impactFrameIndex] < 1f)
                return false;

            for (int i = 1; i <= impactFrameIndex; i++)
            {
                if (values[i] < values[i - 1])
                    return false;
            }

            return true;
        }
    }
}

using UnityEngine;
using Castlebound.Gameplay.AI;

public static class EnemyMovement
{
    public static void ComputeMovement(
        Vector2 position,
        Transform steerTarget,
        Transform barrier,
        float surfaceDistance,
        float engagementDistance,
        float releaseMargin,
        float reseatBias,
        float speed,
        float orbitBase,
        float maxTangent,
        int outrunFrames,
        float epsilonDist,
        float gapCW,
        float gapCCW,
        ref EnemyController2D.State state,
        ref float prevDist,
        ref int distTrend,
        ref Vector2 lastNonZeroDir,
        out Vector2 radial,
        out Vector2 tangent)
    {
        radial = Vector2.zero;
        tangent = Vector2.zero;

        if (steerTarget == null)
            return;

        Vector2 toTarget = (Vector2)steerTarget.position - position;
        float dist = toTarget.magnitude;
        Vector2 dir = dist > 1e-6f ? (toTarget / dist) : Vector2.zero;

        if (dir != Vector2.zero)
        {
            lastNonZeroDir = dir;
        }

        if (surfaceDistance > prevDist + epsilonDist)
            distTrend++;
        else if (surfaceDistance < prevDist - epsilonDist)
            distTrend = 0;

        prevDist = surfaceDistance;

        bool isBarrierTarget = (barrier != null && steerTarget == barrier);
        bool barrierBroken = false;

        if (isBarrierTarget)
        {
            var barrierHealth = barrier.GetComponent<BarrierHealth>();
            if (barrierHealth != null)
            {
                barrierBroken = barrierHealth.IsBroken;
            }
        }

        bool currentlyHolding = state == EnemyController2D.State.HOLD;
        bool shouldHold = EnemyEngagement.ShouldHold(
            currentlyHolding,
            surfaceDistance,
            engagementDistance,
            releaseMargin,
            isBarrierTarget && barrierBroken);

        if (!isBarrierTarget && currentlyHolding && distTrend >= outrunFrames)
        {
            shouldHold = false;
        }

        state = shouldHold ? EnemyController2D.State.HOLD : EnemyController2D.State.CHASE;

        if (state == EnemyController2D.State.CHASE)
        {
            radial = dir * speed;
        }
        else
        {
            if (isBarrierTarget)
            {
                if (surfaceDistance > engagementDistance)
                {
                    radial = dir * (reseatBias * speed);
                }
                tangent = Vector2.zero;
            }
            else
            {
                if (surfaceDistance > engagementDistance)
                    radial = dir * (reseatBias * speed);

                float preference = gapCCW - gapCW;
                bool hasNoGaps = (gapCW == 0f && gapCCW == 0f);

                float sign = 0f;

                if (!hasNoGaps)
                {
                    if (preference > 0f)
                        sign = 1f;
                    else if (preference < 0f)
                        sign = -1f;
                }

                float tangentMag = 0f;

                if (dist > 0f && !hasNoGaps && sign != 0f)
                {
                    float orbitDist = Mathf.Max(dist, engagementDistance);
                    tangentMag = orbitBase * (speed * orbitDist / Mathf.Max(engagementDistance, 0.01f));
                    tangentMag = Mathf.Min(tangentMag, maxTangent);
                }

                if (dir != Vector2.zero)
                {
                    Vector2 perpCCW = new Vector2(-dir.y, dir.x);
                    tangent = perpCCW * (sign * tangentMag);
                }
            }
        }
    }
}

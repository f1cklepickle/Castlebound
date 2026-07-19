using UnityEngine;

public class EnemyApproachSpread : MonoBehaviour
{
    [SerializeField] private float neighborSeparationRadius = 1.5f;
    [SerializeField] private float separationStrength = 0.8f;
    [SerializeField] private float maxLateralRatio = 0.35f;
    [SerializeField] private float minimumForwardRatio = 0.8f;
    [SerializeField] private float surroundArrivalDistance = 10f;
    [SerializeField] private float maxAngularArrivalRatio = 0.18f;
    [SerializeField] private float openGapDeadbandDegrees = 8f;

    public float NeighborSeparationRadius => Mathf.Max(0f, neighborSeparationRadius);

    public void Compute(Vector2 pursuit, Vector2 directionToTarget, Vector2 localSeparation,
        bool hasNeighbors, Vector2 stableBias, float distance, float holdRadius,
        float gapCW, float gapCCW, bool hasGroup, float speed,
        out Vector2 radial, out Vector2 tangent)
    {
        ComputeApproach(pursuit, directionToTarget, localSeparation, hasNeighbors, stableBias,
            distance, holdRadius, gapCW, gapCCW, hasGroup, speed,
            separationStrength, maxLateralRatio, minimumForwardRatio,
            surroundArrivalDistance, maxAngularArrivalRatio,
            openGapDeadbandDegrees * Mathf.Deg2Rad, out radial, out tangent);
    }

    public static void ComputeApproach(Vector2 pursuit, Vector2 directionToTarget,
        Vector2 localSeparation, bool hasNeighbors, Vector2 stableBias, float speed,
        float separationStrength, float maxLateralRatio, float minimumForwardRatio,
        out Vector2 radial, out Vector2 tangent)
    {
        ComputeApproach(pursuit, directionToTarget, localSeparation, hasNeighbors, stableBias,
            float.PositiveInfinity, 0f, 0f, 0f, false, speed,
            separationStrength, maxLateralRatio, minimumForwardRatio,
            0f, 0f, 0f, out radial, out tangent);
    }

    public static void ComputeApproach(Vector2 pursuit, Vector2 directionToTarget,
        Vector2 localSeparation, bool hasNeighbors, Vector2 stableBias,
        float distance, float holdRadius, float gapCW, float gapCCW, bool hasGroup, float speed,
        float separationStrength, float maxLateralRatio, float minimumForwardRatio,
        float surroundArrivalDistance, float maxAngularArrivalRatio, float gapDeadbandRadians,
        out Vector2 radial, out Vector2 tangent)
    {
        radial = pursuit;
        tangent = Vector2.zero;
        if (directionToTarget.sqrMagnitude <= 0f) return;

        float safeSpeed = Mathf.Max(0f, speed);
        Vector2 forward = directionToTarget.normalized;
        Vector2 lateralAxis = new Vector2(-forward.y, forward.x);
        float localPreference = 0f;
        if (hasNeighbors)
        {
            Vector2 separation = localSeparation.sqrMagnitude > 0.0001f
                ? localSeparation.normalized
                : stableBias.normalized;
            localPreference = Vector2.Dot(separation, lateralAxis);
        }

        float angularPreference = 0f;
        float arrival01 = 0f;
        float arrivalDistance = Mathf.Max(0.0001f, surroundArrivalDistance);
        if (hasGroup && distance > holdRadius && distance < holdRadius + arrivalDistance)
        {
            arrival01 = 1f - Mathf.Clamp01((distance - holdRadius) / arrivalDistance);
            float gapDifference = gapCCW - gapCW;
            float deadband = Mathf.Max(0.0001f, gapDeadbandRadians);
            if (Mathf.Abs(gapDifference) > deadband)
                angularPreference = Mathf.Sign(gapDifference) *
                                    Mathf.Clamp01((Mathf.Abs(gapDifference) - deadband) / deadband);
            else if (gapCW <= 0f && gapCCW <= 0f)
                angularPreference = Vector2.Dot(stableBias.normalized, lateralAxis) * 0.5f;
        }

        float localLateral = Mathf.Clamp01(separationStrength) *
                             Mathf.Clamp01(maxLateralRatio) * localPreference;
        float angularLateral = Mathf.Clamp01(maxAngularArrivalRatio) * arrival01 * angularPreference;
        float lateralRatio = Mathf.Clamp(localLateral + angularLateral,
            -Mathf.Clamp01(maxLateralRatio), Mathf.Clamp01(maxLateralRatio));
        if (Mathf.Abs(lateralRatio) <= 0.001f) return;

        float lateralMagnitude = safeSpeed * lateralRatio;
        float minimumForward = safeSpeed * Mathf.Clamp01(minimumForwardRatio);
        Vector2 rawRadial = forward * Mathf.Max(minimumForward, Vector2.Dot(pursuit, forward));
        Vector2 rawTangent = lateralAxis * lateralMagnitude;
        Vector2 combined = rawRadial + rawTangent;
        float scale = combined.magnitude > safeSpeed && combined.sqrMagnitude > 0f
            ? safeSpeed / combined.magnitude
            : 1f;

        radial = rawRadial * scale;
        tangent = rawTangent * scale;
    }
}

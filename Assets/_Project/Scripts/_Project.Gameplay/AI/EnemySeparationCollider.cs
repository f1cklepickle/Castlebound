using System.Collections.Generic;
using Castlebound.Gameplay.AI;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D))]
public sealed class EnemySeparationCollider : MonoBehaviour
{
    private const float DirectStackThreshold = 0.001f;
    private const float MaxCorrectionRadiusRatio = 0.25f;
    // Keeps exact-boundary float noise below the threshold for physical recovery.
    private const float PenetrationToleranceContactOffsetRatio = 0.01f;
    private const int ConstraintPassCount = 2;
    private const int CastBufferSize = 8;

    [SerializeField] private CircleCollider2D separationCollider;

    private readonly List<EnemySeparationCollider> overlappingSensors =
        new List<EnemySeparationCollider>(8);
    private readonly RaycastHit2D[] castBuffer = new RaycastHit2D[CastBufferSize];
    private Rigidbody2D body;
    private CircleCollider2D primaryCollider;
    private EnemyRootReceiver rootReceiver;
    private EnemyStaggerReceiver staggerReceiver;
    private ContactFilter2D worldFilter;
    private Vector2 scheduledBodyPosition;
    private float scheduledFixedTime = float.NegativeInfinity;

#if UNITY_EDITOR
    public int DebugFallbackCorrectionCount { get; private set; }
#endif

    public CircleCollider2D Collider
    {
        get
        {
            if (separationCollider == null)
                separationCollider = GetComponent<CircleCollider2D>();

            return separationCollider;
        }
    }

    private void Awake()
    {
        CacheComponents();
        worldFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = BuildWorldBlockerMask(),
            useTriggers = false
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleOverlap(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandleOverlap(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        EnemySeparationCollider sensor = GetSensor(other);
        if (sensor != null && !IsWithinTrackingRange(sensor))
            overlappingSensors.Remove(sensor);
    }

    private void OnDisable()
    {
        for (int i = overlappingSensors.Count - 1; i >= 0; i--)
        {
            EnemySeparationCollider other = overlappingSensors[i];
            if (other != null)
                other.overlappingSensors.Remove(this);
        }

        overlappingSensors.Clear();
    }

    private void HandleOverlap(Collider2D other)
    {
        EnemySeparationCollider sensor = GetSensor(other);
        if (sensor == null || sensor == this || !sensor.isActiveAndEnabled)
            return;

        TrackSensor(sensor);

        if (GetInstanceID() < sensor.GetInstanceID())
            ResolvePair(sensor);
    }

    private void ResolvePair(EnemySeparationCollider other)
    {
        CacheComponents();
        other.CacheComponents();
        if (!CanResolve(body) || !CanResolve(other.body))
            return;

        CircleCollider2D ownSensor = Collider;
        CircleCollider2D otherSensor = other.Collider;
        Vector2 centerDelta = otherSensor.bounds.center - ownSensor.bounds.center;
        float centerDistance = centerDelta.magnitude;
        float ownRadius = ownSensor.bounds.extents.x;
        float otherRadius = otherSensor.bounds.extents.x;
        float penetration = ownRadius + otherRadius - centerDistance;
        if (penetration <= GetPenetrationTolerance())
            return;

        bool ownLocked = IsMovementLocked();
        bool otherLocked = other.IsMovementLocked();
        if (ownLocked && otherLocked)
            return;

        Vector2 axis = EnemySeparationMath.ResolveAxis(
            centerDelta,
            DirectStackThreshold);
        float maxCorrection = Mathf.Min(ownRadius, otherRadius) *
            MaxCorrectionRadiusRatio;

        float ownCapacity = ownLocked
            ? 0f
            : GetWorldSafeDistance(-axis, Mathf.Min(penetration, maxCorrection));
        float otherCapacity = otherLocked
            ? 0f
            : other.GetWorldSafeDistance(axis, Mathf.Min(penetration, maxCorrection));

        EnemySeparationMath.AllocateCorrections(
            penetration,
            ownCapacity,
            otherCapacity,
            out float ownCorrection,
            out float otherCorrection);

        if (ownCorrection > 0f)
            body.position -= axis * ownCorrection;
        if (otherCorrection > 0f)
            other.body.position += axis * otherCorrection;

#if UNITY_EDITOR
        if (ownCorrection > 0f || otherCorrection > 0f)
            DebugFallbackCorrectionCount++;
#endif
    }

    public Vector2 ConstrainLocomotionDisplacement(Vector2 proposedDisplacement)
    {
        CacheComponents();
        if (body == null || proposedDisplacement.sqrMagnitude <= Mathf.Epsilon)
            return proposedDisplacement;

        Vector2 constrainedDisplacement = proposedDisplacement;
        Vector2 ownCenter = Collider.bounds.center;
        for (int i = overlappingSensors.Count - 1; i >= 0; i--)
        {
            EnemySeparationCollider other = overlappingSensors[i];
            if (other == null || !other.isActiveAndEnabled ||
                !IsWithinTrackingRange(other))
            {
                overlappingSensors.RemoveAt(i);
            }
        }

        for (int pass = 0; pass < ConstraintPassCount; pass++)
        {
            for (int i = 0; i < overlappingSensors.Count; i++)
            {
                EnemySeparationCollider other = overlappingSensors[i];
                other.CacheComponents();
                if (other.body == null)
                    continue;

                Vector2 otherCenter = other.GetScheduledSensorCenter();
                float minimumDistance = Collider.bounds.extents.x +
                    other.Collider.bounds.extents.x + GetPenetrationTolerance();
                constrainedDisplacement = EnemySeparationMath.ClampInwardDisplacement(
                    otherCenter - ownCenter,
                    minimumDistance,
                    constrainedDisplacement,
                    DirectStackThreshold);
            }
        }

        return constrainedDisplacement;
    }

    public void RecordScheduledBodyPosition(Vector2 position)
    {
        scheduledBodyPosition = position;
        scheduledFixedTime = Time.fixedTime;
    }

#if UNITY_EDITOR
    public void Debug_ResetFallbackCorrectionCount()
    {
        DebugFallbackCorrectionCount = 0;
    }
#endif

    private Vector2 GetScheduledSensorCenter()
    {
        Vector2 currentCenter = Collider.bounds.center;
        return body != null && scheduledFixedTime == Time.fixedTime
            ? currentCenter + scheduledBodyPosition - body.position
            : currentCenter;
    }

    private bool IsWithinTrackingRange(EnemySeparationCollider other)
    {
        if (other == null)
            return false;

        CircleCollider2D ownSensor = Collider;
        CircleCollider2D otherSensor = other.Collider;
        float trackingDistance = ownSensor.bounds.extents.x +
            otherSensor.bounds.extents.x + Physics2D.defaultContactOffset;
        return Vector2.Distance(ownSensor.bounds.center, otherSensor.bounds.center) <=
            trackingDistance;
    }

    private static float GetPenetrationTolerance()
    {
        return Physics2D.defaultContactOffset *
            PenetrationToleranceContactOffsetRatio;
    }

    private void TrackSensor(EnemySeparationCollider sensor)
    {
        int sensorId = sensor.GetInstanceID();
        for (int i = 0; i < overlappingSensors.Count; i++)
        {
            EnemySeparationCollider tracked = overlappingSensors[i];
            if (tracked == sensor)
                return;
            if (tracked == null || tracked.GetInstanceID() > sensorId)
            {
                overlappingSensors.Insert(i, sensor);
                return;
            }
        }

        overlappingSensors.Add(sensor);
    }

    private float GetWorldSafeDistance(Vector2 direction, float requestedDistance)
    {
        if (requestedDistance <= 0f || primaryCollider == null ||
            !primaryCollider.enabled || worldFilter.layerMask == 0)
        {
            return Mathf.Max(0f, requestedDistance);
        }

        int hitCount = primaryCollider.Cast(
            direction,
            worldFilter,
            castBuffer,
            requestedDistance);
        float allowedDistance = requestedDistance;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = castBuffer[i];
            if (hit.collider == null)
                continue;

            allowedDistance = Mathf.Min(
                allowedDistance,
                Mathf.Max(0f, hit.distance - Physics2D.defaultContactOffset));
        }

        return allowedDistance;
    }

    private bool IsMovementLocked()
    {
        if (rootReceiver == null && body != null)
            rootReceiver = body.GetComponent<EnemyRootReceiver>();
        if (staggerReceiver == null && body != null)
            staggerReceiver = body.GetComponent<EnemyStaggerReceiver>();

        return (rootReceiver != null && rootReceiver.IsRooted) ||
               (staggerReceiver != null && staggerReceiver.IsActionLocked);
    }

    private void CacheComponents()
    {
        CircleCollider2D sensor = Collider;
        if (body == null)
            body = sensor.attachedRigidbody;
        if (primaryCollider == null && body != null)
            primaryCollider = body.GetComponent<CircleCollider2D>();
        if (rootReceiver == null && body != null)
            rootReceiver = body.GetComponent<EnemyRootReceiver>();
        if (staggerReceiver == null && body != null)
            staggerReceiver = body.GetComponent<EnemyStaggerReceiver>();
    }

    private static EnemySeparationCollider GetSensor(Collider2D candidate)
    {
        return candidate != null
            ? candidate.GetComponent<EnemySeparationCollider>()
            : null;
    }

    private static bool CanResolve(Rigidbody2D candidate)
    {
        return candidate != null &&
               candidate.simulated &&
               candidate.bodyType == RigidbodyType2D.Dynamic;
    }

    private static LayerMask BuildWorldBlockerMask()
    {
        int mask = 0;
        AddLayer(ref mask, "Player");
        AddLayer(ref mask, "Walls");
        AddLayer(ref mask, "Barriers");
        AddLayer(ref mask, "Gates");
        AddLayer(ref mask, "Environment");
        return mask;
    }

    private static void AddLayer(ref int mask, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer >= 0)
            mask |= 1 << layer;
    }
}

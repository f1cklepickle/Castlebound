using System.Collections.Generic;
using UnityEngine;
using Castlebound.Gameplay.AI;

// Local, CHASE-only experiment. No shared assignments or changes to the attack target.
public sealed class EnemyChaseApproachTarget
{
    public readonly struct Footprint
    {
        public readonly Vector2 Center;
        public readonly float Radius;
        public Footprint(Vector2 center, float radius) { Center = center; Radius = radius; }
    }

    public const float MaximumBypassAngleDegrees = 100f;
    private const float ApproachClearanceRatio = 1.1f;
    private const float ReleaseClearanceRatio = 1.35f;
    private const float TangentClearanceRadians = 0.01f;
    private readonly List<Footprint> nearby = new List<Footprint>(16);
    private int previousPlayerId;

    public bool IsActive => Side != 0;
    public bool WaypointChanged { get; private set; }
    // Positive is counter-clockwise around the Player, negative is clockwise.
    public int Side { get; private set; }
    public Vector2 Offset { get; private set; }

    public void Reset()
    {
        WaypointChanged = false;
        previousPlayerId = 0;
        Side = 0;
        Offset = Vector2.zero;
    }

    public bool TryGetTarget(EnemyController2D subject, Transform player, Vector2 position,
        Vector2 stableBias, float surfaceDistance, float engagementDistance, out Vector2 target)
    {
        target = position;
        float range = subject != null ? subject.ApproachSeparationRadius : 0f;
        if (subject == null || player == null || range <= 0f ||
            surfaceDistance > engagementDistance + 2f * range ||
            !TryGetFootprint(subject, out Footprint own))
        {
            Reset();
            return false;
        }

        nearby.Clear();
        foreach (var other in EnemyController2D.All)
        {
            if (other == null || other == subject || !other.isActiveAndEnabled ||
                other.CurrentTargetType != EnemyTargetType.Player || other.Target != player ||
                ((Vector2)other.transform.position - position).sqrMagnitude > range * range)
                continue;
            var health = other.GetComponent<Health>();
            if (health != null && health.Current <= 0) continue;
            if (TryGetFootprint(other, out Footprint footprint)) nearby.Add(footprint);
        }

        Vector2 centerOffset = own.Center - position;
        bool active = TryGetTarget(own.Center, player.position, player.GetInstanceID(), own.Radius,
            range, surfaceDistance, engagementDistance, stableBias, nearby, out target);
        // Convert the sensor-center waypoint to a body movement point.
        target -= centerOffset;
        return active;
    }

    public bool TryGetTarget(Vector2 position, Vector2 player, int playerId, float ownRadius,
        float neighborRange, float surfaceDistance, float engagementDistance, Vector2 stableBias,
        IReadOnlyList<Footprint> neighbors, out Vector2 target)
    {
        WaypointChanged = false;
        target = player;
        if (previousPlayerId != playerId) Reset();
        if (ownRadius <= 0f || neighborRange <= 0f ||
            surfaceDistance <= engagementDistance ||
            surfaceDistance > engagementDistance + 2f * neighborRange)
        {
            Reset();
            return false;
        }

        Vector2 inward = (player - position).normalized;
        Vector2 ccw = new Vector2(inward.y, -inward.x);
        Vector2 directTravel = inward * Mathf.Min(neighborRange, surfaceDistance - engagementDistance);
        float clearance = IsActive ? ReleaseClearanceRatio : ApproachClearanceRatio;
        bool occupied = false;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var other = neighbors[i];
            if (!IsCloserNeighbor(other, position, player, neighborRange)) continue;
            occupied |= Intersects(position, directTravel, other.Center,
                (ownRadius + other.Radius) * clearance);
        }
        if (!occupied)
        {
            Reset();
            return false;
        }

        target = player + Offset;
        // Revalidate retained Player-relative points too: movement can leave one behind us.
        bool needsPoint = !IsActive || (target - position).sqrMagnitude < ownRadius * ownRadius * 0.0625f ||
            Vector2.Angle(inward, target - position) > MaximumBypassAngleDegrees + 0.001f;
        if (!needsPoint)
        {
            // Also replace a stored point if a moving/new ally obstructs that path.
            for (int i = 0; i < neighbors.Count; i++)
            {
                var other = neighbors[i];
                if (!IsCloserNeighbor(other, position, player, neighborRange)) continue;
                needsPoint |= Intersects(position, target - position, other.Center,
                    (ownRadius + other.Radius) * ApproachClearanceRatio);
            }
        }
        if (needsPoint)
        {
            float ccwAngle = RequiredTurn(1, position, player, inward, ccw, ownRadius, neighborRange, neighbors);
            float cwAngle = RequiredTurn(-1, position, player, inward, ccw, ownRadius, neighborRange, neighbors);
            if (!IsActive)
            {
                Side = Mathf.Abs(ccwAngle - cwAngle) <= TangentClearanceRadians
                    ? (Vector2.Dot(stableBias, ccw) >= 0f ? 1 : -1)
                    : (ccwAngle < cwAngle ? 1 : -1);
            }
            // Keep genuine lateral clearing, with at most ten degrees of outward travel.
            // If this bounded route is obstructed, #277 still owns physical separation.
            float angle = Mathf.Min(MaximumBypassAngleDegrees * Mathf.Deg2Rad,
                (Side > 0 ? ccwAngle : cwAngle) + TangentClearanceRadians);
            Vector2 direction = inward * Mathf.Cos(angle) + ccw * (Side * Mathf.Sin(angle));
            target = position + direction * neighborRange;
            Offset = target - player;
            WaypointChanged = true;
        }
        previousPlayerId = playerId;
        return true;
    }

    private static float RequiredTurn(int side, Vector2 position, Vector2 player,
        Vector2 inward, Vector2 ccw, float ownRadius, float range, IReadOnlyList<Footprint> neighbors)
    {
        float angle = 0f;
        for (int i = 0; i < neighbors.Count; i++)
        {
            var other = neighbors[i];
            if (!IsCloserNeighbor(other, position, player, range)) continue;
            Vector2 delta = other.Center - position;
            float bearing = Mathf.Atan2(Vector2.Dot(delta, ccw), Vector2.Dot(delta, inward));
            float radius = (ownRadius + other.Radius) * ApproachClearanceRatio;
            // Tangent to an expanded footprint; at contact, turn at least perpendicular to it.
            float halfAngle = Mathf.Asin(Mathf.Clamp01(radius / Mathf.Max(delta.magnitude, 0.0001f)));
            angle = Mathf.Max(angle, side * bearing + halfAngle);
        }
        return angle;
    }

    private static bool IsCloserNeighbor(Footprint other, Vector2 position, Vector2 player, float range)
    {
        return other.Radius > 0f && (other.Center - position).sqrMagnitude <= range * range &&
               (other.Center - player).sqrMagnitude < (position - player).sqrMagnitude;
    }

    private static bool Intersects(Vector2 position, Vector2 travel, Vector2 center, float radius)
    {
        Vector2 delta = center - position;
        // Already inside the clearance margin: allow movement out, never further in.
        if (delta.sqrMagnitude <= radius * radius)
            return Vector2.Dot(travel, delta) > 0.000001f;
        if (travel.sqrMagnitude <= 0f) return false;
        float t = Mathf.Clamp01(Vector2.Dot(delta, travel) / travel.sqrMagnitude);
        return (delta - travel * t).sqrMagnitude < radius * radius - 0.000001f;
    }

    private static bool TryGetFootprint(EnemyController2D controller, out Footprint footprint)
    {
        var sensor = controller.GetComponentInChildren<EnemySeparationCollider>();
        var circle = sensor != null && sensor.isActiveAndEnabled
            ? sensor.Collider : controller.GetComponent<CircleCollider2D>();
        if (circle != null && circle.enabled && circle.gameObject.activeInHierarchy && circle.bounds.extents.x > 0f)
        {
            footprint = new Footprint(circle.bounds.center, circle.bounds.extents.x);
            return true;
        }
        footprint = default;
        return false;
    }
}

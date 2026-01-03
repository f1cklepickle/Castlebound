using UnityEngine;
using Castlebound.Gameplay.AI;

public static class BarrierOverlapResolver
{
    public static bool IsMostlyInside(Collider2D barrier, Collider2D actor)
    {
        if (barrier == null || actor == null) return false;

        Vector2 center = actor.bounds.center;
        Vector2 extents = actor.bounds.extents;

        float radius = 0f;
        if (actor is CircleCollider2D circle)
        {
            float scale = Mathf.Max(actor.transform.lossyScale.x, actor.transform.lossyScale.y);
            radius = circle.radius * scale;
        }

        Vector2 right = radius > 0f ? new Vector2(radius, 0f) : new Vector2(extents.x, 0f);
        Vector2 up = radius > 0f ? new Vector2(0f, radius) : new Vector2(0f, extents.y);

        Vector2[] points =
        {
            center,
            center + right,
            center - right,
            center + up,
            center - up
        };

        int insideCount = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (barrier.OverlapPoint(points[i]))
                insideCount++;
        }

        return insideCount >= 3;
    }

    public static void ResolveOverlap(Collider2D barrier, Collider2D actor, bool isPlayer)
    {
        if (barrier == null || actor == null) return;

        Vector2 outward = Vector2.zero;
        Vector2 anchorPos = Vector2.zero;
        var hold = barrier.GetComponent<EnemyBarrierHoldBehavior>();
        if (hold != null)
        {
            anchorPos = hold.Debug_GetAnchorPosition();
            Vector2 barrierPos = barrier.bounds.center;
            Vector2 dir = anchorPos - barrierPos;
            if (dir.sqrMagnitude > 0.0001f)
            {
                outward = dir.normalized;
            }
        }

        bool mostlyInside = IsMostlyInside(barrier, actor);
        bool pushIntoBarrier = isPlayer || mostlyInside;
        if (!isPlayer && outward.sqrMagnitude > 0.0001f)
        {
            pushIntoBarrier = IsEnemyPastAnchorThreshold(barrier, actor, anchorPos, -outward);
        }

        Vector2 desiredDir = outward.sqrMagnitude > 0.0001f
            ? (pushIntoBarrier ? -outward : outward)
            : Vector2.zero;

        const float skin = 0.01f;
        const int maxIterations = 3;

        if (isPlayer)
        {
            if (desiredDir == Vector2.zero)
            {
                return;
            }

            for (int i = 0; i < maxIterations; i++)
            {
                Physics2D.SyncTransforms();
                ColliderDistance2D dist = Physics2D.Distance(barrier, actor);
                if (!dist.isOverlapped)
                {
                    break;
                }

                Vector2 dir = desiredDir.normalized;
                float barrierExtent = Mathf.Abs(Vector2.Dot(barrier.bounds.extents, dir));
                float actorExtent = Mathf.Abs(Vector2.Dot(actor.bounds.extents, dir));
                Vector2 target = (Vector2)barrier.bounds.center + dir * (barrierExtent + actorExtent + skin);
                SetPositionImmediate(actor, target);
            }
        }
        else
        {
            if (desiredDir == Vector2.zero)
            {
                for (int i = 0; i < maxIterations; i++)
                {
                    Physics2D.SyncTransforms();

                    ColliderDistance2D dist = Physics2D.Distance(barrier, actor);
                    if (!dist.isOverlapped)
                    {
                        return;
                    }

                    Vector2 normal = dist.normal;
                    Vector2 separation = normal * (-dist.distance + skin);
                    ApplyDelta(actor, separation);
                }

                return;
            }

            for (int i = 0; i < maxIterations; i++)
            {
                Physics2D.SyncTransforms();
                ColliderDistance2D dist = Physics2D.Distance(barrier, actor);
                if (!dist.isOverlapped)
                {
                    return;
                }

                Vector2 dir = desiredDir.normalized;
                float barrierExtent = Mathf.Abs(Vector2.Dot(barrier.bounds.extents, dir));
                float actorExtent = Mathf.Abs(Vector2.Dot(actor.bounds.extents, dir));
                Vector2 target = (Vector2)barrier.bounds.center + dir * (barrierExtent + actorExtent + skin);
                SetPositionImmediate(actor, target);
            }
        }
    }

    private static void ApplyDelta(Collider2D actor, Vector2 delta)
    {
        if (delta == Vector2.zero)
        {
            return;
        }

        var rb = actor.attachedRigidbody;
        if (rb != null)
        {
            rb.MovePosition(rb.position + delta);
        }
        else
        {
            actor.transform.position = (Vector2)actor.transform.position + delta;
        }
    }

    private static void SetPositionImmediate(Collider2D actor, Vector2 position)
    {
        var rb = actor.attachedRigidbody;
        if (rb != null)
        {
            rb.position = position;
        }
        else
        {
            actor.transform.position = position;
        }

        Physics2D.SyncTransforms();
    }

    private static bool IsEnemyPastAnchorThreshold(
        Collider2D barrier,
        Collider2D actor,
        Vector2 anchorPos,
        Vector2 inwardDir)
    {
        var barrierHealth = barrier.GetComponent<BarrierHealth>();
        if (barrierHealth == null)
        {
            return false;
        }

        Vector2 dir = inwardDir.normalized;
        float actorExtent = Mathf.Abs(Vector2.Dot(actor.bounds.extents, dir));
        float required = barrierHealth.EnemyPushInDistance + actorExtent;
        float signed = Vector2.Dot((Vector2)actor.bounds.center - anchorPos, dir);

        return signed >= required;
    }
}

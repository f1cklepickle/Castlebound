using UnityEngine;

public static class PlayerSweepSlideMath
{
    private const float NormalDuplicateDot = 0.9999f;
    private const float MinApproach = 0.0001f;

    public static Vector2 GetWorldCenterOffset(
        Vector2 colliderOffset,
        Vector3 lossyScale,
        float rotationDegrees)
    {
        Vector2 scaledOffset = new Vector2(
            colliderOffset.x * lossyScale.x,
            colliderOffset.y * lossyScale.y);
        float radians = rotationDegrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            scaledOffset.x * cos - scaledOffset.y * sin,
            scaledOffset.x * sin + scaledOffset.y * cos);
    }

    public static float GetWorldRadius(float colliderRadius, Vector3 lossyScale)
    {
        float largestAxis = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
        return Mathf.Max(0f, colliderRadius) * largestAxis;
    }

    public static void AddSortedUniqueNormal(Vector2[] normals, ref int count, Vector2 normal)
    {
        if (normals == null || count >= normals.Length || normal.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        normal.Normalize();
        for (int i = 0; i < count; i++)
        {
            if (Vector2.Dot(normals[i], normal) >= NormalDuplicateDot)
            {
                return;
            }
        }

        float angle = Mathf.Atan2(normal.y, normal.x);
        int insertIndex = count;
        while (insertIndex > 0)
        {
            Vector2 previous = normals[insertIndex - 1];
            float previousAngle = Mathf.Atan2(previous.y, previous.x);
            if (previousAngle <= angle)
            {
                break;
            }

            normals[insertIndex] = previous;
            insertIndex--;
        }

        normals[insertIndex] = normal;
        count++;
    }

    public static Vector2 ClipAgainstNormals(Vector2 displacement, Vector2[] normals, int count)
    {
        if (normals == null || count <= 0)
        {
            return displacement;
        }

        Vector2 clipped = displacement;
        int validCount = Mathf.Min(count, normals.Length);

        // Revisit earlier constraints after each projection. In 2D this converges quickly,
        // and the angle-sorted input keeps the result deterministic at corners.
        for (int pass = 0; pass < validCount; pass++)
        {
            bool changed = false;
            for (int i = 0; i < validCount; i++)
            {
                float inward = Vector2.Dot(clipped, normals[i]);
                if (inward >= 0f)
                {
                    continue;
                }

                clipped -= normals[i] * inward;
                changed = true;
            }

            if (!changed)
            {
                break;
            }
        }

        return clipped;
    }

    public static float GetAllowedTravelDistance(
        float contactDistance,
        float requestedDistance,
        float skin,
        Vector2 direction,
        Vector2[] normals,
        int count)
    {
        float retreatDistance = 0f;
        int validCount = normals == null ? 0 : Mathf.Min(count, normals.Length);
        for (int i = 0; i < validCount; i++)
        {
            float approach = -Vector2.Dot(direction, normals[i]);
            if (approach > MinApproach)
            {
                retreatDistance = Mathf.Max(retreatDistance, skin / approach);
            }
        }

        if (validCount == 0)
        {
            retreatDistance = skin;
        }

        return Mathf.Clamp(contactDistance - retreatDistance, 0f, requestedDistance);
    }
}

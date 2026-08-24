using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public static class EnemySeparationMath
    {
        public static Vector2 ClampInwardDisplacement(
            Vector2 centerDelta,
            float minimumDistance,
            Vector2 proposedDisplacement,
            float usableAxisThreshold)
        {
            float centerDistance = centerDelta.magnitude;
            if (centerDistance <= usableAxisThreshold)
                return proposedDisplacement;

            Vector2 inwardAxis = centerDelta / centerDistance;
            float proposedInwardDistance = Vector2.Dot(
                proposedDisplacement,
                inwardAxis);
            if (proposedInwardDistance <= 0f)
                return proposedDisplacement;

            Vector2 tangentialDisplacement = proposedDisplacement -
                inwardAxis * proposedInwardDistance;
            float requiredRadialDistance = Mathf.Sqrt(Mathf.Max(
                0f,
                minimumDistance * minimumDistance -
                tangentialDisplacement.sqrMagnitude));
            float allowedInwardDistance = Mathf.Max(
                0f,
                centerDistance - requiredRadialDistance);
            float violatingDistance = proposedInwardDistance - allowedInwardDistance;
            return violatingDistance > 0f
                ? proposedDisplacement - inwardAxis * violatingDistance
                : proposedDisplacement;
        }

        public static Vector2 ResolveAxis(Vector2 centerDelta, float directStackThreshold)
        {
            float distance = centerDelta.magnitude;
            return distance > directStackThreshold
                ? centerDelta / distance
                : Vector2.right;
        }

        public static void AllocateCorrections(
            float penetration,
            float ownCapacity,
            float otherCapacity,
            out float ownCorrection,
            out float otherCorrection)
        {
            penetration = Mathf.Max(0f, penetration);
            ownCapacity = Mathf.Max(0f, ownCapacity);
            otherCapacity = Mathf.Max(0f, otherCapacity);

            ownCorrection = Mathf.Min(penetration * 0.5f, ownCapacity);
            otherCorrection = Mathf.Min(penetration * 0.5f, otherCapacity);
            float remaining = penetration - ownCorrection - otherCorrection;
            AllocateRemaining(ref ownCorrection, ownCapacity, ref remaining);
            AllocateRemaining(ref otherCorrection, otherCapacity, ref remaining);
        }

        private static void AllocateRemaining(
            ref float correction,
            float capacity,
            ref float remaining)
        {
            if (remaining <= 0f)
                return;

            float additional = Mathf.Min(remaining, capacity - correction);
            correction += additional;
            remaining -= additional;
        }
    }
}

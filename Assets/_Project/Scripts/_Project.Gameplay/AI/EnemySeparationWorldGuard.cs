using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    // Only returns a prefix of the input, preserving the solver's swept certificate.
    public sealed class EnemySeparationWorldGuard
    {
        private readonly CircleCollider2D primary;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];
        private readonly ContactFilter2D filter;
        public EnemySeparationWorldGuard(CircleCollider2D primary)
        {
            this.primary = primary;
            filter = new ContactFilter2D { useLayerMask = true, useTriggers = false,
                layerMask = LayerMask.GetMask("Default", "Player", "Walls", "Barriers", "Gates", "Environment") };
        }
        public Vector2 Constrain(Vector2 displacement)
        {
            float distance = displacement.magnitude;
            if (distance <= 0f) return Vector2.zero;
            if (primary == null || !primary.enabled) return Vector2.zero;
            Vector2 direction = displacement / distance;
            int count = primary.Cast(direction, filter, hits, distance);
            if (count == hits.Length) return Vector2.zero;
            float allowed = distance;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider == null) continue;
                // Cast travel and signed collider distance have different contact-skin
                // semantics. Preserve the signed starting gap as well as the cast bound.
                ColliderDistance2D gap = primary.Distance(hits[i].collider);
                if (!gap.isValid) return Vector2.zero;
                Vector2 outward = (Vector2)primary.bounds.center - hits[i].collider.ClosestPoint(primary.bounds.center);
                if (outward.sqrMagnitude <= 0f) return Vector2.zero;
                float inward = -Vector2.Dot(direction, outward.normalized);
                if (inward > 0f)
                    allowed = Mathf.Min(allowed, Mathf.Max(0f, gap.distance - Physics2D.defaultContactOffset) / inward);
                if (hits[i].distance <= 0f && Vector2.Dot(direction, hits[i].normal) >= 0f) continue;
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hits[i].distance - Physics2D.defaultContactOffset));
            }
            return direction * allowed;
        }
    }
}

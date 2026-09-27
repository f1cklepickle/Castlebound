using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public readonly struct EnemyNavigationTarget
    {
        public readonly Transform Transform;
        public readonly EnemyTargetType Type;
        public readonly Collider2D[] Colliders;
        public readonly float EngagementDistance;
        public readonly bool Passage, ExteriorOnly;
        public readonly Vector2 Anchor;
        public readonly bool HasAnchor;
        public bool IsValid => Transform != null && Transform.gameObject.activeInHierarchy;

        public EnemyNavigationTarget(Transform target, EnemyTargetType type, Collider2D[] colliders,
            float engagementDistance, bool passage = false, bool exteriorOnly = false,
            Vector2 anchor = default, bool hasAnchor = false)
        {
            Transform = target; Type = type; Colliders = colliders;
            EngagementDistance = engagementDistance; Passage = passage; ExteriorOnly = exteriorOnly;
            Anchor = anchor; HasAnchor = hasAnchor;
        }
    }
}

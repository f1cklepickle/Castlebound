using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    /// <summary>
    /// Simple singleton wrapper around a PolygonCollider2D that represents
    /// the castle interior. Enemies can query whether a position is inside.
    /// </summary>
    [RequireComponent(typeof(PolygonCollider2D))]
    public class CastleRegionDetector : MonoBehaviour
    {
        public static CastleRegionDetector Instance { get; private set; }

        PolygonCollider2D region;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple CastleRegionDetector instances found. Using the first one.");
                return;
            }

            Instance = this;
            region = GetComponent<PolygonCollider2D>();
        }

        public bool IsInside(Vector2 point)
        {
            if (region == null) return false;
            return region.OverlapPoint(point);
        }
    }
}

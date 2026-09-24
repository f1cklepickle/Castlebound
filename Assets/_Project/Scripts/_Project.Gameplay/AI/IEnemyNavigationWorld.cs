using UnityEngine;

namespace Castlebound.Gameplay.AI
{
    public interface IEnemyNavigationWorld
    {
        StaticNavigationWorldCache Cache { get; }
        float BodyRadius { get; }
        StaticNavigationSampleState Point(Vector2 point);
        StaticNavigationSampleState Segment(Vector2 start, Vector2 end);
        StaticNavigationSampleState Sight(Vector2 start, Vector2 end);
        StaticNavigationSampleState Connect(Vector2 start, out Vector2Int cell);
        bool TrySubmit(Vector2Int start, Vector2Int goal, int attempt, out int id);
        bool TryResult(int id, out StaticNavigationPathResult result);
        void Abandon(int id);
    }
}

using UnityEngine;

namespace Castlebound.Gameplay.Inventory
{
    public static class BackpackDropDirectionResolver
    {
        public static Vector3 ResolveVisualForward(Transform origin)
        {
            if (origin == null)
            {
                return Vector3.down;
            }

            // PlayerMovementOrchestrator uses transform.up for logical facing; current player art
            // and attack/drop presentation face visually along -up.
            Vector3 forward = -origin.up;
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.down;
        }
    }
}

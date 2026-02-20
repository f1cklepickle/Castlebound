using System.Collections.Generic;
using UnityEngine;

namespace Castlebound.Gameplay.Castle
{
    public static class BarrierAssemblyBuilder
    {
        public static void Rebuild(Transform parent, GameObject barrierPrefab, IEnumerable<BarrierPlacementSlot> slots)
        {
            if (parent == null || barrierPrefab == null || slots == null)
            {
                return;
            }

            var existingById = new Dictionary<string, Transform>();
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (!existingById.ContainsKey(child.name))
                {
                    existingById.Add(child.name, child);
                }
            }

            var seenIds = new HashSet<string>();

            foreach (var slot in slots)
            {
                if (slot == null || string.IsNullOrWhiteSpace(slot.Id))
                {
                    continue;
                }

                var worldPos = SnapToLattice(slot.Position);
                if (!existingById.TryGetValue(slot.Id, out var instance))
                {
                    var spawned = Object.Instantiate(barrierPrefab, parent);
                    spawned.name = slot.Id;
                    instance = spawned.transform;
                    existingById[slot.Id] = instance;
                }

                instance.SetParent(parent, true);
                instance.position = new Vector3(worldPos.x, worldPos.y, instance.position.z);

                var visualBinder = instance.GetComponent<BarrierVisualBinder>();
                if (visualBinder != null)
                {
                    visualBinder.ApplySide(slot.Side);
                }

                seenIds.Add(slot.Id);
            }

            foreach (var kvp in existingById)
            {
                if (!seenIds.Contains(kvp.Key) && kvp.Value != null)
                {
                    Object.DestroyImmediate(kvp.Value.gameObject);
                }
            }
        }

        private static Vector2 SnapToLattice(Vector2 worldPosition)
        {
            const float step = 3f;
            return new Vector2(
                Mathf.Round(worldPosition.x / step) * step,
                Mathf.Round(worldPosition.y / step) * step);
        }
    }
}

using System;
using UnityEngine;

[Serializable]
public class RepairSensor
{
    [SerializeField] private float repairRadius = 2f;
    [SerializeField] private LayerMask barrierMask;

    /// <summary>
    /// Returns true if there is at least one broken barrier within repair range.
    /// </summary>
    public bool HasRepairableBarrierInRange(Vector2 position)
    {
        var hits = Physics2D.OverlapCircleAll(position, repairRadius, barrierMask);
        if (hits == null) return false;
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (!hit) continue;
            var barrier = hit.GetComponentInParent<BarrierHealth>();
            if (barrier != null && barrier.IsBroken) return true;
        }
        return false;
    }

    /// <summary>
    /// Finds the first broken barrier within repair range and repairs it.
    /// </summary>
    public void TryRepairNearest(Vector2 position)
    {
        var hits = Physics2D.OverlapCircleAll(position, repairRadius, barrierMask);
        if (hits == null || hits.Length == 0)
            return;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (!hit) continue;

            var barrier = hit.GetComponentInParent<BarrierHealth>();
            if (barrier == null)
                continue;

            if (!barrier.IsBroken)
                continue;

            barrier.Repair();
            break; // Repair one barrier per key press.
        }
    }
}

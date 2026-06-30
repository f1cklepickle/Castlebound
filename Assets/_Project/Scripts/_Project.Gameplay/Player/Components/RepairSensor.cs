using System;
using UnityEngine;

[Serializable]
public class RepairSensor
{
    [SerializeField] private float repairRadius = 2f;
    [SerializeField] private LayerMask barrierMask;

    public float RepairRadius
    {
        get => repairRadius;
        set => repairRadius = Mathf.Max(0f, value);
    }

    public LayerMask BarrierMask
    {
        get => barrierMask;
        set => barrierMask = value;
    }

    /// <summary>
    /// Returns true if there is at least one damaged barrier within repair range.
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
            if (barrier != null && barrier.CanRepair) return true;
        }
        return false;
    }

    /// <summary>
    /// Finds the first damaged barrier within repair range and repairs it.
    /// </summary>
    public bool TryRepairNearest(Vector2 position)
    {
        var hits = Physics2D.OverlapCircleAll(position, repairRadius, barrierMask);
        if (hits == null || hits.Length == 0)
            return false;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (!hit) continue;

            var barrier = hit.GetComponentInParent<BarrierHealth>();
            if (barrier == null)
                continue;

            if (!barrier.CanRepair)
                continue;

            return barrier.Repair();
        }

        return false;
    }
}
